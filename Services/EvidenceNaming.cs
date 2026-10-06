namespace Automind.Treinamentos.Services;

public static class EvidenceNaming
{
    public static string CollaboratorFolder(string samAccountName)
        => ShortSegment(samAccountName, 32);

    public static string IndividualFileName(string trainingCode, string trainingVersion, DateTime acceptedAtUtc, string protocol)
    {
        var code = ShortSegment(trainingCode, 20);
        var version = trainingVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? trainingVersion
            : $"v{trainingVersion}";
        version = ShortSegment(version, 16);
        var safeProtocol = ShortSegment(protocol, 32);
        var localDate = acceptedAtUtc.ToLocalTime();
        return $"{code}_{version}_{localDate:yyyyMMdd}_{safeProtocol}.pdf";
    }

    private static string ShortSegment(string value, int maxLength)
    {
        var safe = StorageService.SafeSegment(value);
        return safe.Length <= maxLength ? safe : safe[..maxLength].TrimEnd();
    }
}
