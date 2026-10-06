using System.Security.Claims;
using System.Security.Cryptography;
using Automind.Treinamentos.Models;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automind.Treinamentos.Controllers;

[Authorize]
public sealed class TrainingController : Controller
{
    private readonly TrainingRepository _repo;
    private readonly EvidenceService _evidence;
    private readonly AuditService _audit;

    public TrainingController(TrainingRepository repo, EvidenceService evidence, AuditService audit)
    {
        _repo = repo;
        _evidence = evidence;
        _audit = audit;
    }

    public async Task<IActionResult> Index()
    {
        var sam = Sam();
        var trainings = await _repo.GetPublishedTrainingsAsync();
        var vm = new List<TrainingListItemViewModel>();
        foreach (var t in trainings)
        {
            vm.Add(new TrainingListItemViewModel
            {
                Training = t,
                Completion = await _repo.GetCompletionAsync(t.Id, sam)
            });
        }
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Take(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null || !training.IsPublished) return NotFound();
        var questions = await _repo.GetQuestionsAsync(id);
        var completion = await _repo.GetCompletionAsync(id, Sam());
        if (completion is not null) return RedirectToAction(nameof(Completed), new { id = completion.Id });
        var startedAt = GetOrCreateTrainingStart(id);
        var vm = BuildTakeViewModel(training, questions, null, startedAt);
        return View(training.LayoutKey == "security-awareness-v1" ? "SecurityAwareness" : "Take", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(TrainingSubmitViewModel model)
    {
        var training = await _repo.GetTrainingAsync(model.TrainingId);
        if (training is null || !training.IsPublished) return NotFound();
        var questions = await _repo.GetQuestionsAsync(training.Id);
        var existing = await _repo.GetCompletionAsync(training.Id, Sam());
        if (existing is not null) return RedirectToAction(nameof(Completed), new { id = existing.Id });

        var startedAt = GetOrCreateTrainingStart(training.Id);
        var score = 0;
        foreach (var q in questions)
        {
            if (model.Answers.TryGetValue(q.Id, out var answer) && answer == q.CorrectIndex) score++;
        }

        if (model.Answers.Count < questions.Count || score < training.PassingScore || !model.Accepted)
        {
            var vm = BuildTakeViewModel(training, questions, null, startedAt);
            vm.ErrorMessage = !model.Accepted
                ? "E necessario aceitar o Termo de Ciencia."
                : $"Resultado {score}/{questions.Count}. E necessario atingir {training.PassingScore}/{questions.Count} para registrar o aceite.";
            return View(training.LayoutKey == "security-awareness-v1" ? "SecurityAwareness" : "Take", vm);
        }

        var acceptedAt = DateTime.UtcNow;
        var elapsed = acceptedAt - startedAt;
        var durationSeconds = elapsed.TotalSeconds < 0 || elapsed.TotalHours > 24
            ? 0
            : (int)Math.Round(elapsed.TotalSeconds);

        var completion = new TrainingCompletion
        {
            TrainingId = training.Id,
            SamAccountName = Sam(),
            DisplayName = User.Identity?.Name ?? Sam(),
            Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
            JobTitle = User.FindFirstValue("job_title") ?? "",
            Department = User.FindFirstValue("department") ?? "",
            Score = score,
            Total = questions.Count,
            StartedAtUtc = startedAt,
            AcceptedAtUtc = acceptedAt,
            DurationSeconds = durationSeconds,
            Protocol = $"AM-{acceptedAt:yyyyMMdd}-{RandomNumberGenerator.GetHexString(4)}"
        };

        string? generatedPath = null;
        try
        {
            var ev = await _evidence.CreateIndividualAsync(training, questions, completion);
            generatedPath = ev.path;
            completion.EvidencePdfPath = ev.path;
            completion.EvidencePdfSha256 = ev.pdfHash;
            completion.TrainingSnapshotSha256 = ev.trainingHash;
            await _repo.InsertCompletionAsync(completion);
            HttpContext.Session.Remove(StartKey(training.Id));
            await _audit.WriteAsync("training-completion", "success", completion.SamAccountName, new
            {
                training.Id,
                training.Title,
                training.Version,
                training.EstimatedMinutes,
                completion.Score,
                completion.Total,
                completion.DurationSeconds,
                completion.Protocol,
                completion.EvidencePdfSha256
            });
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(generatedPath) && System.IO.File.Exists(generatedPath))
            {
                try { System.IO.File.Delete(generatedPath); } catch { }
            }
            await _audit.WriteAsync("training-completion", "failed", completion.SamAccountName, new { training.Id, error = ex.GetType().Name });
            var vm = BuildTakeViewModel(training, questions, null, startedAt);
            vm.ErrorMessage = "Nao foi possivel registrar a evidencia. Nenhum aceite foi concluido. Procure a TI.";
            return View(training.LayoutKey == "security-awareness-v1" ? "SecurityAwareness" : "Take", vm);
        }

        var saved = await _repo.GetCompletionAsync(training.Id, completion.SamAccountName);
        return saved is null ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Completed), new { id = saved.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Completed(long id)
    {
        var trainings = await _repo.GetPublishedTrainingsAsync();
        TrainingCompletion? completion = null;
        Training? training = null;
        foreach (var t in trainings)
        {
            var c = await _repo.GetCompletionAsync(t.Id, Sam());
            if (c?.Id == id) { completion = c; training = t; break; }
        }
        if (completion is null || training is null) return NotFound();
        ViewBag.Training = training;
        return View(completion);
    }

    [HttpGet]
    public async Task<IActionResult> Evidence(long trainingId)
    {
        var c = await _repo.GetCompletionAsync(trainingId, Sam());
        if (c is null || !System.IO.File.Exists(c.EvidencePdfPath)) return NotFound();
        return PhysicalFile(c.EvidencePdfPath, "application/pdf", Path.GetFileName(c.EvidencePdfPath));
    }

    private TrainingTakeViewModel BuildTakeViewModel(Training training, List<TrainingQuestion> questions, TrainingCompletion? completion, DateTime startedAt) => new()
    {
        Training = training,
        Questions = questions,
        ExistingCompletion = completion,
        DisplayName = User.Identity?.Name ?? Sam(),
        Email = User.FindFirstValue(ClaimTypes.Email) ?? "",
        JobTitle = User.FindFirstValue("job_title") ?? "",
        Department = User.FindFirstValue("department") ?? "",
        StartedAtUtc = startedAt
    };

    private DateTime GetOrCreateTrainingStart(long trainingId)
    {
        var key = StartKey(trainingId);
        var value = HttpContext.Session.GetString(key);
        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var started))
            return started;
        started = DateTime.UtcNow;
        HttpContext.Session.SetString(key, started.ToString("O"));
        return started;
    }

    private string StartKey(long trainingId) => $"TrainingStart:{Sam()}:{trainingId}";

    private string Sam() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("Sessao sem identificador AD.");
}
