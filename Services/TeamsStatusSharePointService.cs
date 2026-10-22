using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class TeamsStatusSharePointService(
    IHttpClientFactory httpClientFactory,
    IOptions<TeamsStatusOptions> options,
    ILogger<TeamsStatusSharePointService> logger)
{
    private readonly TeamsStatusOptions _options = options.Value;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private readonly SemaphoreSlim _resourceLock = new(1, 1);
    private string _accessToken = string.Empty;
    private DateTimeOffset _accessTokenExpiresAt = DateTimeOffset.MinValue;
    private string _siteId = string.Empty;
    private string _listId = string.Empty;
    private DateTimeOffset _nextRetryAt = DateTimeOffset.MinValue;

    public bool IsConfigured =>
        _options.Enabled &&
        !string.IsNullOrWhiteSpace(_options.TenantId) &&
        !string.IsNullOrWhiteSpace(_options.ClientId) &&
        !string.IsNullOrWhiteSpace(_options.ClientSecret) &&
        !string.IsNullOrWhiteSpace(_options.SiteHostname) &&
        !string.IsNullOrWhiteSpace(_options.SitePath) &&
        !string.IsNullOrWhiteSpace(_options.ListName);

    public async Task<TeamsCloudStatus?> GetStatusAsync(string requestId, CancellationToken cancellationToken = default)
    {
        requestId = (requestId ?? string.Empty).Trim();
        if (!IsConfigured || string.IsNullOrWhiteSpace(requestId)) return null;
        if (_nextRetryAt > DateTimeOffset.UtcNow) return null;

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token)) return null;

        var resources = await GetResourceIdsAsync(token, cancellationToken);
        if (resources is null) return null;

        var escapedRequestId = requestId.Replace("'", "''", StringComparison.Ordinal);
        var filter = Uri.EscapeDataString($"fields/Title eq '{escapedRequestId}'");
        var url =
            $"https://graph.microsoft.com/v1.0/sites/{Uri.EscapeDataString(resources.Value.SiteId)}/lists/{Uri.EscapeDataString(resources.Value.ListId)}/items" +
            $"?$expand=fields($select=Title,Recipient,Success,SentAtUtc,WorkflowRunId,MessageId,Error)&$filter={filter}&$top=1";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");

        var client = httpClientFactory.CreateClient("TeamsStatusGraph");
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Falha ao consultar status Teams no SharePoint via Graph. HTTP {StatusCode}; RequestId {RequestId}.",
                (int)response.StatusCode,
                requestId);
            BackOff();
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("value", out var values) || values.ValueKind != JsonValueKind.Array)
            return null;

        var item = values.EnumerateArray().FirstOrDefault();
        if (item.ValueKind == JsonValueKind.Undefined) return null;
        if (!item.TryGetProperty("fields", out var fields) || fields.ValueKind != JsonValueKind.Object) return null;

        var title = ReadString(fields, "Title");
        if (!string.Equals(title, requestId, StringComparison.OrdinalIgnoreCase)) return null;

        var success = ReadBoolean(fields, "Success");
        if (success is null) return null;

        var sentAtUtc = ReadDateTime(fields, "SentAtUtc");
        var createdAtUtc = item.TryGetProperty("createdDateTime", out var created)
            ? ParseDateTime(created)
            : null;

        return new TeamsCloudStatus(
            requestId,
            ReadString(fields, "Recipient"),
            success.Value,
            sentAtUtc,
            createdAtUtc,
            ReadString(fields, "WorkflowRunId"),
            ReadString(fields, "MessageId"),
            ReadString(fields, "Error"));
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(2))
            return _accessToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(2))
                return _accessToken;

            var tenantId = Uri.EscapeDataString(_options.TenantId.Trim());
            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId.Trim(),
                ["client_secret"] = _options.ClientSecret,
                ["scope"] = "https://graph.microsoft.com/.default",
                ["grant_type"] = "client_credentials"
            });

            var client = httpClientFactory.CreateClient("TeamsStatusGraph");
            using var response = await client.PostAsync(tokenUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Falha ao obter token Graph para status Teams. HTTP {StatusCode}.", (int)response.StatusCode);
                BackOff();
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!document.RootElement.TryGetProperty("access_token", out var accessTokenElement)) return null;

            var token = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(token)) return null;

            var expiresIn = document.RootElement.TryGetProperty("expires_in", out var expiresElement) && expiresElement.TryGetInt32(out var seconds)
                ? seconds
                : 3600;

            _accessToken = token;
            _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(300, expiresIn));
            _nextRetryAt = DateTimeOffset.MinValue;
            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<(string SiteId, string ListId)?> GetResourceIdsAsync(string token, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_siteId) && !string.IsNullOrWhiteSpace(_listId))
            return (_siteId, _listId);

        await _resourceLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_siteId) && !string.IsNullOrWhiteSpace(_listId))
                return (_siteId, _listId);

            var sitePath = _options.SitePath.Trim();
            if (!sitePath.StartsWith('/')) sitePath = "/" + sitePath;
            var siteUrl = $"https://graph.microsoft.com/v1.0/sites/{_options.SiteHostname.Trim()}:{sitePath}?$select=id";
            var siteId = await GetIdAsync(siteUrl, token, cancellationToken);
            if (string.IsNullOrWhiteSpace(siteId)) return null;

            var listName = Uri.EscapeDataString(_options.ListName.Trim());
            var listUrl = $"https://graph.microsoft.com/v1.0/sites/{Uri.EscapeDataString(siteId)}/lists/{listName}?$select=id,displayName";
            var listId = await GetIdAsync(listUrl, token, cancellationToken);
            if (string.IsNullOrWhiteSpace(listId)) return null;

            _siteId = siteId;
            _listId = listId;
            return (_siteId, _listId);
        }
        finally
        {
            _resourceLock.Release();
        }
    }

    private async Task<string?> GetIdAsync(string url, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var client = httpClientFactory.CreateClient("TeamsStatusGraph");
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Falha ao resolver recurso SharePoint via Graph. HTTP {StatusCode}.", (int)response.StatusCode);
            BackOff();
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
    }

    private void BackOff()
    {
        _nextRetryAt = DateTimeOffset.UtcNow.AddSeconds(30);
    }

    private static string ReadString(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return string.Empty;
        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
    }

    private static bool? ReadBoolean(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.True) return true;
        if (value.ValueKind == JsonValueKind.False) return false;
        if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var parsed)) return parsed;
        return null;
    }

    private static DateTime? ReadDateTime(JsonElement fields, string name)
    {
        if (!fields.TryGetProperty(name, out var value)) return null;
        return ParseDateTime(value);
    }

    private static DateTime? ParseDateTime(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String) return null;
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
