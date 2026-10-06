using System.Security.Claims;
using Automind.Treinamentos.Models;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automind.Treinamentos.Controllers;

[Authorize(Policy = "Informatica")]
public sealed class AdminController : Controller
{
    private readonly TrainingRepository _repo;
    private readonly AdDirectoryService _ad;
    private readonly TrainingSnapshotService _snapshot;
    private readonly EvidenceService _evidence;
    private readonly AuditService _audit;
    private readonly TeamsWebhookService _teams;

    public AdminController(TrainingRepository repo, AdDirectoryService ad, TrainingSnapshotService snapshot, EvidenceService evidence, AuditService audit, TeamsWebhookService teams)
    {
        _repo = repo;
        _ad = ad;
        _snapshot = snapshot;
        _evidence = evidence;
        _audit = audit;
        _teams = teams;
    }

    public async Task<IActionResult> Index()
    {
        var trainings = await _repo.GetAllTrainingsAsync();
        var published = trainings.Count(x => x.IsPublished);
        var completionCount = 0;
        foreach (var t in trainings) completionCount += (await _repo.GetCompletionsAsync(t.Id)).Count;
        ViewBag.Published = published;
        ViewBag.Completions = completionCount;
        try { ViewBag.EligibleAdUsers = (await _ad.GetEligibleUsersAsync()).Count; }
        catch (Exception ex) { ViewBag.AdError = ex.Message; }
        return View(trainings);
    }

    public async Task<IActionResult> Trainings() => View(await _repo.GetAllTrainingsAsync());

    [HttpGet]
    public IActionResult CreateTraining() => View(new AdminTrainingCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTraining(AdminTrainingCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var id = await _repo.CreateTrainingAsync(model, Sam());
            await _audit.WriteAsync("training-create", "success", Sam(), new { id, model.Code, model.Slug, model.Title, model.Version });
            return RedirectToAction(nameof(Questions), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Não foi possível cadastrar. Verifique se o slug já existe. " + ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Questions(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();
        ViewBag.Training = training;
        ViewBag.Questions = await _repo.GetQuestionsAsync(id);
        return View(new AdminQuestionCreateViewModel { TrainingId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(AdminQuestionCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var training = await _repo.GetTrainingAsync(model.TrainingId);
            if (training is null) return NotFound();
            ViewBag.Training = training;
            ViewBag.Questions = await _repo.GetQuestionsAsync(model.TrainingId);
            return View("Questions", model);
        }
        await _repo.AddQuestionAsync(model);
        var updatedTraining = await _repo.GetTrainingAsync(model.TrainingId);
        if (updatedTraining?.IsPublished == true)
            await _snapshot.WriteSnapshotAsync(updatedTraining, await _repo.GetQuestionsAsync(model.TrainingId));
        await _audit.WriteAsync("training-question-add", "success", Sam(), new { model.TrainingId, model.Text });
        return RedirectToAction(nameof(Questions), new { id = model.TrainingId });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(long trainingId, long questionId)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        var question = await _repo.GetQuestionAsync(trainingId, questionId);
        if (training is null || question is null) return NotFound();
        ViewBag.Training = training;
        return View(new AdminQuestionEditViewModel
        {
            TrainingId = trainingId,
            QuestionId = questionId,
            Text = question.Text,
            OptionA = question.Options.ElementAtOrDefault(0) ?? "",
            OptionB = question.Options.ElementAtOrDefault(1) ?? "",
            OptionC = question.Options.ElementAtOrDefault(2) ?? "",
            OptionD = question.Options.ElementAtOrDefault(3),
            CorrectIndex = question.CorrectIndex
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(AdminQuestionEditViewModel model)
    {
        var training = await _repo.GetTrainingAsync(model.TrainingId);
        if (training is null) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.Training = training;
            return View(model);
        }

        await _repo.UpdateQuestionAsync(model);
        if (training.IsPublished)
            await _snapshot.WriteSnapshotAsync(training, await _repo.GetQuestionsAsync(training.Id));
        await _audit.WriteAsync("training-question-edit", "success", Sam(), new { model.TrainingId, model.QuestionId, model.Text });
        TempData["Message"] = "Questao atualizada com sucesso.";
        return RedirectToAction(nameof(Questions), new { id = model.TrainingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(long trainingId, long questionId)
    {
        await _repo.DeleteQuestionAsync(trainingId, questionId);
        var updatedTraining = await _repo.GetTrainingAsync(trainingId);
        if (updatedTraining?.IsPublished == true)
            await _snapshot.WriteSnapshotAsync(updatedTraining, await _repo.GetQuestionsAsync(trainingId));
        await _audit.WriteAsync("training-question-delete", "success", Sam(), new { trainingId, questionId });
        return RedirectToAction(nameof(Questions), new { id = trainingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(long id, bool publish)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();
        var questions = await _repo.GetQuestionsAsync(id);
        if (publish)
        {
            if (questions.Count == 0) return BadRequest("Treinamento sem questões.");
            if (training.PassingScore > questions.Count) return BadRequest("Nota mínima maior que a quantidade de questões.");
            await _snapshot.WriteSnapshotAsync(training, questions);
        }
        await _repo.SetPublishedAsync(id, publish);
        await _audit.WriteAsync("training-publish", "success", Sam(), new { id, publish });
        return RedirectToAction(nameof(Trainings));
    }

    [HttpGet]
    public async Task<IActionResult> Completions(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();
        ViewBag.Training = training;
        return View(await _repo.GetCompletionsAsync(id));
    }

    [HttpGet]
    public async Task<IActionResult> Pending(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();
        ViewBag.Training = training;
        ViewBag.TeamsConfigured = _teams.IsConfigured;
        var completions = await _repo.GetCompletionsAsync(id);
        var completed = completions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        try
        {
            var users = await _ad.GetEligibleUsersAsync();
            return View(users.Select(u => new PendingUserViewModel { User = u, Completed = completed.Contains(u.SamAccountName) }).ToList());
        }
        catch (Exception ex)
        {
            ViewBag.AdError = ex.Message;
            return View(new List<PendingUserViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTeamsReminders(long trainingId, List<string>? selectedSams)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        if (training is null) return NotFound();

        var selected = (selectedSams ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(250)
            .ToList();

        if (selected.Count == 0)
        {
            TempData["TeamsError"] = "Selecione pelo menos um colaborador pendente.";
            return RedirectToAction(nameof(Pending), new { id = trainingId });
        }

        if (!_teams.IsConfigured)
        {
            TempData["TeamsError"] = "O webhook do Teams ainda nao esta configurado para o Automind.Treinamentos.";
            return RedirectToAction(nameof(Pending), new { id = trainingId });
        }

        var completions = await _repo.GetCompletionsAsync(trainingId);
        var completed = completions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var users = await _ad.GetEligibleUsersAsync();
        var usersBySam = users.ToDictionary(x => x.SamAccountName, StringComparer.OrdinalIgnoreCase);
        var link = Url.Action("Take", "Training", new { id = training.Id }, Request.Scheme, Request.Host.Value)
            ?? $"{Request.Scheme}://{Request.Host}/Training/Take/{training.Id}";

        var sent = 0;
        var failed = 0;
        foreach (var sam in selected)
        {
            if (completed.Contains(sam)) continue;
            if (!usersBySam.TryGetValue(sam, out var user) || string.IsNullOrWhiteSpace(user.Email))
            {
                failed++;
                await _audit.WriteAsync("training-teams-reminder", "failed", Sam(), new { trainingId, sam, reason = "eligible-user-or-email-not-found" });
                continue;
            }

            var result = await _teams.SendTrainingReminderAsync(user.Email, user.DisplayName, training.Title, training.EstimatedMinutes, link, HttpContext.RequestAborted);
            if (result.Success) sent++; else failed++;
            await _audit.WriteAsync("training-teams-reminder", result.Success ? "success" : "failed", Sam(), new
            {
                trainingId,
                training.Title,
                recipient = user.Email,
                sam = user.SamAccountName,
                link,
                error = result.Error
            });
        }

        TempData["TeamsResult"] = $"Lembrete(s) Teams enviado(s): {sent}." + (failed > 0 ? $" Falhas: {failed}." : "");
        return RedirectToAction(nameof(Pending), new { id = trainingId });
    }

    [HttpGet]
    public async Task<IActionResult> ConsolidatedPdf(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();
        var completions = await _repo.GetCompletionsAsync(id);
        var path = await _evidence.CreateConsolidatedAsync(training, completions);
        await _audit.WriteAsync("report-consolidated", "success", Sam(), new { id, count = completions.Count, path });
        return PhysicalFile(path, "application/pdf", Path.GetFileName(path));
    }

    [HttpGet]
    public async Task<IActionResult> Evidence(long trainingId, string sam)
    {
        var c = await _repo.GetCompletionAsync(trainingId, sam);
        if (c is null || !System.IO.File.Exists(c.EvidencePdfPath)) return NotFound();
        return PhysicalFile(c.EvidencePdfPath, "application/pdf", Path.GetFileName(c.EvidencePdfPath));
    }

    private string Sam() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
}
