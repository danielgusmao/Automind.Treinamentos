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
    private readonly ILogger<TrainingController> _logger;

    public TrainingController(TrainingRepository repo, EvidenceService evidence, AuditService audit, ILogger<TrainingController> logger)
    {
        _repo = repo;
        _evidence = evidence;
        _audit = audit;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var sam = Sam();
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        if (await _repo.IsDirectoryExcludedAsync(sam, email))
        {
            TempData["TrainingMessage"] = "Seu usuario esta fora da base de colaboradores elegiveis para treinamentos.";
            return View(new List<TrainingListItemViewModel>());
        }

        return View(await _repo.GetPublishedTrainingItemsAsync(sam));
    }

    [HttpGet("/Training/Start/{id:long}")]
    public Task<IActionResult> Start(long id) => Take(id);

    [HttpGet]
    public async Task<IActionResult> Take(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null || !training.IsPublished) return NotFound();
        if (await IsExcludedAsync())
        {
            TempData["TrainingMessage"] = "Seu usuario esta fora da base de colaboradores elegiveis para treinamentos.";
            return RedirectToAction(nameof(Index));
        }

        var completion = await _repo.GetCompletionAsync(id, Sam());
        if (completion is not null) return RedirectToAction(nameof(Completed), new { id = completion.Id });

        var questions = await _repo.GetQuestionsAsync(id);
        var startedAt = GetOrCreateTrainingStart(id);
        return View(training.LayoutKey == "security-awareness-v1" ? "SecurityAwareness" : "Take",
            BuildTakeViewModel(training, questions, startedAt));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(TrainingSubmitViewModel model)
    {
        var training = await _repo.GetTrainingAsync(model.TrainingId);
        if (training is null || !training.IsPublished) return NotFound();
        if (await IsExcludedAsync())
        {
            TempData["TrainingMessage"] = "Seu usuario esta fora da base de colaboradores elegiveis para treinamentos.";
            return RedirectToAction(nameof(Index));
        }

        var questions = await _repo.GetQuestionsAsync(training.Id);
        var existing = await _repo.GetCompletionAsync(training.Id, Sam());
        if (existing is not null) return RedirectToAction(nameof(Completed), new { id = existing.Id });

        var startedAt = GetOrCreateTrainingStart(training.Id);
        var quiz = QuizValidationService.Validate(questions, model.Answers);
        if (!quiz.IsStructurallyValid)
        {
            _logger.LogWarning("Quiz invalido recebido para treinamento {TrainingId} e usuario {SamAccountName}: {Reason}", training.Id, Sam(), quiz.Error);
            var vm = BuildTakeViewModel(training, questions, startedAt);
            vm.ErrorMessage = "Responda todas as questoes usando apenas as opcoes apresentadas.";
            return View(training.LayoutKey == "security-awareness-v1" ? "SecurityAwareness" : "Take", vm);
        }

        var score = quiz.Score;
        if (score < training.PassingScore || !model.Accepted)
        {
            var vm = BuildTakeViewModel(training, questions, startedAt);
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
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            JobTitle = User.FindFirstValue("job_title") ?? string.Empty,
            Department = User.FindFirstValue("department") ?? string.Empty,
            Score = score,
            Total = questions.Count,
            StartedAtUtc = startedAt,
            AcceptedAtUtc = acceptedAt,
            DurationSeconds = durationSeconds,
            Protocol = $"AM-{acceptedAt:yyyyMMdd}-{RandomNumberGenerator.GetHexString(4)}"
        };

        string? generatedPath = null;
        var persisted = false;
        try
        {
            var ev = await _evidence.CreateIndividualAsync(training, questions, completion);
            generatedPath = ev.path;
            completion.EvidencePdfPath = ev.path;
            completion.EvidencePdfSha256 = ev.pdfHash;
            completion.TrainingSnapshotSha256 = ev.trainingHash;
            await _repo.InsertCompletionAsync(completion);
            persisted = true;
            HttpContext.Session.Remove(StartKey(training.Id));
        }
        catch (Exception ex)
        {
            if (!persisted && !string.IsNullOrWhiteSpace(generatedPath) && System.IO.File.Exists(generatedPath))
            {
                try { System.IO.File.Delete(generatedPath); } catch { }
            }

            _logger.LogError(ex, "Falha ao persistir conclusao do treinamento {TrainingId} para {SamAccountName}.", training.Id, completion.SamAccountName);
            try
            {
                await _audit.WriteAsync("training-completion", "failed", completion.SamAccountName, new { training.Id, error = ex.GetType().Name, code = ex.HResult });
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Falha adicional ao registrar auditoria de conclusao malsucedida.");
            }

            var vm = BuildTakeViewModel(training, questions, startedAt);
            vm.ErrorMessage = "Nao foi possivel registrar a evidencia. Nenhum aceite foi concluido. Procure a TI.";
            return View(training.LayoutKey == "security-awareness-v1" ? "SecurityAwareness" : "Take", vm);
        }

        // Uma falha de auditoria depois do INSERT nao invalida nem apaga uma conclusao ja persistida.
        try
        {
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
            _logger.LogError(ex, "Conclusao {Protocol} foi persistida, mas a auditoria falhou.", completion.Protocol);
        }

        var saved = await _repo.GetCompletionAsync(training.Id, completion.SamAccountName);
        return saved is null ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Completed), new { id = saved.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Completed(long id)
    {
        var completion = await _repo.GetCompletionByIdAsync(id, Sam());
        if (completion is null) return NotFound();
        var training = await _repo.GetTrainingAsync(completion.TrainingId);
        if (training is null) return NotFound();
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

    private TrainingTakeViewModel BuildTakeViewModel(Training training, List<TrainingQuestion> questions, DateTime startedAt) => new()
    {
        Training = training,
        Questions = questions,
        DisplayName = User.Identity?.Name ?? Sam(),
        Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
        JobTitle = User.FindFirstValue("job_title") ?? string.Empty,
        Department = User.FindFirstValue("department") ?? string.Empty,
        StartedAtUtc = startedAt
    };

    private Task<bool> IsExcludedAsync() =>
        _repo.IsDirectoryExcludedAsync(Sam(), User.FindFirstValue(ClaimTypes.Email));

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
