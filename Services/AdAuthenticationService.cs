using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Automind.Treinamentos.Models;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class AdAuthenticationService
{
    private readonly ActiveDirectoryOptions _options;

    public AdAuthenticationService(IOptions<ActiveDirectoryOptions> options)
    {
        _options = options.Value;
    }

    public AdUser? Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var sam = NormalizeSam(username);
        using var context = new PrincipalContext(ContextType.Domain, _options.Domain);
        if (!context.ValidateCredentials(sam, password, ContextOptions.Negotiate))
            return null;

        using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, sam);
        if (user is null || user.Enabled != true)
            return null;

        var adUser = new AdUser
        {
            SamAccountName = user.SamAccountName ?? sam,
            DisplayName = user.DisplayName ?? user.Name ?? sam,
            Email = user.EmailAddress ?? ""
        };

        try
        {
            if (user.GetUnderlyingObject() is DirectoryEntry entry)
            {
                adUser.JobTitle = entry.Properties["title"]?.Value?.ToString() ?? "";
                adUser.Department = entry.Properties["department"]?.Value?.ToString() ?? "";
            }
        }
        catch { }

        // A autorizacao administrativa nao e calculada nem persistida no login.
        // Ela e consultada diretamente no AD em cada requisicao autenticada.
        return adUser;
    }

    private static string NormalizeSam(string value)
    {
        var x = value.Trim();
        if (x.Contains('\\')) x = x[(x.LastIndexOf('\\') + 1)..];
        if (x.Contains('@')) x = x[..x.IndexOf('@')];
        return x;
    }
}
