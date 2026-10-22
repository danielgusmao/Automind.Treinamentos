namespace Automind.Treinamentos.Services;

public sealed class ActiveDirectoryOptions
{
    public string Domain { get; set; } = "automind.com.br";
    public string BaseDn { get; set; } = "DC=automind,DC=com,DC=br";
    public string AdminGroup { get; set; } = "_treinamentos";
    public string AuthorizationServer { get; set; } = "";
    public string AllowedMailSuffix { get; set; } = "@automind.com.br";
}

public sealed class StorageOptions
{
    public string RootPath { get; set; } = "App_Data";
}

public sealed class TeamsOptions
{
    public bool Enabled { get; set; } = true;
    public string WebhookUrl { get; set; } = "";
    public int DispatchIntervalSeconds { get; set; } = 13;
}

public sealed class TeamsStatusOptions
{
    public bool Enabled { get; set; } = true;
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string SiteHostname { get; set; } = "automindautomacao.sharepoint.com";
    public string SitePath { get; set; } = "/sites/GSTI-NOC";
    public string ListName { get; set; } = "AutomindTreinamentosTeamsStatus";
    public int SyncIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 50;
}

public sealed class PortalOptions
{
    public string PublicBaseUrl { get; set; } = "http://treinamentos.automind.com.br";
}
