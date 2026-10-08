using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public sealed class TrainingSnapshotService
{
    private readonly StorageService _storage;

    public TrainingSnapshotService(StorageService storage)
    {
        _storage = storage;
    }

    public async Task<string> WriteSnapshotAsync(Training training, IReadOnlyCollection<TrainingQuestion> questions)
    {
        _storage.EnsureDirectories();
        var family = string.IsNullOrWhiteSpace(training.FamilySlug) ? training.Slug : training.FamilySlug;
        var dir = _storage.GetTrainingVersionPath(family, training.Version);
        Directory.CreateDirectory(dir);

        var manifestPath = Path.Combine(dir, "manifest.json");
        var hashPath = Path.Combine(dir, "SHA256.txt");
        var json = BuildManifestJson(training, questions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = Convert.ToHexString(SHA256.HashData(bytes));

        // Escrita atomica: evita manifest parcial em interrupcao/falha de I/O.
        var manifestTemp = manifestPath + ".tmp";
        var hashTemp = hashPath + ".tmp";
        await File.WriteAllBytesAsync(manifestTemp, bytes);
        await File.WriteAllTextAsync(hashTemp, hash + "  manifest.json" + Environment.NewLine, Encoding.UTF8);
        File.Move(manifestTemp, manifestPath, true);
        File.Move(hashTemp, hashPath, true);
        return hash;
    }

    public async Task<string> GetOrCreateSnapshotHashAsync(Training training, IReadOnlyCollection<TrainingQuestion> questions)
    {
        var family = string.IsNullOrWhiteSpace(training.FamilySlug) ? training.Slug : training.FamilySlug;
        var dir = _storage.GetTrainingVersionPath(family, training.Version);
        var manifestPath = Path.Combine(dir, "manifest.json");
        var expectedJson = BuildManifestJson(training, questions);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedJson);
        var expectedHash = Convert.ToHexString(SHA256.HashData(expectedBytes));

        if (File.Exists(manifestPath))
        {
            var currentHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(manifestPath)));
            if (string.Equals(currentHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                return expectedHash;
        }

        // Snapshot ausente ou divergente: reconstroi a partir do estado atual que sera evidenciado.
        return await WriteSnapshotAsync(training, questions);
    }

    private static string BuildManifestJson(Training training, IReadOnlyCollection<TrainingQuestion> questions)
    {
        var payload = new
        {
            training.Code,
            training.Slug,
            training.FamilySlug,
            training.Title,
            training.Description,
            training.Version,
            training.ContentText,
            training.PassingScore,
            training.EstimatedMinutes,
            training.RequiredForAll,
            training.LayoutKey,
            Questions = questions.OrderBy(q => q.Position).Select(q => new { q.Position, q.Text, q.Options, q.CorrectIndex })
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }
}
