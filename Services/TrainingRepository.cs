using System.Text.Json;
using Automind.Treinamentos.Data;
using Automind.Treinamentos.Models;
using Microsoft.Data.Sqlite;

namespace Automind.Treinamentos.Services;

public sealed class TrainingRepository
{
    private readonly TrainingDb _db;

    public TrainingRepository(TrainingDb db)
    {
        _db = db;
    }

    public async Task<List<TrainingListItemViewModel>> GetPublishedTrainingItemsAsync(string sam)
    {
        using var c = _db.OpenConnection();
        var trainings = new List<Training>();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT * FROM Trainings WHERE IsPublished=1 AND IsArchived=0 ORDER BY Title;";
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) trainings.Add(MapTraining(r));
        }

        var completions = new Dictionary<long, TrainingCompletion>();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT * FROM TrainingCompletions WHERE lower(SamAccountName)=lower($sam);";
            cmd.Parameters.AddWithValue("$sam", sam);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                var completion = MapCompletion(r);
                completions[completion.TrainingId] = completion;
            }
        }

        return trainings.Select(t => new TrainingListItemViewModel
        {
            Training = t,
            Completion = completions.GetValueOrDefault(t.Id)
        }).ToList();
    }

    public async Task<int> GetTotalCompletionCountAsync()
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TrainingCompletions;";
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<Training>> GetAllTrainingsAsync()
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Trainings ORDER BY CreatedAtUtc DESC;";
        using var r = await cmd.ExecuteReaderAsync();
        var list = new List<Training>();
        while (await r.ReadAsync()) list.Add(MapTraining(r));
        return list;
    }

    public async Task<Training?> GetTrainingAsync(long id)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Trainings WHERE Id=$id LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", id);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? MapTraining(r) : null;
    }

    public async Task<List<TrainingQuestion>> GetQuestionsAsync(long trainingId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM TrainingQuestions WHERE TrainingId=$id ORDER BY Position, Id;";
        cmd.Parameters.AddWithValue("$id", trainingId);
        using var r = await cmd.ExecuteReaderAsync();
        var list = new List<TrainingQuestion>();
        while (await r.ReadAsync())
        {
            list.Add(new TrainingQuestion
            {
                Id = r.GetInt64(r.GetOrdinal("Id")),
                TrainingId = r.GetInt64(r.GetOrdinal("TrainingId")),
                Position = r.GetInt32(r.GetOrdinal("Position")),
                Text = r.GetString(r.GetOrdinal("Text")),
                Options = JsonSerializer.Deserialize<List<string>>(r.GetString(r.GetOrdinal("OptionsJson"))) ?? new(),
                CorrectIndex = r.GetInt32(r.GetOrdinal("CorrectIndex"))
            });
        }
        return list;
    }

    public async Task<TrainingCompletion?> GetCompletionAsync(long trainingId, string sam)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM TrainingCompletions WHERE TrainingId=$trainingId AND lower(SamAccountName)=lower($sam) LIMIT 1;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        cmd.Parameters.AddWithValue("$sam", sam);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? MapCompletion(r) : null;
    }

    public async Task<TrainingCompletion?> GetCompletionByIdAsync(long id, string sam)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM TrainingCompletions WHERE Id=$id AND lower(SamAccountName)=lower($sam) LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$sam", sam);
        using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? MapCompletion(r) : null;
    }

    public async Task<List<TrainingCompletion>> GetCompletionsAsync(long trainingId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM TrainingCompletions WHERE TrainingId=$trainingId ORDER BY DisplayName;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        using var r = await cmd.ExecuteReaderAsync();
        var list = new List<TrainingCompletion>();
        while (await r.ReadAsync()) list.Add(MapCompletion(r));
        return list;
    }

    public async Task<int> GetCompletionCountAsync(long trainingId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM TrainingCompletions WHERE TrainingId=$trainingId;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> TrainingVersionExistsAsync(string code, string version, long? ignoreId = null)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = ignoreId.HasValue
            ? "SELECT 1 FROM Trainings WHERE lower(Code)=lower($code) AND lower(Version)=lower($version) AND Id<>$id LIMIT 1;"
            : "SELECT 1 FROM Trainings WHERE lower(Code)=lower($code) AND lower(Version)=lower($version) LIMIT 1;";
        cmd.Parameters.AddWithValue("$code", (code ?? string.Empty).Trim());
        cmd.Parameters.AddWithValue("$version", (version ?? string.Empty).Trim());
        if (ignoreId.HasValue) cmd.Parameters.AddWithValue("$id", ignoreId.Value);
        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task UpdateTrainingAsync(AdminTrainingEditViewModel m)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
UPDATE Trainings
SET Title=$title,
    Description=$description,
    SummaryText=$summary,
    Version=$version,
    ContentText=$content,
    PassingScore=$passing,
    EstimatedMinutes=$estimated,
    RequiredForAll=$required
WHERE Id=$id AND IsArchived=0;";
        cmd.Parameters.AddWithValue("$title", m.Title.Trim());
        cmd.Parameters.AddWithValue("$description", m.Description?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$summary", m.SummaryText?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$version", m.Version.Trim());
        cmd.Parameters.AddWithValue("$content", m.ContentText?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$passing", m.PassingScore);
        cmd.Parameters.AddWithValue("$estimated", m.EstimatedMinutes);
        cmd.Parameters.AddWithValue("$required", m.RequiredForAll ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", m.Id);
        var changed = await cmd.ExecuteNonQueryAsync();
        if (changed == 0) throw new InvalidOperationException("Treinamento nao encontrado ou versao historica imutavel.");
    }

    public async Task<long> CreateTrainingRevisionAsync(Training source, AdminTrainingEditViewModel m, string actor)
    {
        if (string.Equals(source.Version, m.Version?.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Informe uma nova versao para preservar as evidencias existentes.");

        if (await TrainingVersionExistsAsync(source.Code, m.Version))
            throw new InvalidOperationException($"A versao {m.Version} ja existe para o codigo {source.Code}.");

        var familySlug = string.IsNullOrWhiteSpace(source.FamilySlug) ? source.Slug : source.FamilySlug;
        var versionSlug = new string((m.Version ?? string.Empty).Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray()).Trim('-');
        if (string.IsNullOrWhiteSpace(versionSlug)) throw new InvalidOperationException("Versao invalida.");
        var technicalSlug = $"{familySlug}-v{versionSlug}";

        using var c = _db.OpenConnection();
        using var tx = c.BeginTransaction();
        long newId;
        using (var insert = c.CreateCommand())
        {
            insert.Transaction = tx;
            insert.CommandText = @"
INSERT INTO Trainings(Code, Slug, FamilySlug, Title, Description, SummaryText, Version, ContentText, PassingScore, EstimatedMinutes, IsPublished, IsArchived, RequiredForAll, LayoutKey, CreatedAtUtc, CreatedBy)
VALUES($code,$slug,$familySlug,$title,$description,$summary,$version,$content,$passing,$estimated,0,0,$required,$layout,$created,$actor);
SELECT last_insert_rowid();";
            insert.Parameters.AddWithValue("$code", source.Code.Trim());
            insert.Parameters.AddWithValue("$slug", technicalSlug);
            insert.Parameters.AddWithValue("$familySlug", familySlug.Trim().ToLowerInvariant());
            insert.Parameters.AddWithValue("$title", m.Title.Trim());
            insert.Parameters.AddWithValue("$description", m.Description?.Trim() ?? "");
            insert.Parameters.AddWithValue("$summary", m.SummaryText?.Trim() ?? "");
            insert.Parameters.AddWithValue("$version", m.Version.Trim());
            insert.Parameters.AddWithValue("$content", m.ContentText?.Trim() ?? "");
            insert.Parameters.AddWithValue("$passing", m.PassingScore);
            insert.Parameters.AddWithValue("$estimated", m.EstimatedMinutes);
            insert.Parameters.AddWithValue("$required", m.RequiredForAll ? 1 : 0);
            insert.Parameters.AddWithValue("$layout", source.LayoutKey);
            insert.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
            insert.Parameters.AddWithValue("$actor", actor);
            newId = Convert.ToInt64(await insert.ExecuteScalarAsync());
        }

        using (var copyQuestions = c.CreateCommand())
        {
            copyQuestions.Transaction = tx;
            copyQuestions.CommandText = @"
INSERT INTO TrainingQuestions(TrainingId, Position, Text, OptionsJson, CorrectIndex)
SELECT $newId, Position, Text, OptionsJson, CorrectIndex
FROM TrainingQuestions
WHERE TrainingId=$sourceId
ORDER BY Position, Id;";
            copyQuestions.Parameters.AddWithValue("$newId", newId);
            copyQuestions.Parameters.AddWithValue("$sourceId", source.Id);
            await copyQuestions.ExecuteNonQueryAsync();
        }

        tx.Commit();
        return newId;
    }

    public async Task<List<DirectoryExclusion>> GetDirectoryExclusionsAsync(bool activeOnly = false)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = activeOnly
            ? "SELECT * FROM DirectoryExclusions WHERE IsActive=1 ORDER BY DisplayName, Email, SamAccountName;"
            : "SELECT * FROM DirectoryExclusions ORDER BY IsActive DESC, DisplayName, Email, SamAccountName;";
        using var r = await cmd.ExecuteReaderAsync();
        var list = new List<DirectoryExclusion>();
        while (await r.ReadAsync()) list.Add(MapDirectoryExclusion(r));
        return list;
    }

    public async Task<bool> IsDirectoryExcludedAsync(string sam, string? email = null)
    {
        sam = (sam ?? string.Empty).Trim();
        email = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(sam) && string.IsNullOrWhiteSpace(email)) return false;

        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
SELECT 1
FROM DirectoryExclusions
WHERE IsActive=1
  AND (
        ($sam<>'' AND lower(SamAccountName)=lower($sam))
        OR ($email<>'' AND lower(Email)=lower($email))
      )
LIMIT 1;";
        cmd.Parameters.AddWithValue("$sam", sam);
        cmd.Parameters.AddWithValue("$email", email);
        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task UpsertDirectoryExclusionAsync(AdUser user, string category, string reason, string actor)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
INSERT INTO DirectoryExclusions(
    SamAccountName,DisplayName,Email,Category,Reason,ExcludedAtUtc,ExcludedBy,IsActive,ReincludedAtUtc,ReincludedBy)
VALUES($sam,$display,$email,$category,$reason,$when,$actor,1,NULL,NULL)
ON CONFLICT(SamAccountName) DO UPDATE SET
    DisplayName=excluded.DisplayName,
    Email=excluded.Email,
    Category=excluded.Category,
    Reason=excluded.Reason,
    ExcludedAtUtc=excluded.ExcludedAtUtc,
    ExcludedBy=excluded.ExcludedBy,
    IsActive=1,
    ReincludedAtUtc=NULL,
    ReincludedBy=NULL;";
        cmd.Parameters.AddWithValue("$sam", user.SamAccountName.Trim().ToLowerInvariant());
        cmd.Parameters.AddWithValue("$display", user.DisplayName?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$email", user.Email?.Trim().ToLowerInvariant() ?? "");
        cmd.Parameters.AddWithValue("$category", category.Trim());
        cmd.Parameters.AddWithValue("$reason", reason.Trim());
        cmd.Parameters.AddWithValue("$when", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("$actor", actor);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ReincludeDirectoryUserAsync(string sam, string actor)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
UPDATE DirectoryExclusions
SET IsActive=0,
    ReincludedAtUtc=$when,
    ReincludedBy=$actor
WHERE lower(SamAccountName)=lower($sam) AND IsActive=1;";
        cmd.Parameters.AddWithValue("$sam", (sam ?? string.Empty).Trim());
        cmd.Parameters.AddWithValue("$when", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("$actor", actor);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<long> CreateTrainingAsync(AdminTrainingCreateViewModel m, string actor)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Trainings(Code, Slug, FamilySlug, Title, Description, SummaryText, Version, ContentText, PassingScore, EstimatedMinutes, IsPublished, IsArchived, RequiredForAll, LayoutKey, CreatedAtUtc, CreatedBy)
VALUES($code,$slug,$slug,$title,$description,$summary,$version,$content,$passing,$estimated,0,0,$required,'generic',$created,$actor);
SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$code", m.Code.Trim());
        cmd.Parameters.AddWithValue("$slug", m.Slug.Trim().ToLowerInvariant());
        cmd.Parameters.AddWithValue("$title", m.Title.Trim());
        cmd.Parameters.AddWithValue("$description", m.Description?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$summary", m.SummaryText?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$version", m.Version.Trim());
        cmd.Parameters.AddWithValue("$content", m.ContentText?.Trim() ?? "");
        cmd.Parameters.AddWithValue("$passing", m.PassingScore);
        cmd.Parameters.AddWithValue("$estimated", m.EstimatedMinutes);
        cmd.Parameters.AddWithValue("$required", m.RequiredForAll ? 1 : 0);
        cmd.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("$actor", actor);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    public async Task<TrainingQuestion?> GetQuestionAsync(long trainingId, long questionId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM TrainingQuestions WHERE Id=$qid AND TrainingId=$tid LIMIT 1;";
        cmd.Parameters.AddWithValue("$qid", questionId);
        cmd.Parameters.AddWithValue("$tid", trainingId);
        using var r = await cmd.ExecuteReaderAsync();
        if (!await r.ReadAsync()) return null;
        return new TrainingQuestion
        {
            Id = r.GetInt64(r.GetOrdinal("Id")),
            TrainingId = r.GetInt64(r.GetOrdinal("TrainingId")),
            Position = r.GetInt32(r.GetOrdinal("Position")),
            Text = r.GetString(r.GetOrdinal("Text")),
            Options = JsonSerializer.Deserialize<List<string>>(r.GetString(r.GetOrdinal("OptionsJson"))) ?? new(),
            CorrectIndex = r.GetInt32(r.GetOrdinal("CorrectIndex"))
        };
    }

    public async Task UpdateQuestionAsync(AdminQuestionEditViewModel m)
    {
        var options = new List<string> { m.OptionA.Trim(), m.OptionB.Trim(), m.OptionC.Trim() };
        if (!string.IsNullOrWhiteSpace(m.OptionD)) options.Add(m.OptionD.Trim());
        if (m.CorrectIndex >= options.Count) throw new InvalidOperationException("Resposta correta fora das opcoes informadas.");

        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"UPDATE TrainingQuestions
SET Text=$text, OptionsJson=$options, CorrectIndex=$correct
WHERE Id=$qid AND TrainingId=$tid;";
        cmd.Parameters.AddWithValue("$text", m.Text.Trim());
        cmd.Parameters.AddWithValue("$options", JsonSerializer.Serialize(options));
        cmd.Parameters.AddWithValue("$correct", m.CorrectIndex);
        cmd.Parameters.AddWithValue("$qid", m.QuestionId);
        cmd.Parameters.AddWithValue("$tid", m.TrainingId);
        var changed = await cmd.ExecuteNonQueryAsync();
        if (changed == 0) throw new InvalidOperationException("Questao nao encontrada.");
    }

    public async Task AddQuestionAsync(AdminQuestionCreateViewModel m)
    {
        var options = new List<string> { m.OptionA.Trim(), m.OptionB.Trim(), m.OptionC.Trim() };
        if (!string.IsNullOrWhiteSpace(m.OptionD)) options.Add(m.OptionD.Trim());
        if (m.CorrectIndex >= options.Count) throw new InvalidOperationException("Resposta correta fora das opcoes informadas.");

        using var c = _db.OpenConnection();
        int nextPosition;
        using (var pos = c.CreateCommand())
        {
            pos.CommandText = "SELECT COALESCE(MAX(Position),0)+1 FROM TrainingQuestions WHERE TrainingId=$id;";
            pos.Parameters.AddWithValue("$id", m.TrainingId);
            nextPosition = Convert.ToInt32(await pos.ExecuteScalarAsync());
        }
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"INSERT INTO TrainingQuestions(TrainingId, Position, Text, OptionsJson, CorrectIndex)
VALUES($trainingId,$position,$text,$options,$correct);";
        cmd.Parameters.AddWithValue("$trainingId", m.TrainingId);
        cmd.Parameters.AddWithValue("$position", nextPosition);
        cmd.Parameters.AddWithValue("$text", m.Text.Trim());
        cmd.Parameters.AddWithValue("$options", JsonSerializer.Serialize(options));
        cmd.Parameters.AddWithValue("$correct", m.CorrectIndex);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteQuestionAsync(long trainingId, long questionId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "DELETE FROM TrainingQuestions WHERE Id=$qid AND TrainingId=$tid;";
        cmd.Parameters.AddWithValue("$qid", questionId);
        cmd.Parameters.AddWithValue("$tid", trainingId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SetPublishedAsync(long trainingId, bool published)
    {
        using var c = _db.OpenConnection();
        using var tx = c.BeginTransaction();

        string? code = null;
        bool archived = false;
        using (var lookup = c.CreateCommand())
        {
            lookup.Transaction = tx;
            lookup.CommandText = "SELECT Code, IsArchived FROM Trainings WHERE Id=$id LIMIT 1;";
            lookup.Parameters.AddWithValue("$id", trainingId);
            using var r = await lookup.ExecuteReaderAsync();
            if (!await r.ReadAsync()) throw new InvalidOperationException("Treinamento nao encontrado.");
            code = r.GetString(0);
            archived = r.GetInt32(1) == 1;
        }

        if (published && archived)
            throw new InvalidOperationException("Uma versao historica nao pode ser publicada novamente. Crie uma nova revisao.");

        if (published)
        {
            using var archivePrevious = c.CreateCommand();
            archivePrevious.Transaction = tx;
            archivePrevious.CommandText = @"
UPDATE Trainings
SET IsPublished=0, IsArchived=1
WHERE Id<>$id AND lower(Code)=lower($code) AND IsPublished=1;";
            archivePrevious.Parameters.AddWithValue("$id", trainingId);
            archivePrevious.Parameters.AddWithValue("$code", code);
            await archivePrevious.ExecuteNonQueryAsync();
        }

        using (var cmd = c.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = "UPDATE Trainings SET IsPublished=$p, IsArchived=CASE WHEN $p=1 THEN 0 ELSE IsArchived END WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$p", published ? 1 : 0);
            cmd.Parameters.AddWithValue("$id", trainingId);
            await cmd.ExecuteNonQueryAsync();
        }

        tx.Commit();
    }

    public async Task InsertCompletionAsync(TrainingCompletion x)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
INSERT INTO TrainingCompletions(
    TrainingId,SamAccountName,DisplayName,Email,JobTitle,Department,Score,Total,StartedAtUtc,AcceptedAtUtc,DurationSeconds,Protocol,TrainingSnapshotSha256,EvidencePdfPath,EvidencePdfSha256)
VALUES($trainingId,$sam,$display,$email,$title,$department,$score,$total,$started,$accepted,$duration,$protocol,$trainingHash,$pdfPath,$pdfHash);";
        cmd.Parameters.AddWithValue("$trainingId", x.TrainingId);
        cmd.Parameters.AddWithValue("$sam", x.SamAccountName);
        cmd.Parameters.AddWithValue("$display", x.DisplayName);
        cmd.Parameters.AddWithValue("$email", x.Email);
        cmd.Parameters.AddWithValue("$title", x.JobTitle);
        cmd.Parameters.AddWithValue("$department", x.Department);
        cmd.Parameters.AddWithValue("$score", x.Score);
        cmd.Parameters.AddWithValue("$total", x.Total);
        cmd.Parameters.AddWithValue("$started", x.StartedAtUtc.ToString("O"));
        cmd.Parameters.AddWithValue("$accepted", x.AcceptedAtUtc.ToString("O"));
        cmd.Parameters.AddWithValue("$duration", x.DurationSeconds);
        cmd.Parameters.AddWithValue("$protocol", x.Protocol);
        cmd.Parameters.AddWithValue("$trainingHash", x.TrainingSnapshotSha256);
        cmd.Parameters.AddWithValue("$pdfPath", x.EvidencePdfPath);
        cmd.Parameters.AddWithValue("$pdfHash", x.EvidencePdfSha256);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<TrainingReminderSchedule> GetTrainingReminderScheduleAsync(long trainingId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM TrainingReminderSchedules WHERE TrainingId=$trainingId LIMIT 1;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        using var r = await cmd.ExecuteReaderAsync();
        if (await r.ReadAsync()) return MapTrainingReminderSchedule(r);

        return new TrainingReminderSchedule
        {
            TrainingId = trainingId,
            Enabled = false,
            IntervalDays = 15,
            UpdatedAtUtc = DateTime.MinValue
        };
    }

    public async Task SaveTrainingReminderScheduleAsync(long trainingId, bool enabled, int intervalDays, DateTime? nextRunAtUtc, string updatedBy)
    {
        intervalDays = Math.Clamp(intervalDays, 1, 365);
        var now = DateTime.UtcNow;
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
INSERT INTO TrainingReminderSchedules(TrainingId, Enabled, IntervalDays, NextRunAtUtc, LastRunAtUtc, UpdatedBy, UpdatedAtUtc)
VALUES($trainingId, $enabled, $intervalDays, $nextRunAtUtc, NULL, $updatedBy, $updatedAtUtc)
ON CONFLICT(TrainingId) DO UPDATE SET
    Enabled=excluded.Enabled,
    IntervalDays=excluded.IntervalDays,
    NextRunAtUtc=excluded.NextRunAtUtc,
    UpdatedBy=excluded.UpdatedBy,
    UpdatedAtUtc=excluded.UpdatedAtUtc;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        cmd.Parameters.AddWithValue("$enabled", enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("$intervalDays", intervalDays);
        cmd.Parameters.AddWithValue("$nextRunAtUtc", nextRunAtUtc is null ? DBNull.Value : nextRunAtUtc.Value.ToUniversalTime().ToString("O"));
        cmd.Parameters.AddWithValue("$updatedBy", updatedBy ?? string.Empty);
        cmd.Parameters.AddWithValue("$updatedAtUtc", now.ToString("O"));
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<TrainingReminderSchedule>> GetDueTrainingReminderSchedulesAsync(DateTime nowUtc)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
SELECT * FROM TrainingReminderSchedules
WHERE Enabled=1 AND NextRunAtUtc IS NOT NULL AND NextRunAtUtc <= $now
ORDER BY NextRunAtUtc, TrainingId;";
        cmd.Parameters.AddWithValue("$now", nowUtc.ToUniversalTime().ToString("O"));
        using var r = await cmd.ExecuteReaderAsync();
        var result = new List<TrainingReminderSchedule>();
        while (await r.ReadAsync()) result.Add(MapTrainingReminderSchedule(r));
        return result;
    }

    public async Task<bool> AdvanceTrainingReminderScheduleAsync(TrainingReminderSchedule schedule, DateTime finishedAtUtc)
    {
        if (schedule.NextRunAtUtc is null) return false;

        var intervalDays = Math.Clamp(schedule.IntervalDays, 1, 365);
        var next = schedule.NextRunAtUtc.Value.ToUniversalTime();
        var finished = finishedAtUtc.ToUniversalTime();
        do { next = next.AddDays(intervalDays); } while (next <= finished);

        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
UPDATE TrainingReminderSchedules
SET LastRunAtUtc=$lastRun, NextRunAtUtc=$nextRun
WHERE TrainingId=$trainingId AND Enabled=1 AND NextRunAtUtc=$expectedNextRun;";
        cmd.Parameters.AddWithValue("$lastRun", finished.ToString("O"));
        cmd.Parameters.AddWithValue("$nextRun", next.ToString("O"));
        cmd.Parameters.AddWithValue("$trainingId", schedule.TrainingId);
        cmd.Parameters.AddWithValue("$expectedNextRun", schedule.NextRunAtUtc.Value.ToUniversalTime().ToString("O"));
        return await cmd.ExecuteNonQueryAsync() == 1;
    }

    public async Task<Dictionary<string, TeamsReminderHistory>> GetLatestTeamsReminderHistoryAsync(long trainingId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
SELECT * FROM TeamsReminderHistory
WHERE TrainingId=$trainingId
ORDER BY AttemptedAtUtc DESC, Id DESC;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        using var r = await cmd.ExecuteReaderAsync();
        var result = new Dictionary<string, TeamsReminderHistory>(StringComparer.OrdinalIgnoreCase);
        while (await r.ReadAsync())
        {
            var item = MapTeamsReminderHistory(r);
            if (!result.ContainsKey(item.SamAccountName)) result[item.SamAccountName] = item;
        }
        return result;
    }

    public async Task<Dictionary<string, TeamsReminderHistory>> GetLatestSuccessfulTeamsReminderHistoryAsync(long trainingId)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
SELECT * FROM TeamsReminderHistory
WHERE TrainingId=$trainingId AND Status=$sent
ORDER BY AttemptedAtUtc DESC, Id DESC;";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        cmd.Parameters.AddWithValue("$sent", TeamsReminderStatus.Sent);
        using var r = await cmd.ExecuteReaderAsync();
        var result = new Dictionary<string, TeamsReminderHistory>(StringComparer.OrdinalIgnoreCase);
        while (await r.ReadAsync())
        {
            var item = MapTeamsReminderHistory(r);
            if (!result.ContainsKey(item.SamAccountName)) result[item.SamAccountName] = item;
        }
        return result;
    }

    public async Task<long?> BeginTeamsReminderAttemptAsync(
        long trainingId,
        AdUser user,
        string source,
        string actor,
        string? cycleKey,
        string requestId,
        string callbackToken,
        DateTime attemptedAtUtc)
    {
        using var c = _db.OpenConnection();
        using var tx = c.BeginTransaction();
        using var cmd = c.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
INSERT OR IGNORE INTO TeamsReminderHistory(
    TrainingId, SamAccountName, DisplayName, Recipient, AttemptedAtUtc, SubmittedAtUtc, FinishedAtUtc,
    Source, Status, HttpStatusCode, WorkflowRunId, MessageId, Error, Actor, CycleKey, ImportKey, RequestId, CallbackToken)
VALUES(
    $trainingId, lower($sam), $displayName, $recipient, $attemptedAtUtc, NULL, NULL,
    $source, $status, NULL, '', '', '', $actor, $cycleKey, NULL, $requestId, $callbackToken);";
        cmd.Parameters.AddWithValue("$trainingId", trainingId);
        cmd.Parameters.AddWithValue("$sam", user.SamAccountName.Trim());
        cmd.Parameters.AddWithValue("$displayName", user.DisplayName ?? string.Empty);
        cmd.Parameters.AddWithValue("$recipient", user.Email ?? string.Empty);
        cmd.Parameters.AddWithValue("$attemptedAtUtc", attemptedAtUtc.ToUniversalTime().ToString("O"));
        cmd.Parameters.AddWithValue("$source", source);
        cmd.Parameters.AddWithValue("$status", TeamsReminderStatus.Queued);
        cmd.Parameters.AddWithValue("$actor", actor ?? string.Empty);
        cmd.Parameters.AddWithValue("$requestId", requestId);
        cmd.Parameters.AddWithValue("$callbackToken", callbackToken);
        cmd.Parameters.AddWithValue("$cycleKey", cycleKey is null ? DBNull.Value : cycleKey);
        var changed = await cmd.ExecuteNonQueryAsync();
        if (changed == 0)
        {
            tx.Rollback();
            return null;
        }

        using var id = c.CreateCommand();
        id.Transaction = tx;
        id.CommandText = "SELECT last_insert_rowid();";
        var insertedId = Convert.ToInt64(await id.ExecuteScalarAsync());
        tx.Commit();
        return insertedId;
    }

    public async Task<TeamsReminderHistory?> ClaimNextQueuedTeamsReminderAsync(DateTime submittedAtUtc)
    {
        using var c = _db.OpenConnection();
        using var tx = c.BeginTransaction();
        long? id = null;

        using (var select = c.CreateCommand())
        {
            select.Transaction = tx;
            select.CommandText = @"
SELECT Id FROM TeamsReminderHistory
WHERE Status=$queued
ORDER BY AttemptedAtUtc, Id
LIMIT 1;";
            select.Parameters.AddWithValue("$queued", TeamsReminderStatus.Queued);
            var value = await select.ExecuteScalarAsync();
            if (value is not null && value != DBNull.Value) id = Convert.ToInt64(value);
        }

        if (id is null)
        {
            tx.Rollback();
            return null;
        }

        using (var claim = c.CreateCommand())
        {
            claim.Transaction = tx;
            claim.CommandText = @"
UPDATE TeamsReminderHistory
SET Status=$processing, SubmittedAtUtc=$submittedAtUtc, Error=''
WHERE Id=$id AND Status=$queued;";
            claim.Parameters.AddWithValue("$processing", TeamsReminderStatus.Processing);
            claim.Parameters.AddWithValue("$submittedAtUtc", submittedAtUtc.ToUniversalTime().ToString("O"));
            claim.Parameters.AddWithValue("$id", id.Value);
            claim.Parameters.AddWithValue("$queued", TeamsReminderStatus.Queued);
            if (await claim.ExecuteNonQueryAsync() != 1)
            {
                tx.Rollback();
                return null;
            }
        }

        TeamsReminderHistory? item = null;
        using (var read = c.CreateCommand())
        {
            read.Transaction = tx;
            read.CommandText = "SELECT * FROM TeamsReminderHistory WHERE Id=$id;";
            read.Parameters.AddWithValue("$id", id.Value);
            using var reader = await read.ExecuteReaderAsync();
            if (await reader.ReadAsync()) item = MapTeamsReminderHistory(reader);
        }

        tx.Commit();
        return item;
    }

    public async Task MarkTeamsReminderSubmittedAsync(long historyId, int? httpStatusCode, DateTime submittedAtUtc)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
UPDATE TeamsReminderHistory
SET Status=$processing, SubmittedAtUtc=$submittedAtUtc, HttpStatusCode=$httpStatusCode, Error=''
WHERE Id=$id AND Status=$processing;";
        cmd.Parameters.AddWithValue("$processing", TeamsReminderStatus.Processing);
        cmd.Parameters.AddWithValue("$submittedAtUtc", submittedAtUtc.ToUniversalTime().ToString("O"));
        cmd.Parameters.AddWithValue("$httpStatusCode", httpStatusCode is null ? DBNull.Value : httpStatusCode.Value);
        cmd.Parameters.AddWithValue("$id", historyId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarkTeamsReminderSubmissionUncertainAsync(long historyId, string error, DateTime submittedAtUtc)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
UPDATE TeamsReminderHistory
SET Status=$processing, SubmittedAtUtc=$submittedAtUtc, Error=$error
WHERE Id=$id AND Status=$processing;";
        cmd.Parameters.AddWithValue("$processing", TeamsReminderStatus.Processing);
        cmd.Parameters.AddWithValue("$submittedAtUtc", submittedAtUtc.ToUniversalTime().ToString("O"));
        cmd.Parameters.AddWithValue("$error", error ?? string.Empty);
        cmd.Parameters.AddWithValue("$id", historyId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> CompleteTeamsReminderAttemptByCallbackAsync(
        string requestId,
        string callbackToken,
        TeamsSendResult result,
        DateTime finishedAtUtc)
    {
        using var c = _db.OpenConnection();
        using var tx = c.BeginTransaction();
        long? id = null;
        string? currentStatus = null;

        using (var select = c.CreateCommand())
        {
            select.Transaction = tx;
            select.CommandText = @"
SELECT Id, Status FROM TeamsReminderHistory
WHERE RequestId=$requestId AND CallbackToken=$callbackToken
LIMIT 1;";
            select.Parameters.AddWithValue("$requestId", requestId);
            select.Parameters.AddWithValue("$callbackToken", callbackToken);
            using var reader = await select.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                id = reader.GetInt64(0);
                currentStatus = reader.GetString(1);
            }
        }

        if (id is null)
        {
            tx.Rollback();
            return false;
        }

        if (string.Equals(currentStatus, TeamsReminderStatus.Sent, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentStatus, TeamsReminderStatus.Failed, StringComparison.OrdinalIgnoreCase))
        {
            tx.Commit();
            return true;
        }

        using (var update = c.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = @"
UPDATE TeamsReminderHistory
SET FinishedAtUtc=$finishedAtUtc,
    Status=$status,
    HttpStatusCode=coalesce($httpStatusCode, HttpStatusCode),
    WorkflowRunId=$workflowRunId,
    MessageId=$messageId,
    Error=$error
WHERE Id=$id;";
            update.Parameters.AddWithValue("$finishedAtUtc", finishedAtUtc.ToUniversalTime().ToString("O"));
            update.Parameters.AddWithValue("$status", result.Status);
            update.Parameters.AddWithValue("$httpStatusCode", result.HttpStatusCode is null ? DBNull.Value : result.HttpStatusCode.Value);
            update.Parameters.AddWithValue("$workflowRunId", result.WorkflowRunId ?? string.Empty);
            update.Parameters.AddWithValue("$messageId", result.MessageId ?? string.Empty);
            update.Parameters.AddWithValue("$error", result.Error ?? string.Empty);
            update.Parameters.AddWithValue("$id", id.Value);
            await update.ExecuteNonQueryAsync();
        }

        tx.Commit();
        return true;
    }

    public async Task CompleteTeamsReminderAttemptAsync(long historyId, TeamsSendResult result, DateTime finishedAtUtc)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
UPDATE TeamsReminderHistory
SET FinishedAtUtc=$finishedAtUtc,
    Status=$status,
    HttpStatusCode=$httpStatusCode,
    WorkflowRunId=$workflowRunId,
    MessageId=$messageId,
    Error=$error
WHERE Id=$id;";
        cmd.Parameters.AddWithValue("$finishedAtUtc", finishedAtUtc.ToUniversalTime().ToString("O"));
        cmd.Parameters.AddWithValue("$status", result.Status);
        cmd.Parameters.AddWithValue("$httpStatusCode", result.HttpStatusCode is null ? DBNull.Value : result.HttpStatusCode.Value);
        cmd.Parameters.AddWithValue("$workflowRunId", result.WorkflowRunId ?? string.Empty);
        cmd.Parameters.AddWithValue("$messageId", result.MessageId ?? string.Empty);
        cmd.Parameters.AddWithValue("$error", result.Error ?? string.Empty);
        cmd.Parameters.AddWithValue("$id", historyId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static DirectoryExclusion MapDirectoryExclusion(SqliteDataReader r)
    {
        DateTime? reincludedAt = null;
        var reincludedAtOrdinal = r.GetOrdinal("ReincludedAtUtc");
        if (!r.IsDBNull(reincludedAtOrdinal))
        {
            var value = r.GetString(reincludedAtOrdinal);
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
                reincludedAt = parsed;
        }

        return new DirectoryExclusion
        {
            SamAccountName = r.GetString(r.GetOrdinal("SamAccountName")),
            DisplayName = r.GetString(r.GetOrdinal("DisplayName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            Category = r.GetString(r.GetOrdinal("Category")),
            Reason = r.GetString(r.GetOrdinal("Reason")),
            ExcludedAtUtc = DateTime.Parse(r.GetString(r.GetOrdinal("ExcludedAtUtc")), null, System.Globalization.DateTimeStyles.RoundtripKind),
            ExcludedBy = r.GetString(r.GetOrdinal("ExcludedBy")),
            IsActive = r.GetInt32(r.GetOrdinal("IsActive")) == 1,
            ReincludedAtUtc = reincludedAt,
            ReincludedBy = r.IsDBNull(r.GetOrdinal("ReincludedBy")) ? "" : r.GetString(r.GetOrdinal("ReincludedBy"))
        };
    }

    private static Training MapTraining(SqliteDataReader r) => new()
    {
        Id = r.GetInt64(r.GetOrdinal("Id")),
        Code = r.GetString(r.GetOrdinal("Code")),
        Slug = r.GetString(r.GetOrdinal("Slug")),
        FamilySlug = r.GetString(r.GetOrdinal("FamilySlug")),
        Title = r.GetString(r.GetOrdinal("Title")),
        Description = r.GetString(r.GetOrdinal("Description")),
        SummaryText = r.GetString(r.GetOrdinal("SummaryText")),
        Version = r.GetString(r.GetOrdinal("Version")),
        ContentText = r.GetString(r.GetOrdinal("ContentText")),
        PassingScore = r.GetInt32(r.GetOrdinal("PassingScore")),
        EstimatedMinutes = r.GetInt32(r.GetOrdinal("EstimatedMinutes")),
        IsPublished = r.GetInt32(r.GetOrdinal("IsPublished")) == 1,
        IsArchived = r.GetInt32(r.GetOrdinal("IsArchived")) == 1,
        RequiredForAll = r.GetInt32(r.GetOrdinal("RequiredForAll")) == 1,
        LayoutKey = r.GetString(r.GetOrdinal("LayoutKey")),
        CreatedAtUtc = DateTime.Parse(r.GetString(r.GetOrdinal("CreatedAtUtc")), null, System.Globalization.DateTimeStyles.RoundtripKind),
        CreatedBy = r.GetString(r.GetOrdinal("CreatedBy"))
    };

    private static TeamsReminderHistory MapTeamsReminderHistory(SqliteDataReader r)
    {
        DateTime? finishedAtUtc = null;
        var finishedOrdinal = r.GetOrdinal("FinishedAtUtc");
        if (!r.IsDBNull(finishedOrdinal))
            finishedAtUtc = DateTime.Parse(r.GetString(finishedOrdinal), null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();

        DateTime? submittedAtUtc = null;
        var submittedOrdinal = r.GetOrdinal("SubmittedAtUtc");
        if (!r.IsDBNull(submittedOrdinal))
            submittedAtUtc = DateTime.Parse(r.GetString(submittedOrdinal), null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();

        int? httpStatusCode = null;
        var httpOrdinal = r.GetOrdinal("HttpStatusCode");
        if (!r.IsDBNull(httpOrdinal)) httpStatusCode = r.GetInt32(httpOrdinal);

        return new TeamsReminderHistory
        {
            Id = r.GetInt64(r.GetOrdinal("Id")),
            TrainingId = r.GetInt64(r.GetOrdinal("TrainingId")),
            SamAccountName = r.GetString(r.GetOrdinal("SamAccountName")),
            DisplayName = r.GetString(r.GetOrdinal("DisplayName")),
            Recipient = r.GetString(r.GetOrdinal("Recipient")),
            AttemptedAtUtc = DateTime.Parse(r.GetString(r.GetOrdinal("AttemptedAtUtc")), null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime(),
            SubmittedAtUtc = submittedAtUtc,
            FinishedAtUtc = finishedAtUtc,
            Source = r.GetString(r.GetOrdinal("Source")),
            Status = r.GetString(r.GetOrdinal("Status")),
            HttpStatusCode = httpStatusCode,
            WorkflowRunId = r.GetString(r.GetOrdinal("WorkflowRunId")),
            MessageId = r.GetString(r.GetOrdinal("MessageId")),
            Error = r.GetString(r.GetOrdinal("Error")),
            Actor = r.GetString(r.GetOrdinal("Actor")),
            CycleKey = r.IsDBNull(r.GetOrdinal("CycleKey")) ? null : r.GetString(r.GetOrdinal("CycleKey")),
            ImportKey = r.IsDBNull(r.GetOrdinal("ImportKey")) ? null : r.GetString(r.GetOrdinal("ImportKey")),
            RequestId = r.GetString(r.GetOrdinal("RequestId")),
            CallbackToken = r.GetString(r.GetOrdinal("CallbackToken"))
        };
    }

    private static TrainingReminderSchedule MapTrainingReminderSchedule(SqliteDataReader r)
    {
        static DateTime? ReadNullableDate(SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal)) return null;
            return DateTime.Parse(reader.GetString(ordinal), null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();
        }

        return new TrainingReminderSchedule
        {
            TrainingId = r.GetInt64(r.GetOrdinal("TrainingId")),
            Enabled = r.GetInt32(r.GetOrdinal("Enabled")) == 1,
            IntervalDays = r.GetInt32(r.GetOrdinal("IntervalDays")),
            NextRunAtUtc = ReadNullableDate(r, "NextRunAtUtc"),
            LastRunAtUtc = ReadNullableDate(r, "LastRunAtUtc"),
            UpdatedBy = r.GetString(r.GetOrdinal("UpdatedBy")),
            UpdatedAtUtc = DateTime.Parse(r.GetString(r.GetOrdinal("UpdatedAtUtc")), null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime()
        };
    }

    private static TrainingCompletion MapCompletion(SqliteDataReader r)
    {
        var accepted = DateTime.Parse(r.GetString(r.GetOrdinal("AcceptedAtUtc")), null, System.Globalization.DateTimeStyles.RoundtripKind);
        var startedText = r.GetString(r.GetOrdinal("StartedAtUtc"));
        var started = DateTime.TryParse(startedText, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedStarted)
            ? parsedStarted
            : accepted;
        return new TrainingCompletion
        {
            Id = r.GetInt64(r.GetOrdinal("Id")),
            TrainingId = r.GetInt64(r.GetOrdinal("TrainingId")),
            SamAccountName = r.GetString(r.GetOrdinal("SamAccountName")),
            DisplayName = r.GetString(r.GetOrdinal("DisplayName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            JobTitle = r.GetString(r.GetOrdinal("JobTitle")),
            Department = r.GetString(r.GetOrdinal("Department")),
            Score = r.GetInt32(r.GetOrdinal("Score")),
            Total = r.GetInt32(r.GetOrdinal("Total")),
            StartedAtUtc = started,
            AcceptedAtUtc = accepted,
            DurationSeconds = r.GetInt32(r.GetOrdinal("DurationSeconds")),
            Protocol = r.GetString(r.GetOrdinal("Protocol")),
            TrainingSnapshotSha256 = r.GetString(r.GetOrdinal("TrainingSnapshotSha256")),
            EvidencePdfPath = r.GetString(r.GetOrdinal("EvidencePdfPath")),
            EvidencePdfSha256 = r.GetString(r.GetOrdinal("EvidencePdfSha256"))
        };
    }
}
