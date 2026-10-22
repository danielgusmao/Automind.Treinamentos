using System.Net;
using System.Net.Http.Json;
using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public sealed class TeamsWebhookService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<TeamsWebhookService> logger)
{
    private string WebhookUrl => configuration["Automind:Teams:WebhookUrl"]?.Trim() ?? string.Empty;
    public bool IsConfigured =>
        configuration.GetValue("Automind:Teams:Enabled", true) &&
        Uri.TryCreate(WebhookUrl, UriKind.Absolute, out var uri) &&
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    public async Task<TeamsWorkflowSubmissionResult> SubmitTrainingReminderAsync(
        TeamsReminderHistory attempt,
        string trainingTitle,
        int estimatedMinutes,
        string trainingUrl,
        string callbackUrl,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new TeamsWorkflowSubmissionResult(false, false, null, "A integracao Teams esta desabilitada ou sem webhook configurado.");

        if (string.IsNullOrWhiteSpace(attempt.Recipient))
            return new TeamsWorkflowSubmissionResult(false, false, null, "Destinatario sem e-mail/UPN.");

        var firstName = string.IsNullOrWhiteSpace(attempt.DisplayName)
            ? "colaborador"
            : attempt.DisplayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "colaborador";

        var safeFirstName = WebUtility.HtmlEncode(firstName);
        var safeTitle = WebUtility.HtmlEncode(trainingTitle);
        var safeUrl = WebUtility.HtmlEncode(trainingUrl);

        var text =
            $"Ola, {safeFirstName}. Voce possui o treinamento <strong>{safeTitle}</strong> pendente.<br>" +
            $"Tempo estimado: {estimatedMinutes} minutos.<br>" +
            $"Este \u00e9 um ambiente interno, ent\u00e3o esteja conectado \u00e0 VPN.<br>" +
            $"<strong>Acessar treinamento:</strong> <a href=\"{safeUrl}\">{safeUrl}</a>";

        try
        {
            var client = httpClientFactory.CreateClient("TeamsWebhook");
            using var response = await client.PostAsJsonAsync(
                WebhookUrl,
                new
                {
                    recipient = attempt.Recipient.Trim(),
                    text,
                    requestId = attempt.RequestId,
                    callbackToken = attempt.CallbackToken,
                    callbackUrl
                },
                cancellationToken);

            var statusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Workflow Teams recusou a solicitacao. HTTP {StatusCode}; RequestId {RequestId}.", statusCode, attempt.RequestId);
                return new TeamsWorkflowSubmissionResult(false, false, statusCode, $"O webhook Teams respondeu HTTP {statusCode}.");
            }

            // O gatilho do Power Automate normalmente devolve 202 antes de a acao do Teams terminar.
            // Portanto, 2xx confirma apenas que a execucao foi aceita, nunca que a mensagem foi enviada.
            return new TeamsWorkflowSubmissionResult(true, false, statusCode, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            logger.LogWarning(exception, "Timeout ao entregar solicitacao ao Workflow Teams. RequestId {RequestId}.", attempt.RequestId);
            return new TeamsWorkflowSubmissionResult(false, true, null, "Nao foi possivel confirmar se o Workflow recebeu a solicitacao.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Falha de rede ao chamar Workflow Teams. RequestId {RequestId}.", attempt.RequestId);
            return new TeamsWorkflowSubmissionResult(false, true, null, "Nao foi possivel confirmar se o Workflow recebeu a solicitacao.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Falha ao chamar Workflow Teams. RequestId {RequestId}.", attempt.RequestId);
            return new TeamsWorkflowSubmissionResult(false, false, null, $"Falha tecnica ao chamar o Workflow ({exception.GetType().Name}).");
        }
    }
}

public sealed record TeamsWorkflowSubmissionResult(
    bool Accepted,
    bool Uncertain,
    int? HttpStatusCode,
    string? Error);

public sealed record TeamsSendResult(
    bool Success,
    string Status,
    string? Error,
    int? HttpStatusCode = null,
    string? WorkflowRunId = null,
    string? MessageId = null);
