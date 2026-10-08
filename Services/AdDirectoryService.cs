using System.DirectoryServices;
using Automind.Treinamentos.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Automind.Treinamentos.Services;

public sealed class AdDirectoryService
{
    private const string CacheKey = "ad-eligible-users";
    private readonly ActiveDirectoryOptions _options;
    private readonly IMemoryCache _cache;

    public AdDirectoryService(IOptions<ActiveDirectoryOptions> options, IMemoryCache cache)
    {
        _options = options.Value;
        _cache = cache;
    }

    public Task<List<AdUser>> GetEligibleUsersAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<AdUser>? cached) && cached is not null)
            return Task.FromResult(cached.Select(Clone).ToList());

        return Task.Run(() =>
        {
            var result = new List<AdUser>();
            using var root = new DirectoryEntry($"LDAP://{_options.BaseDn}");
            using var searcher = new DirectorySearcher(root)
            {
                Filter = $"(&(objectCategory=person)(objectClass=user)(mail=*{EscapeLdapFilter(_options.AllowedMailSuffix)})(!(userAccountControl:1.2.840.113556.1.4.803:=2)))",
                PageSize = 1000,
                SearchScope = SearchScope.Subtree,
                ClientTimeout = TimeSpan.FromSeconds(10),
                ServerTimeLimit = TimeSpan.FromSeconds(10)
            };
            foreach (var p in new[] { "sAMAccountName", "displayName", "mail", "title", "department" })
                searcher.PropertiesToLoad.Add(p);

            using var found = searcher.FindAll();
            foreach (SearchResult item in found)
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

            var ordered = result.OrderBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
            _cache.Set(CacheKey, ordered.Select(Clone).ToList(), TimeSpan.FromMinutes(1));
            return ordered;
        });
    }

    private static AdUser Clone(AdUser user) => new()
    {
        SamAccountName = user.SamAccountName,
        DisplayName = user.DisplayName,
        Email = user.Email,
        JobTitle = user.JobTitle,
        Department = user.Department
    };

    private static string First(SearchResult r, string name) =>
        r.Properties.Contains(name) && r.Properties[name].Count > 0
            ? r.Properties[name][0]?.ToString() ?? string.Empty
            : string.Empty;

    private static string EscapeLdapFilter(string value) => (value ?? string.Empty)
        .Replace("\\", "\\5c")
        .Replace("*", "\\2a")
        .Replace("(", "\\28")
        .Replace(")", "\\29")
        .Replace("\0", "\\00");
}
