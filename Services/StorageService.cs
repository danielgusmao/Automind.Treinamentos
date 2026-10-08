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
        ? Path.GetFullPath(_options.RootPath)
        : Path.GetFullPath(Path.Combine(_env.ContentRootPath, _options.RootPath));

    public string DataPath => Path.Combine(RootPath, "Data");
    public string TrainingsPath => Path.Combine(RootPath, "Treinamentos");
    public string EvidencePath => Path.Combine(RootPath, "Evidencias");
    public string CollaboratorsEvidencePath => Path.Combine(EvidencePath, "Colaboradores");
    public string ReportsPath => Path.Combine(EvidencePath, "Relatorios");
    public string LogsPath => Path.Combine(RootPath, "Logs");

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(TrainingsPath);
        Directory.CreateDirectory(CollaboratorsEvidencePath);
        Directory.CreateDirectory(ReportsPath);
        Directory.CreateDirectory(LogsPath);
        // Backup e mantido por operacao/deploy com identidade administrativa.
        // O processo web nao precisa criar nem modificar esta pasta.
    }

    public string GetTrainingVersionPath(string familySlug, string version)
    {
        var family = ValidatePathSegment(familySlug, nameof(familySlug));
        var ver = ValidatePathSegment(version, nameof(version));
        return CombineUnderRoot(TrainingsPath, family, ver);
    }

    public static string ValidatePathSegment(string value, string fieldName)
    {
        var cleaned = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned is "." or "..")
            throw new InvalidOperationException($"{fieldName} invalido para armazenamento.");
        if (cleaned.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || cleaned.Contains('/') || cleaned.Contains('\\'))
            throw new InvalidOperationException($"{fieldName} contem caracteres invalidos para armazenamento.");
        return cleaned;
    }

    public static string CombineUnderRoot(string root, params string[] segments)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var candidate = Path.GetFullPath(Path.Combine(new[] { normalizedRoot }.Concat(segments).ToArray()));
        if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Caminho calculado saiu da raiz de armazenamento autorizada.");
        return candidate;
    }

    public static string SafeSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string((value ?? string.Empty).Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        cleaned = string.Join(" ", cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (cleaned is "." or "..") cleaned = cleaned.Replace('.', '_');
        return string.IsNullOrWhiteSpace(cleaned) ? "sem-nome" : cleaned;
    }
}
