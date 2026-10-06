using System.Text.Json;
using Automind.Treinamentos.Services;
using Microsoft.Data.Sqlite;

namespace Automind.Treinamentos.Data;

public sealed class DatabaseInitializer
{
    private readonly TrainingDb _db;
    private readonly StorageService _storage;
    private readonly TrainingSnapshotService _snapshot;

    public DatabaseInitializer(TrainingDb db, StorageService storage, TrainingSnapshotService snapshot)
    {
        _db = db;
        _storage = storage;
        _snapshot = snapshot;
    }

    public async Task InitializeAsync()
    {
        _storage.EnsureDirectories();
        using var connection = _db.OpenConnection();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS Trainings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL,
    Slug TEXT NOT NULL UNIQUE,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    SummaryText TEXT NOT NULL DEFAULT '',
    Version TEXT NOT NULL,
    ContentText TEXT NOT NULL DEFAULT '',
    PassingScore INTEGER NOT NULL,
    EstimatedMinutes INTEGER NOT NULL DEFAULT 8,
    IsPublished INTEGER NOT NULL DEFAULT 0,
    RequiredForAll INTEGER NOT NULL DEFAULT 1,
    LayoutKey TEXT NOT NULL DEFAULT 'generic',
    CreatedAtUtc TEXT NOT NULL,
    CreatedBy TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS TrainingQuestions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TrainingId INTEGER NOT NULL,
    Position INTEGER NOT NULL,
    Text TEXT NOT NULL,
    OptionsJson TEXT NOT NULL,
    CorrectIndex INTEGER NOT NULL,
    FOREIGN KEY (TrainingId) REFERENCES Trainings(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_TrainingQuestions_Training_Position ON TrainingQuestions(TrainingId, Position);
CREATE TABLE IF NOT EXISTS TrainingCompletions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TrainingId INTEGER NOT NULL,
    SamAccountName TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    Email TEXT NOT NULL,
    JobTitle TEXT NOT NULL DEFAULT '',
    Department TEXT NOT NULL DEFAULT '',
    Score INTEGER NOT NULL,
    Total INTEGER NOT NULL,
    StartedAtUtc TEXT NOT NULL DEFAULT '',
    AcceptedAtUtc TEXT NOT NULL,
    DurationSeconds INTEGER NOT NULL DEFAULT 0,
    Protocol TEXT NOT NULL UNIQUE,
    TrainingSnapshotSha256 TEXT NOT NULL,
    EvidencePdfPath TEXT NOT NULL,
    EvidencePdfSha256 TEXT NOT NULL,
    FOREIGN KEY (TrainingId) REFERENCES Trainings(Id) ON DELETE RESTRICT,
    UNIQUE(TrainingId, SamAccountName)
);
CREATE INDEX IF NOT EXISTS IX_TrainingCompletions_Training ON TrainingCompletions(TrainingId);
CREATE INDEX IF NOT EXISTS IX_TrainingCompletions_Sam ON TrainingCompletions(SamAccountName);
CREATE TABLE IF NOT EXISTS TrainingExclusions (
    TrainingId INTEGER NOT NULL,
    SamAccountName TEXT NOT NULL,
    DisplayName TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Reason TEXT NOT NULL,
    ExcludedAtUtc TEXT NOT NULL,
    ExcludedBy TEXT NOT NULL,
    PRIMARY KEY (TrainingId, SamAccountName),
    FOREIGN KEY (TrainingId) REFERENCES Trainings(Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_TrainingExclusions_Training ON TrainingExclusions(TrainingId);
";
            await command.ExecuteNonQueryAsync();
        }

        await EnsureColumnAsync(connection, "Trainings", "EstimatedMinutes", "INTEGER NOT NULL DEFAULT 8");
        await EnsureColumnAsync(connection, "Trainings", "SummaryText", "TEXT NOT NULL DEFAULT ''");
        await EnsureColumnAsync(connection, "TrainingCompletions", "StartedAtUtc", "TEXT NOT NULL DEFAULT ''");
        await EnsureColumnAsync(connection, "TrainingCompletions", "DurationSeconds", "INTEGER NOT NULL DEFAULT 0");

        long trainingId;
        using (var check = connection.CreateCommand())
        {
            check.CommandText = "SELECT Id FROM Trainings WHERE Slug = 'seguranca-da-informacao' LIMIT 1;";
            var existing = await check.ExecuteScalarAsync();
            if (existing is not null && existing != DBNull.Value)
            {
                trainingId = Convert.ToInt64(existing);
                using var update = connection.CreateCommand();
                update.CommandText = @"UPDATE Trainings
SET EstimatedMinutes = CASE WHEN EstimatedMinutes<=0 THEN 8 ELSE EstimatedMinutes END,
    SummaryText = CASE WHEN trim(coalesce(SummaryText,''))='' THEN 'Treinamento de conscientizacao sobre protecao de credenciais, prevencao a phishing e engenharia social, tratamento adequado de dados pessoais conforme a LGPD e resposta a incidentes de seguranca. Reforca o uso de senhas fortes e unicas, MFA, a verificacao de mensagens e links suspeitos, o sigilo das informacoes e o reporte imediato de incidentes pelo TOPDESK.' ELSE SummaryText END
WHERE Id=$id;";
                update.Parameters.AddWithValue("$id", trainingId);
                await update.ExecuteNonQueryAsync();
            }
            else
            {
                using var insert = connection.CreateCommand();
                insert.CommandText = @"
INSERT INTO Trainings(Code, Slug, Title, Description, SummaryText, Version, ContentText, PassingScore, EstimatedMinutes, IsPublished, RequiredForAll, LayoutKey, CreatedAtUtc, CreatedBy)
VALUES('SI-001', 'seguranca-da-informacao', 'Treinamento de Conscientização em Segurança da Informação',
'Leia os módulos, responda ao quiz e registre o seu aceite. Tempo estimado: 8 minutos.',
'Treinamento de conscientizacao sobre protecao de credenciais, prevencao a phishing e engenharia social, tratamento adequado de dados pessoais conforme a LGPD e resposta a incidentes de seguranca. Reforca o uso de senhas fortes e unicas, MFA, a verificacao de mensagens e links suspeitos, o sigilo das informacoes e o reporte imediato de incidentes pelo TOPDESK.',
'1.0.0', '', 5, 8, 1, 1, 'security-awareness-v1', $created, 'system');
SELECT last_insert_rowid();";
                insert.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
                trainingId = Convert.ToInt64(await insert.ExecuteScalarAsync());

                var questions = new[]
                {
                    new { Text = "Qual é a prática correta com senhas?", Options = new[] { "Compartilhar com um colega de confiança", "Usar a mesma senha em todos os sistemas", "Usar senha forte e única, com MFA ativado" }, Correct = 2 },
                    new { Text = "O que é a autenticação multifator (MFA)?", Options = new[] { "Um tipo de antivírus", "Uma segunda etapa de verificação além da senha", "Um backup automático" }, Correct = 1 },
                    new { Text = "Você recebe um e-mail urgente pedindo para clicar em um link e informar sua senha. O que fazer?", Options = new[] { "Clicar rápido para não perder o prazo", "Desconfiar, não clicar e reportar", "Encaminhar para os colegas" }, Correct = 1 },
                    new { Text = "O que é dado pessoal?", Options = new[] { "Apenas CPF e RG", "Qualquer informação que identifique uma pessoa (nome, e-mail, telefone...)", "Apenas dados bancários" }, Correct = 1 },
                    new { Text = "Ao perceber um possível incidente de segurança, você deve:", Options = new[] { "Resolver sozinho e não comentar", "Abrir um chamado no TOPDESK imediatamente", "Esperar que alguém perceba" }, Correct = 1 }
                };
                for (var i = 0; i < questions.Length; i++)
                {
                    using var q = connection.CreateCommand();
                    q.CommandText = @"INSERT INTO TrainingQuestions(TrainingId, Position, Text, OptionsJson, CorrectIndex)
VALUES($trainingId, $position, $text, $options, $correct);";
                    q.Parameters.AddWithValue("$trainingId", trainingId);
                    q.Parameters.AddWithValue("$position", i + 1);
                    q.Parameters.AddWithValue("$text", questions[i].Text);
                    q.Parameters.AddWithValue("$options", JsonSerializer.Serialize(questions[i].Options));
                    q.Parameters.AddWithValue("$correct", questions[i].Correct);
                    await q.ExecuteNonQueryAsync();
                }
            }
        }

        var repo = new TrainingRepository(_db);
        var training = await repo.GetTrainingAsync(trainingId);
        var seededQuestions = await repo.GetQuestionsAsync(trainingId);
        if (training is not null)
            await _snapshot.WriteSnapshotAsync(training, seededQuestions);
    }

    private static async Task EnsureColumnAsync(SqliteConnection connection, string table, string column, string definition)
    {
        var found = false;
        using (var info = connection.CreateCommand())
        {
            info.CommandText = $"PRAGMA table_info({table});";
            using var reader = await info.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    break;
                }
            }
        }
        if (found) return;
        using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition};";
        await alter.ExecuteNonQueryAsync();
    }
}
