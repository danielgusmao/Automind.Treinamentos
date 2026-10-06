using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class StorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly StorageOptions _options;

    public StorageService(IWebHostEnvironment env, IOptions<StorageOptions> options)
    {
        _env = env;
        _options = options.Value;
    }

    public string RootPath => Path.IsPathRooted(_options.RootPath)
        ? _options.RootPath
        : Path.GetFullPath(Path.Combine(_env.ContentRootPath, _options.RootPath));

    public string DataPath => Path.Combine(RootPath, "Data");
    public string TrainingsPath => Path.Combine(RootPath, "Treinamentos");
    public string EvidencePath => Path.Combine(RootPath, "Evidencias");
    public string CollaboratorsEvidencePath => Path.Combine(EvidencePath, "Colaboradores");
    public string ReportsPath => Path.Combine(EvidencePath, "Relatorios");
    public string LogsPath => Path.Combine(RootPath, "Logs");
    public string BackupPath => Path.Combine(RootPath, "Backup");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(TrainingsPath);
        Directory.CreateDirectory(CollaboratorsEvidencePath);
        Directory.CreateDirectory(ReportsPath);
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(BackupPath);
    }

    public static string SafeSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        cleaned = string.Join(" ", cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(cleaned) ? "sem-nome" : cleaned;
    }
}
