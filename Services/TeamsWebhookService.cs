using System.Net;
using System.Net.Http.Json;

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
            return new TeamsSendResult(false, "A integracao Teams esta desabilitada ou sem webhook configurado.");

        if (string.IsNullOrWhiteSpace(recipient))
            return new TeamsSendResult(false, "Destinatario sem e-mail/UPN.");

        var firstName = string.IsNullOrWhiteSpace(displayName)
            ? "colaborador"
            : displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "colaborador";

        var safeFirstName = WebUtility.HtmlEncode(firstName);
        var safeTitle = WebUtility.HtmlEncode(trainingTitle);
        var safeUrl = WebUtility.HtmlEncode(trainingUrl);

        var text =
            $"Olá, {safeFirstName}. Você possui o treinamento <strong>{safeTitle}</strong> pendente.<br>" +
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
            if (response.IsSuccessStatusCode)
                return new TeamsSendResult(true, null);

            logger.LogWarning("Webhook Teams recusou lembrete de treinamento. HTTP {StatusCode}.", statusCode);
            return new TeamsSendResult(false, $"O webhook Teams respondeu HTTP {statusCode}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Falha no envio Teams. Tipo: {ErrorType}; codigo: {Code}", exception.GetType().Name, exception.HResult);
            return new TeamsSendResult(false, $"Falha tecnica no envio Teams ({exception.GetType().Name}).");
        }
    }
}

public sealed record TeamsSendResult(bool Success, string? Error);
