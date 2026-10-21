using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<TeamsSendResult> SendTrainingReminderAsync(
        string recipient,
        string displayName,
        string trainingTitle,
        int estimatedMinutes,
        string trainingUrl,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new TeamsSendResult(false, TeamsReminderStatus.Failed, "A integra\u00e7\u00e3o Teams est\u00e1 desabilitada ou sem webhook configurado.");

        if (string.IsNullOrWhiteSpace(recipient))
            return new TeamsSendResult(false, TeamsReminderStatus.Failed, "Destinat\u00e1rio sem e-mail/UPN.");

        var firstName = string.IsNullOrWhiteSpace(displayName)
            ? "colaborador"
            : displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "colaborador";

        var safeFirstName = WebUtility.HtmlEncode(firstName);
        var safeTitle = WebUtility.HtmlEncode(trainingTitle);
        var safeUrl = WebUtility.HtmlEncode(trainingUrl);

        var text =
            $"Ol\u00e1, {safeFirstName}. Voc\u00ea possui o treinamento <strong>{safeTitle}</strong> pendente.<br>" +
            $"Tempo estimado: {estimatedMinutes} minutos.<br>" +
            $"<strong>Acessar treinamento:</strong> <a href=\"{safeUrl}\">{safeUrl}</a>";

        try
        {
            var client = httpClientFactory.CreateClient("TeamsWebhook");
            using var response = await client.PostAsJsonAsync(
                WebhookUrl,
                new { recipient = recipient.Trim(), text },
                cancellationToken);

            var statusCode = (int)response.StatusCode;
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var workflow = ParseWorkflowResponse(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Webhook Teams recusou lembrete de treinamento. HTTP {StatusCode}.", statusCode);
                return new TeamsSendResult(false, TeamsReminderStatus.Failed, $"O webhook Teams respondeu HTTP {statusCode}.", statusCode, workflow.WorkflowRunId, workflow.MessageId);
            }

            if (workflow.ExplicitFailure)
            {
                logger.LogWarning("Workflow Teams informou falha apos resposta HTTP {StatusCode}.", statusCode);
                return new TeamsSendResult(false, TeamsReminderStatus.Failed, workflow.Error ?? "O Workflow informou falha no envio.", statusCode, workflow.WorkflowRunId, workflow.MessageId);
            }

            var confirmed = workflow.Delivered || !string.IsNullOrWhiteSpace(workflow.MessageId);
            var deliveryStatus = confirmed ? TeamsReminderStatus.DeliveryConfirmed : TeamsReminderStatus.WorkflowAccepted;
            return new TeamsSendResult(true, deliveryStatus, null, statusCode, workflow.WorkflowRunId, workflow.MessageId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha no envio Teams. Tipo: {ErrorType}; codigo: {Code}", exception.GetType().Name, exception.HResult);
            return new TeamsSendResult(false, TeamsReminderStatus.Failed, $"Falha t\u00e9cnica no envio Teams ({exception.GetType().Name}).");
        }
    }

    private static WorkflowResponse ParseWorkflowResponse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return new WorkflowResponse();

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return new WorkflowResponse();

            var root = doc.RootElement;
            var hasSuccess = TryGetBoolean(root, "success", out var success);
            var delivered = TryGetBoolean(root, "delivered", out var deliveredValue) && deliveredValue;
            var status = TryGetString(root, "status");
            var workflowRunId = TryGetString(root, "workflowRunId");
            var messageId = TryGetString(root, "messageId");
            var error = TryGetString(root, "error");
            var explicitFailure = (hasSuccess && !success) ||
                string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "error", StringComparison.OrdinalIgnoreCase);

            return new WorkflowResponse(delivered, explicitFailure, workflowRunId, messageId, error);
        }
        catch (JsonException)
        {
            return new WorkflowResponse();
        }
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)) continue;
            return property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : property.Value.ToString();
        }
        return null;
    }

    private static bool TryGetBoolean(JsonElement root, string propertyName, out bool value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)) continue;
            if (property.Value.ValueKind == JsonValueKind.True) { value = true; return true; }
            if (property.Value.ValueKind == JsonValueKind.False) { value = false; return true; }
            if (property.Value.ValueKind == JsonValueKind.String && bool.TryParse(property.Value.GetString(), out value)) return true;
        }
        value = false;
        return false;
    }

    private sealed record WorkflowResponse(
        bool Delivered = false,
        bool ExplicitFailure = false,
        string? WorkflowRunId = null,
        string? MessageId = null,
        string? Error = null);
}

public sealed record TeamsSendResult(
    bool Success,
    string Status,
    string? Error,
    int? HttpStatusCode = null,
    string? WorkflowRunId = null,
    string? MessageId = null);
