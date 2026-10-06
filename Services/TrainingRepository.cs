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

    public async Task<List<Training>> GetPublishedTrainingsAsync()
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Trainings WHERE IsPublished=1 ORDER BY Title;";
        using var r = await cmd.ExecuteReaderAsync();
        var list = new List<Training>();
        while (await r.ReadAsync()) list.Add(MapTraining(r));
        return list;
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

    public async Task<long> CreateTrainingAsync(AdminTrainingCreateViewModel m, string actor)
    {
        using var c = _db.OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Trainings(Code, Slug, Title, Description, Version, ContentText, PassingScore, EstimatedMinutes, IsPublished, RequiredForAll, LayoutKey, CreatedAtUtc, CreatedBy)
VALUES($code,$slug,$title,$description,$version,$content,$passing,$estimated,0,$required,'generic',$created,$actor);
SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$code", m.Code.Trim());
        cmd.Parameters.AddWithValue("$slug", m.Slug.Trim().ToLowerInvariant());
        cmd.Parameters.AddWithValue("$title", m.Title.Trim());
        cmd.Parameters.AddWithValue("$description", m.Description?.Trim() ?? "");
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
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Trainings SET IsPublished=$p WHERE Id=$id;";
        cmd.Parameters.AddWithValue("$p", published ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", trainingId);
        await cmd.ExecuteNonQueryAsync();
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

    private static Training MapTraining(SqliteDataReader r) => new()
    {
        Id = r.GetInt64(r.GetOrdinal("Id")),
        Code = r.GetString(r.GetOrdinal("Code")),
        Slug = r.GetString(r.GetOrdinal("Slug")),
        Title = r.GetString(r.GetOrdinal("Title")),
        Description = r.GetString(r.GetOrdinal("Description")),
        Version = r.GetString(r.GetOrdinal("Version")),
        ContentText = r.GetString(r.GetOrdinal("ContentText")),
        PassingScore = r.GetInt32(r.GetOrdinal("PassingScore")),
        EstimatedMinutes = r.GetInt32(r.GetOrdinal("EstimatedMinutes")),
        IsPublished = r.GetInt32(r.GetOrdinal("IsPublished")) == 1,
        RequiredForAll = r.GetInt32(r.GetOrdinal("RequiredForAll")) == 1,
        LayoutKey = r.GetString(r.GetOrdinal("LayoutKey")),
        CreatedAtUtc = DateTime.Parse(r.GetString(r.GetOrdinal("CreatedAtUtc")), null, System.Globalization.DateTimeStyles.RoundtripKind),
        CreatedBy = r.GetString(r.GetOrdinal("CreatedBy"))
    };

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
