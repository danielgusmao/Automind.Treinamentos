using System.Security.Cryptography;
using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public sealed class EvidenceService
{
    private readonly StorageService _storage;
    private readonly SimplePdfService _pdf;
    private readonly TrainingSnapshotService _snapshot;

    public EvidenceService(StorageService storage, SimplePdfService pdf, TrainingSnapshotService snapshot)
    {
        _storage = storage;
        _pdf = pdf;
        _snapshot = snapshot;
    }

    public async Task<(string path, string pdfHash, string trainingHash)> CreateIndividualAsync(
        Training training,
        IReadOnlyCollection<TrainingQuestion> questions,
        TrainingCompletion completion)
    {
        var trainingHash = await _snapshot.GetOrCreateSnapshotHashAsync(training, questions);
        var collaboratorFolder = $"{StorageService.SafeSegment(completion.DisplayName)} - {StorageService.SafeSegment(completion.SamAccountName)}";
        var dir = Path.Combine(_storage.CollaboratorsEvidencePath, collaboratorFolder);
        Directory.CreateDirectory(dir);

        var localDate = completion.AcceptedAtUtc.ToLocalTime();
        var file = $"{localDate:yyyy-MM-dd} - {StorageService.SafeSegment(training.Slug)} - {StorageService.SafeSegment(completion.Protocol)}.pdf";
        var fullPath = Path.Combine(dir, file);

        var bytes = _pdf.CreateEvidencePdf(training, completion, trainingHash);
        await File.WriteAllBytesAsync(fullPath, bytes);
        var pdfHash = Convert.ToHexString(SHA256.HashData(bytes));
        return (fullPath, pdfHash, trainingHash);
    }

    public async Task<string> CreateConsolidatedAsync(Training training, IReadOnlyCollection<TrainingCompletion> completions)
    {
        Directory.CreateDirectory(_storage.ReportsPath);
        var file = $"Consolidado-{StorageService.SafeSegment(training.Slug)}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
        var path = Path.Combine(_storage.ReportsPath, file);
        await File.WriteAllBytesAsync(path, _pdf.CreateConsolidatedPdf(training, completions));
        return path;
    }
}
