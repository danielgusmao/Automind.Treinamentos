using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Automind.Treinamentos.Models;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class AdAuthenticationService
{
    private readonly ActiveDirectoryOptions _options;
    private readonly ILogger<AdAuthenticationService> _logger;

    public AdAuthenticationService(IOptions<ActiveDirectoryOptions> options, ILogger<AdAuthenticationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public AdUser? Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var sam = NormalizeSam(username);
        using var context = new PrincipalContext(ContextType.Domain, _options.Domain);
        if (!context.ValidateCredentials(sam, password, ContextOptions.Negotiate))
            return null;

        return LoadEligibleUser(context, sam);
    }

    public bool IsEnabledAndEligible(string samAccountName)
    {
        var sam = NormalizeSam(samAccountName);
        if (string.IsNullOrWhiteSpace(sam)) return false;

        try
        {
            using var context = new PrincipalContext(ContextType.Domain, _options.Domain);
            return LoadEligibleUser(context, sam) is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao revalidar estado/elegibilidade AD de {SamAccountName}.", sam);
            return false;
        }
    }

    private AdUser? LoadEligibleUser(PrincipalContext context, string sam)
    {
        using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sam);
        if (user is null || user.Enabled != true)
            return null;

        var email = user.EmailAddress?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(_options.AllowedMailSuffix) ||
            !email.EndsWith(_options.AllowedMailSuffix.Trim(), StringComparison.OrdinalIgnoreCase))
            return null;

        var adUser = new AdUser
        {
            SamAccountName = user.SamAccountName ?? sam,
            DisplayName = user.DisplayName ?? user.Name ?? sam,
            Email = email
        };

        try
        {
            if (user.GetUnderlyingObject() is DirectoryEntry entry)
            {
                adUser.JobTitle = entry.Properties["title"]?.Value?.ToString() ?? string.Empty;
                adUser.Department = entry.Properties["department"]?.Value?.ToString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Nao foi possivel carregar title/department de {SamAccountName}.", sam);
        }

        return adUser;
    }

    private static string NormalizeSam(string value)
    {
        var x = (value ?? string.Empty).Trim();
        if (x.Contains('\\')) x = x[(x.LastIndexOf('\\') + 1)..];
        if (x.Contains('@')) x = x[..x.IndexOf('@')];
        return x;
    }
}
