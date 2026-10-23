using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsStatusSharePointService(
    IHttpClientFactory httpClientFactory,
    IOptions<TeamsStatusOptions> options,
    ILogger<TeamsStatusSharePointService> logger)
{
    private readonly TeamsStatusOptions _options = options.Value;
    private DateTimeOffset _nextRetryAt = DateTimeOffset.MinValue;

    public bool IsConfigured =>
        _options.Enabled &&
        Uri.TryCreate((_options.LookupUrl ?? string.Empty).Trim(), UriKind.Absolute, out var uri) &&
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    public async Task<TeamsCloudStatus?> GetStatusAsync(string requestId, CancellationToken cancellationToken = default)
    {
        requestId = (requestId ?? string.Empty).Trim();
        if (!IsConfigured || string.IsNullOrWhiteSpace(requestId)) return null;
        if (_nextRetryAt > DateTimeOffset.UtcNow) return null;

        var client = httpClientFactory.CreateClient("TeamsStatusLookup");
        using var response = await client.PostAsJsonAsync(
            _options.LookupUrl.Trim(),
            new { requestId },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Falha ao consultar status Teams pelo Power Automate. HTTP {StatusCode}; RequestId {RequestId}.",
                (int)response.StatusCode,
                requestId);
            BackOff();
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        if (ReadBoolean(root, "found") is not true) return null;

        var returnedRequestId = ReadString(root, "requestId");
        if (!string.Equals(returnedRequestId, requestId, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Resposta de status Teams retornou RequestId diferente do solicitado.");
            return null;
        }

        var success = ReadBoolean(root, "success");
        if (success is null) return null;

        _nextRetryAt = DateTimeOffset.MinValue;
        return new TeamsCloudStatus(
            requestId,
            ReadString(root, "recipient"),
            success.Value,
            ReadDateTime(root, "sentAtUtc"),
            null,
            ReadString(root, "workflowRunId"),
            ReadString(root, "messageId"),
            ReadString(root, "error"));
    }

    private void BackOff() => _nextRetryAt = DateTimeOffset.UtcNow.AddSeconds(30);

    private static string ReadString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null)
            return string.Empty;
        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
    }

    private static bool? ReadBoolean(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return null;
        if (value.ValueKind == JsonValueKind.True) return true;
        if (value.ValueKind == JsonValueKind.False) return false;
        if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var parsed)) return parsed;
        return null;
    }

    private static DateTime? ReadDateTime(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.String) return null;
        if (!DateTimeOffset.TryParse(value.GetString(), out var parsed)) return null;
        return parsed.UtcDateTime;
    }
}

public sealed record TeamsCloudStatus(
    string RequestId,
    string Recipient,
    bool Success,
    DateTime? SentAtUtc,
    DateTime? CreatedAtUtc,
    string WorkflowRunId,
    string MessageId,
    string Error);
