using System.Security.Claims;
using Automind.Treinamentos.Models;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Controllers;

[Authorize(Policy = "TreinamentosAdmin")]
public sealed class AdminController : Controller
{
    private readonly TrainingRepository _repo;
    private readonly AdDirectoryService _ad;
    private readonly TrainingSnapshotService _snapshot;
    private readonly EvidenceService _evidence;
    private readonly AuditService _audit;
    private readonly TeamsWebhookService _teams;
    private readonly TeamsReminderService _reminders;
    private readonly PortalOptions _portal;
    private readonly ILogger<AdminController> _logger;

    public AdminController(TrainingRepository repo, AdDirectoryService ad, TrainingSnapshotService snapshot, EvidenceService evidence, AuditService audit, TeamsWebhookService teams, TeamsReminderService reminders, IOptions<PortalOptions> portal, ILogger<AdminController> logger)
    {
        _repo = repo;
        _ad = ad;
        _snapshot = snapshot;
        _evidence = evidence;
        _audit = audit;
        _teams = teams;
        _reminders = reminders;
        _portal = portal.Value;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var trainings = await _repo.GetAllTrainingsAsync();
        var published = trainings.Count(x => x.IsPublished);
        var completionCount = await _repo.GetTotalCompletionCountAsync();
        ViewBag.Published = published;
        ViewBag.Completions = completionCount;

        var exclusions = await _repo.GetDirectoryExclusionsAsync(activeOnly: true);
        ViewBag.DirectoryExclusions = exclusions.Count;
        try
        {
            var users = await _ad.GetEligibleUsersAsync();
            var excludedSams = exclusions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var excludedEmails = exclusions.Where(x => !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
            ViewBag.EligibleAdUsers = users.Count(u => !excludedSams.Contains(u.SamAccountName) && !excludedEmails.Contains(u.Email));
            ViewBag.TotalAdUsers = users.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao consultar usuários elegíveis no AD para dashboard.");
            ViewBag.AdError = "Não foi possível consultar o Active Directory.";
        }
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
        if (await _repo.TrainingVersionExistsAsync(model.Code, model.Version))
        {
            ModelState.AddModelError(nameof(model.Version), $"A versão {model.Version} já existe para o código {model.Code}.");
            return View(model);
        }
        try
        {
            var id = await _repo.CreateTrainingAsync(model, Sam());
            await _audit.WriteAsync("training-create", "success", Sam(), new { id, model.Code, model.Slug, model.Title, model.Version });
            return RedirectToAction(nameof(Questions), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao criar treinamento {Code} {Version}.", model.Code, model.Version);
            ModelState.AddModelError("", "Não foi possível cadastrar. Verifique código, versão e slug informados.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditTraining(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();

        var completionCount = await _repo.GetCompletionCountAsync(id);
        var hasCompletions = completionCount > 0;
        return View(new AdminTrainingEditViewModel
        {
            Id = training.Id,
            Code = training.Code,
            Slug = training.Slug,
            Title = training.Title,
            Description = training.Description,
            SummaryText = training.SummaryText,
            Version = hasCompletions ? SuggestNextVersion(training.Version) : training.Version,
            CurrentVersion = training.Version,
            ContentText = training.ContentText,
            PassingScore = training.PassingScore,
            EstimatedMinutes = training.EstimatedMinutes,
            RequiredForAll = training.RequiredForAll,
            HasCompletions = hasCompletions,
            CompletionCount = completionCount,
            IsPublished = training.IsPublished,
            IsArchived = training.IsArchived,
            LayoutKey = training.LayoutKey
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTraining(AdminTrainingEditViewModel model)
    {
        var training = await _repo.GetTrainingAsync(model.Id);
        if (training is null) return NotFound();

        var completionCount = await _repo.GetCompletionCountAsync(training.Id);
        model.Code = training.Code;
        model.Slug = training.Slug;
        model.CurrentVersion = training.Version;
        model.HasCompletions = completionCount > 0;
        model.CompletionCount = completionCount;
        model.IsPublished = training.IsPublished;
        model.IsArchived = training.IsArchived;
        model.LayoutKey = training.LayoutKey;

        if (training.LayoutKey == "security-awareness-v1")
            model.ContentText = training.ContentText;

        if (!ModelState.IsValid) return View(model);

        try
        {
            if (completionCount > 0 || training.IsArchived)
            {
                if (string.Equals(training.Version, model.Version?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.Version), "Este treinamento já possui evidências. Informe uma nova versão.");
                    return View(model);
                }

                var newId = await _repo.CreateTrainingRevisionAsync(training, model, Sam());
                await _audit.WriteAsync("training-revision-create", "success", Sam(), new
                {
                    sourceTrainingId = training.Id,
                    newTrainingId = newId,
                    training.Code,
                    fromVersion = training.Version,
                    toVersion = model.Version,
                    completionCount
                });
                TempData["Message"] = $"Nova versão {model.Version} criada como rascunho. Os {completionCount} registros de conclusão da versão {training.Version} foram preservados.";
                return RedirectToAction(nameof(Questions), new { id = newId });
            }

            if (await _repo.TrainingVersionExistsAsync(training.Code, model.Version, training.Id))
            {
                ModelState.AddModelError(nameof(model.Version), $"A versão {model.Version} já existe para o código {training.Code}.");
                return View(model);
            }

            if (training.IsPublished)
            {
                var questionCount = (await _repo.GetQuestionsAsync(training.Id)).Count;
                if (questionCount == 0)
                {
                    ModelState.AddModelError("", "Um treinamento publicado precisa ter pelo menos uma questão.");
                    return View(model);
                }
                if (model.PassingScore > questionCount)
                {
                    ModelState.AddModelError(nameof(model.PassingScore), "A nota mínima não pode ser maior que a quantidade de questões.");
                    return View(model);
                }
            }

            await _repo.UpdateTrainingAsync(model);
            var updated = await _repo.GetTrainingAsync(training.Id);
            await RefreshPublishedSnapshotBestEffortAsync(updated);

            await _audit.WriteAsync("training-edit", "success", Sam(), new
            {
                training.Id,
                training.Code,
                oldVersion = training.Version,
                newVersion = model.Version,
                model.Title
            });
            TempData["TrainingMessage"] = "Treinamento atualizado com sucesso.";
            return RedirectToAction(nameof(Trainings));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao editar treinamento {TrainingId}.", model.Id);
            ModelState.AddModelError("", "Não foi possível salvar o treinamento. Consulte a TI se o problema persistir.");
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
        ViewBag.CompletionCount = await _repo.GetCompletionCountAsync(id);
        ViewBag.QuestionsLocked = training.IsArchived || (int)ViewBag.CompletionCount > 0;
        return View(new AdminQuestionCreateViewModel { TrainingId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(AdminQuestionCreateViewModel model)
    {
        var trainingForWrite = await _repo.GetTrainingAsync(model.TrainingId);
        if (trainingForWrite is null) return NotFound();
        if (trainingForWrite.IsArchived || await _repo.GetCompletionCountAsync(model.TrainingId) > 0)
        {
            TempData["Message"] = "Esta versão possui evidências e é imutável. Use Editar treinamento para criar uma nova versão antes de alterar as questões.";
            return RedirectToAction(nameof(Questions), new { id = model.TrainingId });
        }

        if (!ModelState.IsValid)
        {
            var training = await _repo.GetTrainingAsync(model.TrainingId);
            if (training is null) return NotFound();
            ViewBag.Training = training;
            ViewBag.Questions = await _repo.GetQuestionsAsync(model.TrainingId);
            ViewBag.CompletionCount = 0;
            ViewBag.QuestionsLocked = false;
            return View("Questions", model);
        }
        await _repo.AddQuestionAsync(model);
        var updatedTraining = await _repo.GetTrainingAsync(model.TrainingId);
        await RefreshPublishedSnapshotBestEffortAsync(updatedTraining);
        await _audit.WriteAsync("training-question-add", "success", Sam(), new { model.TrainingId, model.Text });
        return RedirectToAction(nameof(Questions), new { id = model.TrainingId });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(long trainingId, long questionId)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        var question = await _repo.GetQuestionAsync(trainingId, questionId);
        if (training is null || question is null) return NotFound();
        if (training.IsArchived || await _repo.GetCompletionCountAsync(trainingId) > 0)
        {
            TempData["Message"] = "Esta versão possui evidências e é imutável. Crie uma nova versão para alterar as questões.";
            return RedirectToAction(nameof(Questions), new { id = trainingId });
        }
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
        if (training.IsArchived || await _repo.GetCompletionCountAsync(model.TrainingId) > 0)
        {
            TempData["Message"] = "Esta versão possui evidências e é imutável. Crie uma nova versão para alterar as questões.";
            return RedirectToAction(nameof(Questions), new { id = model.TrainingId });
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Training = training;
            return View(model);
        }

        await _repo.UpdateQuestionAsync(model);
        await RefreshPublishedSnapshotBestEffortAsync(training);
        await _audit.WriteAsync("training-question-edit", "success", Sam(), new { model.TrainingId, model.QuestionId, model.Text });
        TempData["Message"] = "Questão atualizada com sucesso.";
        return RedirectToAction(nameof(Questions), new { id = model.TrainingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(long trainingId, long questionId)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        if (training is null) return NotFound();
        if (training.IsArchived || await _repo.GetCompletionCountAsync(trainingId) > 0)
        {
            TempData["Message"] = "Esta versão possui evidências e é imutável. Crie uma nova versão para alterar as questões.";
            return RedirectToAction(nameof(Questions), new { id = trainingId });
        }
        await _repo.DeleteQuestionAsync(trainingId, questionId);
        var updatedTraining = await _repo.GetTrainingAsync(trainingId);
        await RefreshPublishedSnapshotBestEffortAsync(updatedTraining);
        await _audit.WriteAsync("training-question-delete", "success", Sam(), new { trainingId, questionId });
        return RedirectToAction(nameof(Questions), new { id = trainingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishTraining(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();

        if (training.IsArchived)
        {
            TempData["TrainingError"] = "Versoes históricas não podem ser republicadas. Crie uma nova versão.";
            return RedirectToAction(nameof(Trainings));
        }

        if (training.IsPublished)
        {
            TempData["TrainingMessage"] = $"Treinamento {training.Code} - versão {training.Version} já esta publicado.";
            return RedirectToAction(nameof(Trainings));
        }

        var questions = await _repo.GetQuestionsAsync(id);
        if (questions.Count == 0)
        {
            TempData["Message"] = "Para publicar este treinamento, adicione pelo menos uma questão.";
            return RedirectToAction(nameof(Questions), new { id });
        }

        if (training.PassingScore > questions.Count)
        {
            TempData["Message"] = $"A nota mínima ({training.PassingScore}) é maior que a quantidade de questões ({questions.Count}). Ajuste o treinamento antes de publicar.";
            return RedirectToAction(nameof(Questions), new { id });
        }

        try
        {
            await _snapshot.WriteSnapshotAsync(training, questions);
            await _repo.SetPublishedAsync(id, true);
            await _audit.WriteAsync("training-publish", "success", Sam(), new { id, publish = true });
            TempData["TrainingMessage"] = $"Treinamento {training.Code} - versão {training.Version} publicado com sucesso.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao publicar treinamento {TrainingId}.", id);
            await _audit.WriteAsync("training-publish", "error", Sam(), new { id, publish = true, error = ex.GetType().Name, code = ex.HResult });
            TempData["TrainingError"] = "Não foi possível publicar o treinamento. Consulte a TI se o problema persistir.";
        }

        return RedirectToAction(nameof(Trainings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnpublishTraining(long id)
    {
        var training = await _repo.GetTrainingAsync(id);
        if (training is null) return NotFound();

        if (training.IsArchived)
        {
            TempData["TrainingError"] = "Versoes históricas não podem ser alteradas.";
            return RedirectToAction(nameof(Trainings));
        }

        if (!training.IsPublished)
        {
            TempData["TrainingMessage"] = $"Treinamento {training.Code} - versão {training.Version} já esta despublicado.";
            return RedirectToAction(nameof(Trainings));
        }

        try
        {
            await _repo.SetPublishedAsync(id, false);
            await _audit.WriteAsync("training-unpublish", "success", Sam(), new { id, publish = false });
            TempData["TrainingMessage"] = $"Treinamento {training.Code} - versão {training.Version} despublicado com sucesso.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao despublicar treinamento {TrainingId}.", id);
            await _audit.WriteAsync("training-unpublish", "error", Sam(), new { id, publish = false, error = ex.GetType().Name, code = ex.HResult });
            TempData["TrainingError"] = "Não foi possível despublicar o treinamento. Consulte a TI se o problema persistir.";
        }

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
        if (training.IsArchived)
        {
            TempData["TrainingMessage"] = "Esta é uma versão histórica. Consulte Concluídos para as evidências preservadas.";
            return RedirectToAction(nameof(Trainings));
        }
        ViewBag.Training = training;
        ViewBag.TeamsConfigured = _teams.IsConfigured;
        var schedule = await _repo.GetTrainingReminderScheduleAsync(id);
        ViewBag.TeamsSchedule = schedule;

        var completions = await _repo.GetCompletionsAsync(id);
        var completed = completions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var latestReminders = await _repo.GetLatestTeamsReminderHistoryAsync(id);
        var latestSuccessfulReminders = await _repo.GetLatestSuccessfulTeamsReminderHistoryAsync(id);
        var exclusions = await _repo.GetDirectoryExclusionsAsync(activeOnly: true);
        var excludedSams = exclusions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excludedEmails = exclusions.Where(x => !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        ViewBag.ExcludedCount = exclusions.Count;

        try
        {
            var users = await _ad.GetEligibleUsersAsync();
            var eligible = users
                .Where(u => !excludedSams.Contains(u.SamAccountName) && !excludedEmails.Contains(u.Email))
                .ToList();
            return View(eligible.Select(u =>
            {
                latestReminders.TryGetValue(u.SamAccountName, out var lastReminder);
                latestSuccessfulReminders.TryGetValue(u.SamAccountName, out var lastSuccessfulReminder);
                var isCompleted = completed.Contains(u.SamAccountName);
                return new PendingUserViewModel
                {
                    User = u,
                    Completed = isCompleted,
                    LastTeamsReminder = lastReminder,
                    LastSuccessfulTeamsReminder = lastSuccessfulReminder,
                    ReminderDue = !isCompleted && TeamsReminderPolicy.IsDue(lastSuccessfulReminder, schedule.IntervalDays, DateTime.UtcNow)
                };
            }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao consultar usuários elegíveis no AD.");
            ViewBag.AdError = "Não foi possível consultar o Active Directory.";
            return View(new List<PendingUserViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcludeUser(long trainingId, string sam, string category, string reason)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        if (training is null) return NotFound();

        sam = (sam ?? string.Empty).Trim();
        category = NormalizeExclusionCategory(category);
        reason = (reason ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(sam) || string.IsNullOrWhiteSpace(reason))
        {
            TempData["ExclusionError"] = "Informe o colaborador e o motivo da exclusão permanente.";
            return RedirectToAction(nameof(Pending), new { id = trainingId });
        }
        if (reason.Length > 300) reason = reason[..300];

        var users = await _ad.GetEligibleUsersAsync();
        var user = users.FirstOrDefault(x => string.Equals(x.SamAccountName, sam, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            TempData["ExclusionError"] = "Conta não localizada entre os usuários elegíveis do Active Directory.";
            return RedirectToAction(nameof(Pending), new { id = trainingId });
        }

        await _repo.UpsertDirectoryExclusionAsync(user, category, reason, Sam());
        await _audit.WriteAsync("directory-exclusion-add", "success", Sam(), new
        {
            sam = user.SamAccountName,
            user.DisplayName,
            user.Email,
            category,
            reason,
            sourceTrainingId = trainingId
        });
        TempData["ExclusionResult"] = $"{user.DisplayName} foi adicionado à lista permanente de exclusões e não será considerado em nenhum treinamento.";
        return RedirectToAction(nameof(Pending), new { id = trainingId });
    }

    [HttpGet]
    public async Task<IActionResult> DirectoryExclusions()
    {
        var all = await _repo.GetDirectoryExclusionsAsync();
        ViewBag.Active = all.Where(x => x.IsActive).ToList();
        ViewBag.History = all.Where(x => !x.IsActive).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDirectoryExclusion(string lookup, string category, string reason)
    {
        lookup = (lookup ?? string.Empty).Trim();
        category = NormalizeExclusionCategory(category);
        reason = (reason ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(lookup) || string.IsNullOrWhiteSpace(reason))
        {
            TempData["DirectoryExclusionError"] = "Informe o login/e-mail e o motivo.";
            return RedirectToAction(nameof(DirectoryExclusions));
        }
        if (reason.Length > 300) reason = reason[..300];

        try
        {
            var users = await _ad.GetEligibleUsersAsync();
            var user = users.FirstOrDefault(x =>
                string.Equals(x.SamAccountName, lookup, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Email, lookup, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                TempData["DirectoryExclusionError"] = "Nenhuma conta elegivel foi localizada no AD com esse login ou e-mail.";
                return RedirectToAction(nameof(DirectoryExclusions));
            }

            await _repo.UpsertDirectoryExclusionAsync(user, category, reason, Sam());
            await _audit.WriteAsync("directory-exclusion-add", "success", Sam(), new
            {
                sam = user.SamAccountName,
                user.DisplayName,
                user.Email,
                category,
                reason,
                source = "directory-exclusions-page"
            });
            TempData["DirectoryExclusionResult"] = $"{user.DisplayName} foi excluido permanentemente da base de treinamentos.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao consultar AD para exclusão permanente.");
            TempData["DirectoryExclusionError"] = "Não foi possível consultar o Active Directory.";
        }
        return RedirectToAction(nameof(DirectoryExclusions));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReincludeDirectoryUser(string sam)
    {
        sam = (sam ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sam)) return RedirectToAction(nameof(DirectoryExclusions));

        await _repo.ReincludeDirectoryUserAsync(sam, Sam());
        await _audit.WriteAsync("directory-exclusion-remove", "success", Sam(), new { sam });
        TempData["DirectoryExclusionResult"] = $"{sam} voltou a ser considerado em todos os treinamentos.";
        return RedirectToAction(nameof(DirectoryExclusions));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTeamsReminderSchedule(long trainingId, bool enabled, int intervalDays)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        if (training is null) return NotFound();
        if (training.IsArchived || !training.IsPublished)
        {
            TempData["TeamsScheduleError"] = "A agenda so pode ser ativada na versao atualmente publicada.";
            return RedirectToAction(nameof(Pending), new { id = trainingId });
        }

        intervalDays = Math.Clamp(intervalDays, 1, 365);
        if (enabled && !_teams.IsConfigured)
        {
            TempData["TeamsScheduleError"] = "Configure o Workflow do Teams antes de ativar a agenda.";
            return RedirectToAction(nameof(Pending), new { id = trainingId });
        }

        var nextRunAtUtc = enabled ? DateTime.UtcNow.AddDays(intervalDays) : (DateTime?)null;
        await _repo.SaveTrainingReminderScheduleAsync(trainingId, enabled, intervalDays, nextRunAtUtc, Sam());
        await _audit.WriteAsync("training-teams-schedule", "success", Sam(), new
        {
            trainingId,
            enabled,
            intervalDays,
            nextRunAtUtc
        });

        TempData["TeamsScheduleResult"] = enabled
            ? $"Agenda ativada: novo ciclo a cada {intervalDays} dia(s)."
            : "Agenda automatica desativada.";
        return RedirectToAction(nameof(Pending), new { id = trainingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTeamsReminders(long trainingId, List<string>? selectedSams)
    {
        var training = await _repo.GetTrainingAsync(trainingId);
        if (training is null) return NotFound();
        if (training.IsArchived || !training.IsPublished)
        {
            TempData["TeamsError"] = "Lembretes so podem ser enviados para a versao atualmente publicada.";
            return RedirectToAction(nameof(Trainings));
        }

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
        var exclusions = await _repo.GetDirectoryExclusionsAsync(activeOnly: true);
        var excludedSams = exclusions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excludedEmails = exclusions.Where(x => !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var users = await _ad.GetEligibleUsersAsync();
        var usersBySam = users.ToDictionary(x => x.SamAccountName, StringComparer.OrdinalIgnoreCase);
        var baseUrl = (_portal.PublicBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        var link = string.IsNullOrWhiteSpace(baseUrl)
            ? (Url.Action("Start", "Training", new { id = training.Id }, Request.Scheme, Request.Host.Value)
                ?? $"{Request.Scheme}://{Request.Host}/Training/Start/{training.Id}")
            : $"{baseUrl}/Training/Start/{training.Id}";

        var confirmed = 0;
        var accepted = 0;
        var failed = 0;
        foreach (var sam in selected)
        {
            if (completed.Contains(sam)) continue;
            if (!usersBySam.TryGetValue(sam, out var user) || string.IsNullOrWhiteSpace(user.Email))
            {
                failed++;
                await _audit.WriteAsync("training-teams-reminder", "failed", Sam(), new { trainingId, sam, source = TeamsReminderSource.Manual, reason = "eligible-user-or-email-not-found" });
                continue;
            }
            if (excludedSams.Contains(user.SamAccountName) || excludedEmails.Contains(user.Email)) continue;

            var dispatch = await _reminders.SendAsync(
                training,
                user,
                link,
                TeamsReminderSource.Manual,
                Sam(),
                cycleKey: null,
                HttpContext.RequestAborted);

            var result = dispatch.SendResult;
            if (result is null || !result.Success) failed++;
            else if (string.Equals(result.Status, TeamsReminderStatus.DeliveryConfirmed, StringComparison.OrdinalIgnoreCase)) confirmed++;
            else accepted++;
        }

        TempData["TeamsResult"] = $"Teams - confirmados: {confirmed}; aceitos pelo Workflow: {accepted}; falhas: {failed}.";
        return RedirectToAction(nameof(Pending), new { id = trainingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

    private async Task RefreshPublishedSnapshotBestEffortAsync(Training? training)
    {
        if (training?.IsPublished != true) return;
        try
        {
            await _snapshot.WriteSnapshotAsync(training, await _repo.GetQuestionsAsync(training.Id));
        }
        catch (Exception ex)
        {
            // O banco é a fonte autoritativa. Na conclusão, EvidenceService valida/reconstroi o snapshot.
            _logger.LogWarning(ex, "Treinamento {TrainingId} foi atualizado, mas o refresh imediato do snapshot falhou.", training.Id);
        }
    }

    private static string NormalizeExclusionCategory(string? category)
    {
        var value = (category ?? string.Empty).Trim();
        return value switch
        {
            "E-mail geral / Caixa compartilhada" => value,
            "Conta de serviço" => value,
            "Terceiro / Não colaborador" => value,
            "Outro" => value,
            _ => "E-mail geral / Caixa compartilhada"
        };
    }

    private static string SuggestNextVersion(string? current)
    {
        var parts = (current ?? string.Empty).Trim().Split('.');
        if (parts.Length == 3 && int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor) && int.TryParse(parts[2], out var patch))
            return $"{major}.{minor}.{patch + 1}";
        return string.IsNullOrWhiteSpace(current) ? "1.0.0" : current + ".1";
    }

    private string Sam() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
}
