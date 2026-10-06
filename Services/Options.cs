namespace Automind.Treinamentos.Services;

public sealed class ActiveDirectoryOptions
{
    public string Domain { get; set; } = "automind.com.br";
    public string BaseDn { get; set; } = "DC=automind,DC=com,DC=br";
    public string AdminGroup { get; set; } = "_informatica";
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
}

public sealed class PortalOptions
{
    public string PublicBaseUrl { get; set; } = "http://treinamentos.automind.com.br";
}
