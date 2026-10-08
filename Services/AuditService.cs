using System.Text.Json;

namespace Automind.Treinamentos.Services;

public sealed class AuditService
{
    private readonly StorageService _storage;
    private readonly int _retentionDays;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateOnly _lastCleanup = DateOnly.MinValue;

    public AuditService(StorageService storage, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _storage = storage;
        _httpContextAccessor = httpContextAccessor;
        _retentionDays = Math.Clamp(configuration.GetValue("Audit:RetentionDays", 365), 30, 3650);
    }

    public async Task WriteAsync(string action, string status, string actor, object? data = null)
    {
        _storage.EnsureDirectories();
        var now = DateTime.UtcNow;
        var record = new
        {
            timestampUtc = now,
            action,
            status,
            actor,
            correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier,
            data
        };
        var line = JsonSerializer.Serialize(record) + Environment.NewLine;
        var path = Path.Combine(_storage.LogsPath, $"Audit-{now:yyyyMMdd}.jsonl");

        await _gate.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(path, line);
            CleanupOldAuditFiles(now);
        }
        finally
        {
            _gate.Release();
        }
    }

    private void CleanupOldAuditFiles(DateTime nowUtc)
    {
        var today = DateOnly.FromDateTime(nowUtc);
        if (_lastCleanup == today) return;
        _lastCleanup = today;

        var cutoff = nowUtc.Date.AddDays(-_retentionDays);
        foreach (var file in Directory.EnumerateFiles(_storage.LogsPath, "Audit-*.jsonl", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var info = new FileInfo(file);
                if (info.LastWriteTimeUtc < cutoff) info.Delete();
            }
            catch
            {
                // Retencao nao pode impedir o registro do evento atual.
            }
        }
    }
}
