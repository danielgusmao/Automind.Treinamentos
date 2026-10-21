using System.Security.Cryptography;
using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public sealed class TeamsReminderService(
    TrainingRepository repository,
    AuditService audit)
{
    public async Task<TeamsReminderQueueResult> QueueAsync(
        Training training,
        AdUser user,
        string source,
        string actor,
        string? cycleKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var attemptedAtUtc = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString("N");
        var callbackToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var historyId = await repository.BeginTeamsReminderAttemptAsync(
            training.Id,
            user,
            source,
            actor,
            cycleKey,
            requestId,
            callbackToken,
            attemptedAtUtc);

        if (historyId is null)
            return new TeamsReminderQueueResult(false, null);

        await audit.WriteAsync("training-teams-reminder", "queued", actor, new
        {
            trainingId = training.Id,
            training.Title,
            recipient = user.Email,
            sam = user.SamAccountName,
            source,
            requestId
        });

        return new TeamsReminderQueueResult(true, historyId);
    }
}

public sealed record TeamsReminderQueueResult(bool Queued, long? HistoryId);
