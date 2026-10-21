using Automind.Treinamentos.Models;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsReminderDispatchWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TeamsOptions> teamsOptions,
    IOptions<PortalOptions> portalOptions,
    ILogger<TeamsReminderDispatchWorker> logger) : BackgroundService
{
    private readonly TimeSpan _dispatchInterval = TimeSpan.FromSeconds(Math.Clamp(teamsOptions.Value.DispatchIntervalSeconds, 12, 300));
    private readonly string _publicBaseUrl = (portalOptions.Value.PublicBaseUrl ?? string.Empty).Trim().TrimEnd('/');

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var dispatched = false;
            try
            {
                dispatched = await DispatchNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao processar a fila de lembretes Teams.");
            }

            try
            {
                await Task.Delay(dispatched ? _dispatchInterval : TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<bool> DispatchNextAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<TrainingRepository>();
        var webhook = scope.ServiceProvider.GetRequiredService<TeamsWebhookService>();
        var audit = scope.ServiceProvider.GetRequiredService<AuditService>();

        if (!webhook.IsConfigured || string.IsNullOrWhiteSpace(_publicBaseUrl)) return false;

        var attempt = await repository.ClaimNextQueuedTeamsReminderAsync(DateTime.UtcNow);
        if (attempt is null) return false;

        var training = await repository.GetTrainingAsync(attempt.TrainingId);
        if (training is null || training.IsArchived || !training.IsPublished)
        {
            await repository.CompleteTeamsReminderAttemptAsync(
                attempt.Id,
                new TeamsSendResult(false, TeamsReminderStatus.Failed, "Treinamento nao esta publicado."),
                DateTime.UtcNow);
            return true;
        }

        var trainingUrl = $"{_publicBaseUrl}/Training/Start/{training.Id}";
        var callbackUrl = $"{_publicBaseUrl}/api/teams/reminder-result";
        var submission = await webhook.SubmitTrainingReminderAsync(
            attempt,
            training.Title,
            training.EstimatedMinutes,
            trainingUrl,
            callbackUrl,
            cancellationToken);

        if (submission.Accepted)
        {
            await repository.MarkTeamsReminderSubmittedAsync(attempt.Id, submission.HttpStatusCode, DateTime.UtcNow);
            await audit.WriteAsync("training-teams-reminder", "workflow-accepted", attempt.Actor, new
            {
                attempt.TrainingId,
                attempt.SamAccountName,
                attempt.Recipient,
                attempt.RequestId,
                submission.HttpStatusCode
            });
            return true;
        }

        if (submission.Uncertain)
        {
            await repository.MarkTeamsReminderSubmissionUncertainAsync(attempt.Id, submission.Error ?? "Resultado de submissao incerto.", DateTime.UtcNow);
            await audit.WriteAsync("training-teams-reminder", "submission-uncertain", attempt.Actor, new
            {
                attempt.TrainingId,
                attempt.SamAccountName,
                attempt.Recipient,
                attempt.RequestId,
                submission.Error
            });
            return true;
        }

        await repository.CompleteTeamsReminderAttemptAsync(
            attempt.Id,
            new TeamsSendResult(false, TeamsReminderStatus.Failed, submission.Error, submission.HttpStatusCode),
            DateTime.UtcNow);
        await audit.WriteAsync("training-teams-reminder", "failed", attempt.Actor, new
        {
            attempt.TrainingId,
            attempt.SamAccountName,
            attempt.Recipient,
            attempt.RequestId,
            submission.HttpStatusCode,
            submission.Error
        });
        return true;
    }
}
