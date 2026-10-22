using Automind.Treinamentos.Models;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsReminderStatusSynchronizer(
    TrainingRepository repository,
    TeamsStatusSharePointService cloudStatus,
    AuditService audit,
    IOptions<TeamsStatusOptions> options,
    ILogger<TeamsReminderStatusSynchronizer> logger)
{
    private readonly TeamsStatusOptions _options = options.Value;

    public bool IsConfigured => cloudStatus.IsConfigured;

    public async Task<int> SyncOutstandingAsync(CancellationToken cancellationToken = default)
    {
        if (!cloudStatus.IsConfigured) return 0;

        var attempts = await repository.GetOutstandingTeamsReminderAttemptsAsync(Math.Clamp(_options.BatchSize, 1, 250));
        var changed = 0;

        foreach (var attempt in attempts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TeamsCloudStatus? cloud;
            try
            {
                cloud = await cloudStatus.GetStatusAsync(attempt.RequestId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao sincronizar status Teams. RequestId {RequestId}.", attempt.RequestId);
                continue;
            }

            if (cloud is null) continue;

            var status = cloud.Success ? TeamsReminderStatus.Sent : TeamsReminderStatus.Failed;
            var error = cloud.Success
                ? null
                : (string.IsNullOrWhiteSpace(cloud.Error) ? "O Workflow informou falha no envio ao Teams." : cloud.Error);
            var finishedAtUtc = cloud.SentAtUtc ?? cloud.CreatedAtUtc ?? DateTime.UtcNow;
            var result = new TeamsSendResult(
                cloud.Success,
                status,
                error,
                attempt.HttpStatusCode,
                cloud.WorkflowRunId,
                cloud.MessageId);

            if (!await repository.CompleteTeamsReminderAttemptByCloudStatusAsync(attempt.RequestId, result, finishedAtUtc))
                continue;

            changed++;
            await audit.WriteAsync(
                "training-teams-reminder-cloud-sync",
                cloud.Success ? "success" : "failed",
                "sharepoint-status",
                new
                {
                    attempt.TrainingId,
                    attempt.SamAccountName,
                    attempt.Recipient,
                    attempt.RequestId,
                    cloud.WorkflowRunId,
                    cloud.MessageId,
                    finishedAtUtc,
                    error
                });
        }

        return changed;
    }
}
