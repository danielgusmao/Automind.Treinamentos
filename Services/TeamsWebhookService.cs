using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsWebhookService
{
    private readonly HttpClient _http;
    private readonly TeamsOptions _options;

    public TeamsWebhookService(HttpClient http, IOptions<TeamsOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public bool IsConfigured =>
        _options.Enabled &&
        Uri.TryCreate(_options.WebhookUrl, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps;

    public async Task<TeamsSendResult> SendTrainingReminderAsync(
        string recipient,
        string displayName,
        string trainingTitle,
        int estimatedMinutes,
        string trainingUrl,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new TeamsSendResult(false, "Webhook do Teams nao configurado para este aplicativo.");

        if (string.IsNullOrWhiteSpace(recipient))
            return new TeamsSendResult(false, "Destinatario sem e-mail/UPN.");

        var safeName = WebUtility.HtmlEncode(displayName);
        var safeTitle = WebUtility.HtmlEncode(trainingTitle);
        var safeUrl = WebUtility.HtmlEncode(trainingUrl);
        var text = $"Ola, {safeName}.<br><br>Voce ainda possui o treinamento <strong>{safeTitle}</strong> pendente.<br>Tempo estimado: {estimatedMinutes} minutos.<br><br>Acesse o treinamento pelo link:<br><a href=\"{safeUrl}\">{safeUrl}</a><br><br>Mensagem automatica do portal Automind.Treinamentos.";

        try
        {
            using var response = await _http.PostAsJsonAsync(
                _options.WebhookUrl,
                new { recipient = recipient.Trim(), text },
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return new TeamsSendResult(true, null);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var detail = string.IsNullOrWhiteSpace(body)
                ? $"HTTP {(int)response.StatusCode}"
                : $"HTTP {(int)response.StatusCode}: {body}";
            return new TeamsSendResult(false, detail.Length > 700 ? detail[..700] : detail);
        }
        catch (Exception ex)
        {
            return new TeamsSendResult(false, ex.Message);
        }
    }
}

public sealed record TeamsSendResult(bool Success, string? Error);
