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

        adUser.IsAdministrator = IsInAdminGroup(user);
        return adUser;
    }

    // Revalida a autorizacao sem precisar da senha. Usado tambem durante a validacao do cookie.
    public bool IsInAdminGroup(string samAccountName)
    {
        if (string.IsNullOrWhiteSpace(samAccountName)) return false;

        try
        {
            using var context = new PrincipalContext(ContextType.Domain, _options.Domain);
            using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, NormalizeSam(samAccountName));
            return user is not null && user.Enabled == true && IsInAdminGroup(user);
        }
        catch
        {
            // Falha de AD nunca eleva privilegio.
            return false;
        }
    }

    private bool IsInAdminGroup(UserPrincipal user)
    {
        try
        {
            foreach (var group in user.GetAuthorizationGroups())
            {
                using (group)
                {
                    if (string.Equals(group.SamAccountName, _options.AdminGroup, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
        }
        catch
        {
            // Algumas florestas possuem SIDs/grupos que nao resolvem. Nao elevar privilegio em caso de erro.
        }

        return false;
    }

    private static string NormalizeSam(string value)
    {
        var x = value.Trim();
        if (x.Contains('\\')) x = x[(x.LastIndexOf('\\') + 1)..];
        if (x.Contains('@')) x = x[..x.IndexOf('@')];
        return x;
    }
}
