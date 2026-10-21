using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public static class TeamsReminderPolicy
{
    public static bool IsDue(TeamsReminderHistory? lastSuccessfulReminder, int intervalDays, DateTime nowUtc)
    {
        if (lastSuccessfulReminder is null) return true;
        var normalizedDays = Math.Clamp(intervalDays, 1, 365);
        return lastSuccessfulReminder.AttemptedAtUtc.ToUniversalTime() <= nowUtc.ToUniversalTime().AddDays(-normalizedDays);
    }
}
