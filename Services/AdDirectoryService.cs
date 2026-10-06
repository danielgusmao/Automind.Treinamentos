using System.DirectoryServices;
using Automind.Treinamentos.Models;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class AdDirectoryService
{
    private readonly ActiveDirectoryOptions _options;

    public AdDirectoryService(IOptions<ActiveDirectoryOptions> options)
    {
        _options = options.Value;
    }

    public Task<List<AdUser>> GetEligibleUsersAsync()
    {
        return Task.Run(() =>
        {
            var result = new List<AdUser>();
            using var root = new DirectoryEntry($"LDAP://{_options.BaseDn}");
            using var searcher = new DirectorySearcher(root)
            {
                Filter = $"(&(objectCategory=person)(objectClass=user)(mail=*{EscapeLdapFilter(_options.AllowedMailSuffix)})(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                PageSize = 1000,
                SearchScope = SearchScope.Subtree
            };
            foreach (var p in new[] { "sAMAccountName", "displayName", "mail", "title", "department" })
                searcher.PropertiesToLoad.Add(p);

            foreach (SearchResult item in searcher.FindAll())
            {
                var sam = First(item, "sAMAccountName");
                var mail = First(item, "mail");
                if (string.IsNullOrWhiteSpace(sam) || string.IsNullOrWhiteSpace(mail)) continue;
                if (!mail.EndsWith(_options.AllowedMailSuffix, StringComparison.OrdinalIgnoreCase)) continue;

                result.Add(new AdUser
                {
                    SamAccountName = sam,
                    DisplayName = First(item, "displayName") is { Length: > 0 } dn ? dn : sam,
                    Email = mail,
                    JobTitle = First(item, "title"),
                    Department = First(item, "department")
                });
            }

            return result.OrderBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
        });
    }

    private static string First(SearchResult r, string name) =>
        r.Properties.Contains(name) && r.Properties[name].Count > 0
            ? r.Properties[name][0]?.ToString() ?? ""
            : "";

    private static string EscapeLdapFilter(string value) => value
        .Replace("\\", "\\5c")
        .Replace("*", "\\2a")
        .Replace("(", "\\28")
        .Replace(")", "\\29")
        .Replace("\0", "\\00");
}
