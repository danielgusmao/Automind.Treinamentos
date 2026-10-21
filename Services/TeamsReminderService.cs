using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public sealed class TeamsReminderService(
    TrainingRepository repository,
    TeamsWebhookService webhook,
    AuditService audit)
{
    public async Task<TeamsReminderDispatchResult> SendAsync(
        Training training,
        AdUser user,
        string trainingUrl,
        string source,
        string actor,
        string? cycleKey,
        CancellationToken cancellationToken = default)
    {
        var attemptedAtUtc = DateTime.UtcNow;
        var historyId = await repository.BeginTeamsReminderAttemptAsync(
            training.Id,
            user,
            source,
            actor,
            cycleKey,
            attemptedAtUtc);

        if (historyId is null)
            return new TeamsReminderDispatchResult(false, null);

        TeamsSendResult result;
        try
        {
            result = await webhook.SendTrainingReminderAsync(
                user.Email,
                user.DisplayName,
                training.Title,
                training.EstimatedMinutes,
                trainingUrl,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            result = new TeamsSendResult(
                false,
                TeamsReminderStatus.Failed,
                "A tentativa foi interrompida antes da confirmacao do envio.");
            await repository.CompleteTeamsReminderAttemptAsync(historyId.Value, result, DateTime.UtcNow);
            await WriteAuditAsync(training, user, trainingUrl, source, actor, result);
            throw;
        }

        await repository.CompleteTeamsReminderAttemptAsync(historyId.Value, result, DateTime.UtcNow);
        await WriteAuditAsync(training, user, trainingUrl, source, actor, result);
        return new TeamsReminderDispatchResult(true, result);
    }

    private Task WriteAuditAsync(
        Training training,
        AdUser user,
        string trainingUrl,
        string source,
        string actor,
        TeamsSendResult result)
    {
        return audit.WriteAsync("training-teams-reminder", result.Success ? "success" : "failed", actor, new
        {
            trainingId = training.Id,
            training.Title,
            recipient = user.Email,
            sam = user.SamAccountName,
            link = trainingUrl,
            source,
            deliveryStatus = result.Status,
            httpStatusCode = result.HttpStatusCode,
            workflowRunId = result.WorkflowRunId,
            messageId = result.MessageId,
            error = result.Error
        });
    }
}

public sealed record TeamsReminderDispatchResult(bool AttemptCreated, TeamsSendResult? SendResult);
