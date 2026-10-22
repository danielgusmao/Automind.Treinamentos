using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsReminderStatusSyncWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TeamsStatusOptions> options,
    ILogger<TeamsReminderStatusSyncWorker> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(Math.Clamp(options.Value.SyncIntervalSeconds, 10, 300));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var synchronizer = scope.ServiceProvider.GetRequiredService<TeamsReminderStatusSynchronizer>();
                await synchronizer.SyncOutstandingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha no worker de sincronizacao de status Teams/SharePoint.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
