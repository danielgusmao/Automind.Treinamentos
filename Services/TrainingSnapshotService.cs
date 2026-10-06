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
        var dir = Path.Combine(_storage.TrainingsPath, StorageService.SafeSegment(training.Slug), StorageService.SafeSegment(training.Version));
        Directory.CreateDirectory(dir);
        var manifestPath = Path.Combine(dir, "manifest.json");
        var payload = new
        {
            training.Code,
            training.Slug,
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
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, json, Encoding.UTF8);
        var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(manifestPath)));
        await File.WriteAllTextAsync(Path.Combine(dir, "SHA256.txt"), hash + "  manifest.json" + Environment.NewLine, Encoding.UTF8);
        return hash;
    }

    public async Task<string> GetOrCreateSnapshotHashAsync(Training training, IReadOnlyCollection<TrainingQuestion> questions)
    {
        var dir = Path.Combine(_storage.TrainingsPath, StorageService.SafeSegment(training.Slug), StorageService.SafeSegment(training.Version));
        var manifestPath = Path.Combine(dir, "manifest.json");
        if (!File.Exists(manifestPath)) return await WriteSnapshotAsync(training, questions);
        return Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(manifestPath)));
    }
}
