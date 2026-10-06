namespace Automind.Treinamentos.Models;

public sealed class Training
{
    public long Id { get; set; }
    public string Code { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string SummaryText { get; set; } = "";
    public string Version { get; set; } = "1.0.0";
    public string ContentText { get; set; } = "";
    public int PassingScore { get; set; }
    public int EstimatedMinutes { get; set; } = 8;
    public bool IsPublished { get; set; }
    public bool RequiredForAll { get; set; }
    public string LayoutKey { get; set; } = "generic";
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = "";
}

public sealed class TrainingQuestion
{
    public long Id { get; set; }
    public long TrainingId { get; set; }
    public int Position { get; set; }
    public string Text { get; set; } = "";
    public List<string> Options { get; set; } = new();
    public int CorrectIndex { get; set; }
}

public sealed class TrainingCompletion
{
    public long Id { get; set; }
    public long TrainingId { get; set; }
    public string SamAccountName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Department { get; set; } = "";
    public int Score { get; set; }
    public int Total { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime AcceptedAtUtc { get; set; }
    public int DurationSeconds { get; set; }
    public string Protocol { get; set; } = "";
    public string TrainingSnapshotSha256 { get; set; } = "";
    public string EvidencePdfPath { get; set; } = "";
    public string EvidencePdfSha256 { get; set; } = "";

    public string DurationLabel
    {
        get
        {
            if (DurationSeconds <= 0) return "Nao registrado";
            var t = TimeSpan.FromSeconds(DurationSeconds);
            if (t.TotalHours >= 1) return $"{(int)t.TotalHours} h {t.Minutes:D2} min";
            return t.Seconds > 0 ? $"{t.Minutes} min {t.Seconds:D2} s" : $"{t.Minutes} min";
        }
    }
}


public sealed class TrainingExclusion
{
    public long TrainingId { get; set; }
    public string SamAccountName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTime ExcludedAtUtc { get; set; }
    public string ExcludedBy { get; set; } = "";
}

public sealed class AdUser
{
    public string SamAccountName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Department { get; set; } = "";
}

public sealed class DirectoryExclusion
{
    public string SamAccountName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Category { get; set; } = "E-mail geral / Caixa compartilhada";
    public string Reason { get; set; } = "";
    public DateTime ExcludedAtUtc { get; set; }
    public string ExcludedBy { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime? ReincludedAtUtc { get; set; }
    public string ReincludedBy { get; set; } = "";
}
