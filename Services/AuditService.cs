using System.Text.Json;

namespace Automind.Treinamentos.Services;

public sealed class AuditService
{
    private readonly StorageService _storage;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AuditService(StorageService storage)
    {
        _storage = storage;
    }

    public async Task WriteAsync(string action, string status, string actor, object? data = null)
    {
        _storage.EnsureDirectories();
        var record = new
        {
            timestampUtc = DateTime.UtcNow,
            action,
            status,
            actor,
            data
        };
        var line = JsonSerializer.Serialize(record) + Environment.NewLine;
        var path = Path.Combine(_storage.LogsPath, "Audit.jsonl");
        await _gate.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(path, line);
        }
        finally
        {
            _gate.Release();
        }
    }
}
