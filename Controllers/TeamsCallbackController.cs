using Automind.Treinamentos.Models;
using Automind.Treinamentos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automind.Treinamentos.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/teams/reminder-result")]
public sealed class TeamsCallbackController(
    TrainingRepository repository,
    AuditService audit,
    ILogger<TeamsCallbackController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Complete([FromBody] TeamsReminderCallbackRequest request)
    {
        request.RequestId = (request.RequestId ?? string.Empty).Trim();
        request.CallbackToken = (request.CallbackToken ?? string.Empty).Trim();
        request.WorkflowRunId = (request.WorkflowRunId ?? string.Empty).Trim();
        request.MessageId = (request.MessageId ?? string.Empty).Trim();
        request.Error = (request.Error ?? string.Empty).Trim();

        if (request.RequestId.Length is < 20 or > 80 || request.CallbackToken.Length is < 32 or > 160)
            return BadRequest();

        if (request.WorkflowRunId.Length > 200) request.WorkflowRunId = request.WorkflowRunId[..200];
        if (request.MessageId.Length > 300) request.MessageId = request.MessageId[..300];
        if (request.Error.Length > 1000) request.Error = request.Error[..1000];

        var finishedAtUtc = request.Success
            ? (request.SentAtUtc?.ToUniversalTime() ?? DateTime.UtcNow)
            : DateTime.UtcNow;

        var result = new TeamsSendResult(
            request.Success,
            request.Success ? TeamsReminderStatus.Sent : TeamsReminderStatus.Failed,
            request.Success ? null : (string.IsNullOrWhiteSpace(request.Error) ? "O Workflow informou falha no envio ao Teams." : request.Error),
            request.HttpStatusCode,
            request.WorkflowRunId,
            request.MessageId);

        var completed = await repository.CompleteTeamsReminderAttemptByCallbackAsync(
            request.RequestId,
            request.CallbackToken,
            result,
            finishedAtUtc);

        if (!completed)
        {
            logger.LogWarning("Callback Teams rejeitado ou desconhecido. RequestId {RequestId}.", request.RequestId);
            return NotFound();
        }

        await audit.WriteAsync("training-teams-reminder-callback", request.Success ? "success" : "failed", "teams-workflow", new
        {
            request.RequestId,
            request.WorkflowRunId,
            request.MessageId,
            request.HttpStatusCode,
            request.Error,
            finishedAtUtc
        });

        return NoContent();
    }
}
