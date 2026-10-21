using Automind.Treinamentos.Models;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsReminderWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TeamsReminderWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSchedulesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao processar a agenda de lembretes Teams.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessDueSchedulesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<TrainingRepository>();
        var directory = scope.ServiceProvider.GetRequiredService<AdDirectoryService>();
        var reminders = scope.ServiceProvider.GetRequiredService<TeamsReminderService>();
        var webhook = scope.ServiceProvider.GetRequiredService<TeamsWebhookService>();
        var portal = scope.ServiceProvider.GetRequiredService<IOptions<PortalOptions>>().Value;

        if (!webhook.IsConfigured) return;

        var nowUtc = DateTime.UtcNow;
        var schedules = await repository.GetDueTrainingReminderSchedulesAsync(nowUtc);
        foreach (var schedule in schedules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (schedule.NextRunAtUtc is null) continue;

            var training = await repository.GetTrainingAsync(schedule.TrainingId);
            if (training is null || training.IsArchived || !training.IsPublished)
            {
                await repository.SaveTrainingReminderScheduleAsync(
                    schedule.TrainingId,
                    false,
                    schedule.IntervalDays,
                    null,
                    "scheduler:auto-disable");
                continue;
            }

            var baseUrl = (portal.PublicBaseUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogWarning("Agenda Teams ignorada para treinamento {TrainingId}: Portal:PublicBaseUrl nao configurado.", training.Id);
                continue;
            }

            var completions = await repository.GetCompletionsAsync(training.Id);
            var completed = completions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var exclusions = await repository.GetDirectoryExclusionsAsync(activeOnly: true);
            var excludedSams = exclusions.Select(x => x.SamAccountName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var excludedEmails = exclusions.Where(x => !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var latestSuccessful = await repository.GetLatestSuccessfulTeamsReminderHistoryAsync(training.Id);
            var users = await directory.GetEligibleUsersAsync();

            foreach (var user in users)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(user.SamAccountName) || string.IsNullOrWhiteSpace(user.Email)) continue;
                if (completed.Contains(user.SamAccountName)) continue;
                if (excludedSams.Contains(user.SamAccountName) || excludedEmails.Contains(user.Email)) continue;
                latestSuccessful.TryGetValue(user.SamAccountName, out var lastSuccess);
                if (!TeamsReminderPolicy.IsDue(lastSuccess, schedule.IntervalDays, nowUtc)) continue;

                var cycleKey = $"automatic:{training.Id}:{schedule.NextRunAtUtc.Value.ToUniversalTime():O}:{user.SamAccountName.Trim().ToLowerInvariant()}";
                await reminders.QueueAsync(
                    training,
                    user,
                    TeamsReminderSource.Automatic,
                    "scheduler",
                    cycleKey,
                    cancellationToken);
            }

            await repository.AdvanceTrainingReminderScheduleAsync(schedule, DateTime.UtcNow);
        }
    }
}
