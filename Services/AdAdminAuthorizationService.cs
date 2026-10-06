using System.DirectoryServices;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

/// <summary>
/// Consulta a autorizacao administrativa diretamente no LDAP a cada requisicao.
/// Nenhuma decisao de privilegio e persistida em cookie ou sessao.
/// </summary>
public sealed class AdAdminAuthorizationService
{
    private const string MatchingRuleInChain = "1.2.840.113556.1.4.1941";

    private readonly ActiveDirectoryOptions _options;
    private readonly ILogger<AdAdminAuthorizationService> _logger;

    public AdAdminAuthorizationService(
        IOptions<ActiveDirectoryOptions> options,
        ILogger<AdAdminAuthorizationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsInAdminGroup(string samAccountName)
    {
        var sam = NormalizeSam(samAccountName);
        if (string.IsNullOrWhiteSpace(sam) || string.IsNullOrWhiteSpace(_options.AdminGroup))
            return false;

        try
        {
            using var root = CreateDirectoryRoot();

            var groupDn = FindGroupDistinguishedName(root, _options.AdminGroup);
            if (string.IsNullOrWhiteSpace(groupDn))
            {
                _logger.LogWarning(
                    "Grupo administrativo {AdminGroup} nao foi localizado no Active Directory usando {LdapTarget}.",
                    _options.AdminGroup,
                    GetLdapTarget());
                return false;
            }

            // Consulta o AD diretamente, em vez de usar GetAuthorizationGroups/tokenGroups.
            // O matching rule in chain contempla membership direta e grupos aninhados.
            using var searcher = new DirectorySearcher(root)
            {
                SearchScope = SearchScope.Subtree,
                PageSize = 1,
                Filter =
                    $"(&(objectCategory=person)(objectClass=user)" +
                    $"(sAMAccountName={EscapeLdapFilter(sam)})" +
                    $"(!(userAccountControl:1.2.840.113556.1.4.803:=2))" +
                    $"(memberOf:{MatchingRuleInChain}:={EscapeLdapFilter(groupDn)}))"
            };
            searcher.PropertiesToLoad.Add("distinguishedName");

            return searcher.FindOne() is not null;
        }
        catch (Exception ex)
        {
            // Autorizacao administrativa sempre falha fechada.
            _logger.LogWarning(
                ex,
                "Falha ao revalidar no AD o grupo administrativo {AdminGroup} para {SamAccountName} usando {LdapTarget}.",
                _options.AdminGroup,
                sam,
                GetLdapTarget());
            return false;
        }
    }

    private DirectoryEntry CreateDirectoryRoot()
    {
        var path = $"LDAP://{GetLdapTarget()}/{_options.BaseDn}";
        return new DirectoryEntry(path, null, null, AuthenticationTypes.Secure);
    }

    private string GetLdapTarget() =>
        string.IsNullOrWhiteSpace(_options.AuthorizationServer)
            ? _options.Domain.Trim()
            : _options.AuthorizationServer.Trim();

    private static string? FindGroupDistinguishedName(DirectoryEntry root, string groupSamAccountName)
    {
        using var searcher = new DirectorySearcher(root)
        {
            SearchScope = SearchScope.Subtree,
            PageSize = 1,
            Filter = $"(&(objectCategory=group)(sAMAccountName={EscapeLdapFilter(groupSamAccountName)}))"
        };
        searcher.PropertiesToLoad.Add("distinguishedName");

        var result = searcher.FindOne();
        if (result is null || !result.Properties.Contains("distinguishedName") || result.Properties["distinguishedName"].Count == 0)
            return null;

        return result.Properties["distinguishedName"][0]?.ToString();
    }

    private static string NormalizeSam(string value)
    {
        var x = (value ?? string.Empty).Trim();
        if (x.Contains('\\')) x = x[(x.LastIndexOf('\\') + 1)..];
        if (x.Contains('@')) x = x[..x.IndexOf('@')];
        return x;
    }

    private static string EscapeLdapFilter(string value) => value
        .Replace("\\", "\\5c")
        .Replace("*", "\\2a")
        .Replace("(", "\\28")
        .Replace(")", "\\29")
        .Replace("\0", "\\00");
}
