namespace Automind.Treinamentos.Models;

public static class TeamsReminderStatus
{
    public const string Queued = "queued";
    public const string Processing = "processing";
    public const string Sent = "sent";
    public const string Failed = "failed";

    public static bool CountsAsNotified(string? status) =>
        string.Equals(status, Sent, StringComparison.OrdinalIgnoreCase);
}

public static class TeamsReminderSource
{
    public const string Manual = "manual";
    public const string Automatic = "automatic";
}

public sealed class TeamsReminderHistory
{
    public long Id { get; set; }
    public long TrainingId { get; set; }
    public string SamAccountName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Recipient { get; set; } = "";
    public DateTime AttemptedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public string Source { get; set; } = TeamsReminderSource.Manual;
    public string Status { get; set; } = TeamsReminderStatus.Queued;
    public int? HttpStatusCode { get; set; }
    public string WorkflowRunId { get; set; } = "";
    public string MessageId { get; set; } = "";
    public string Error { get; set; } = "";
    public string Actor { get; set; } = "";
    public string? CycleKey { get; set; }
    public string? ImportKey { get; set; }
    public string RequestId { get; set; } = "";
    public string CallbackToken { get; set; } = "";
}

public sealed class TeamsReminderCallbackRequest
{
    public string RequestId { get; set; } = "";
    public string CallbackToken { get; set; } = "";
    public bool Success { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public string WorkflowRunId { get; set; } = "";
    public string MessageId { get; set; } = "";
    public int? HttpStatusCode { get; set; }
    public string Error { get; set; } = "";
}

public sealed class TrainingReminderSchedule
{
    public long TrainingId { get; set; }
    public bool Enabled { get; set; }
    public int IntervalDays { get; set; } = 15;
    public DateTime? NextRunAtUtc { get; set; }
    public DateTime? LastRunAtUtc { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime UpdatedAtUtc { get; set; }
}
