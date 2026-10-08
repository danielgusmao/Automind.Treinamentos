using System.Globalization;
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
        using (var databasePragmas = connection.CreateCommand())
        {
            databasePragmas.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            await databasePragmas.ExecuteNonQueryAsync();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS Trainings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL,
    Slug TEXT NOT NULL UNIQUE,
    FamilySlug TEXT NOT NULL DEFAULT '',
    Title TEXT NOT NULL,
    Description TEXT NOT NULL DEFAULT '',
    SummaryText TEXT NOT NULL DEFAULT '',
    Version TEXT NOT NULL,
    ContentText TEXT NOT NULL DEFAULT '',
    PassingScore INTEGER NOT NULL,
    EstimatedMinutes INTEGER NOT NULL DEFAULT 8,
    IsPublished INTEGER NOT NULL DEFAULT 0,
    IsArchived INTEGER NOT NULL DEFAULT 0,
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
CREATE TABLE IF NOT EXISTS DirectoryExclusions (
    SamAccountName TEXT PRIMARY KEY COLLATE NOCASE,
    DisplayName TEXT NOT NULL DEFAULT '',
    Email TEXT NOT NULL DEFAULT '',
    Category TEXT NOT NULL DEFAULT 'E-mail geral / Caixa compartilhada',
    Reason TEXT NOT NULL,
    ExcludedAtUtc TEXT NOT NULL,
    ExcludedBy TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    ReincludedAtUtc TEXT NULL,
    ReincludedBy TEXT NULL
);
CREATE INDEX IF NOT EXISTS IX_DirectoryExclusions_Active ON DirectoryExclusions(IsActive);
CREATE INDEX IF NOT EXISTS IX_DirectoryExclusions_Email ON DirectoryExclusions(Email);
";
            await command.ExecuteNonQueryAsync();
        }

        await EnsureColumnAsync(connection, "Trainings", "EstimatedMinutes", "INTEGER NOT NULL DEFAULT 8");
        await EnsureColumnAsync(connection, "Trainings", "FamilySlug", "TEXT NOT NULL DEFAULT ''");
        await EnsureColumnAsync(connection, "Trainings", "IsArchived", "INTEGER NOT NULL DEFAULT 0");
        await EnsureColumnAsync(connection, "Trainings", "SummaryText", "TEXT NOT NULL DEFAULT ''");
        await EnsureColumnAsync(connection, "TrainingCompletions", "StartedAtUtc", "TEXT NOT NULL DEFAULT ''");
        await EnsureColumnAsync(connection, "TrainingCompletions", "DurationSeconds", "INTEGER NOT NULL DEFAULT 0");

        // v0.0.14 revisada: reduz caminhos das evidencias existentes.
        // Pasta do colaborador = somente sAMAccountName; arquivo = codigo, versao, data e protocolo.
        await MigrateEvidencePathsAsync(connection);

        // v0.0.11: FamilySlug identifica a familia logica do treinamento entre revisoes.
        // Slug continua unico e tecnico para manter compatibilidade com o banco existente.
        using (var familySlug = connection.CreateCommand())
        {
            familySlug.CommandText = "UPDATE Trainings SET FamilySlug=Slug WHERE trim(coalesce(FamilySlug,''))='';";
            await familySlug.ExecuteNonQueryAsync();
        }

        // v0.0.15: hardening de integridade e registro explicito da migration aplicada.
        await ApplyV015SchemaHardeningAsync(connection);

        // v0.0.8: converte as exclusoes antigas por treinamento em uma lista permanente
        // aplicavel a todos os treinamentos. A tabela legada e preservada apenas para historico/rollback.
        using (var migrateExclusions = connection.CreateCommand())
        {
            migrateExclusions.CommandText = @"
INSERT OR IGNORE INTO DirectoryExclusions(
    SamAccountName, DisplayName, Email, Category, Reason, ExcludedAtUtc, ExcludedBy, IsActive, ReincludedAtUtc, ReincludedBy)
SELECT
    lower(SamAccountName), DisplayName, Email, 'Migrado da lista anterior', Reason, ExcludedAtUtc, ExcludedBy, 1, NULL, NULL
FROM TrainingExclusions
ORDER BY ExcludedAtUtc DESC;

INSERT OR IGNORE INTO DirectoryExclusions(
    SamAccountName, DisplayName, Email, Category, Reason, ExcludedAtUtc, ExcludedBy, IsActive, ReincludedAtUtc, ReincludedBy)
VALUES(
    'produtos', 'Produtos', 'produtos@automind.com.br', 'E-mail geral / Caixa compartilhada',
    'E-mail de uso geral da empresa; nao representa uma pessoa e nao deve receber treinamentos.',
    $now, 'system-seed', 1, NULL, NULL);";
            migrateExclusions.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
            await migrateExclusions.ExecuteNonQueryAsync();
        }

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
INSERT INTO Trainings(Code, Slug, FamilySlug, Title, Description, SummaryText, Version, ContentText, PassingScore, EstimatedMinutes, IsPublished, IsArchived, RequiredForAll, LayoutKey, CreatedAtUtc, CreatedBy)
VALUES('SI-001', 'seguranca-da-informacao', 'seguranca-da-informacao', 'Treinamento de Conscientização em Segurança da Informação',
'Leia os módulos, responda ao quiz e registre o seu aceite. Tempo estimado: 8 minutos.',
'Treinamento de conscientizacao sobre protecao de credenciais, prevencao a phishing e engenharia social, tratamento adequado de dados pessoais conforme a LGPD e resposta a incidentes de seguranca. Reforca o uso de senhas fortes e unicas, MFA, a verificacao de mensagens e links suspeitos, o sigilo das informacoes e o reporte imediato de incidentes pelo TOPDESK.',
'1.0.0', '', 5, 8, 1, 0, 1, 'security-awareness-v1', $created, 'system');
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

    private async Task MigrateEvidencePathsAsync(SqliteConnection connection)
    {
        var rows = new List<(long Id, string Sam, string Code, string Version, DateTime AcceptedAtUtc, string Protocol, string ExistingPath)>();

        using (var select = connection.CreateCommand())
        {
            select.CommandText = @"
SELECT c.Id, c.SamAccountName, t.Code, t.Version, c.AcceptedAtUtc, c.Protocol, c.EvidencePdfPath
FROM TrainingCompletions c
JOIN Trainings t ON t.Id = c.TrainingId
WHERE trim(coalesce(c.EvidencePdfPath,'')) <> '';";

            using var reader = await select.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var acceptedText = reader.GetString(4);
                if (!DateTime.TryParse(acceptedText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var acceptedAtUtc))
                    continue;

                rows.Add((
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    acceptedAtUtc,
                    reader.GetString(5),
                    reader.GetString(6)));
            }
        }

        foreach (var row in rows)
        {
            var folder = EvidenceNaming.CollaboratorFolder(row.Sam);
            var targetDirectory = Path.Combine(_storage.CollaboratorsEvidencePath, folder);
            var targetFile = EvidenceNaming.IndividualFileName(row.Code, row.Version, row.AcceptedAtUtc, row.Protocol);
            var targetPath = Path.Combine(targetDirectory, targetFile);

            string currentPath;
            string normalizedTarget;
            try
            {
                currentPath = Path.GetFullPath(row.ExistingPath);
                normalizedTarget = Path.GetFullPath(targetPath);
            }
            catch
            {
                continue;
            }

            if (string.Equals(currentPath, normalizedTarget, StringComparison.OrdinalIgnoreCase))
                continue;

            var sourceExists = File.Exists(currentPath);
            var targetExists = File.Exists(normalizedTarget);
            if (!sourceExists && !targetExists)
                continue;

            Directory.CreateDirectory(targetDirectory);

            // Nunca sobrescreve evidencia existente. Em colisao, preserva os dois arquivos e o caminho atual do banco.
            if (sourceExists && targetExists)
                continue;

            var moved = false;
            if (sourceExists)
            {
                File.Move(currentPath, normalizedTarget);
                moved = true;
            }

            try
            {
                using var update = connection.CreateCommand();
                update.CommandText = "UPDATE TrainingCompletions SET EvidencePdfPath=$path WHERE Id=$id;";
                update.Parameters.AddWithValue("$path", normalizedTarget);
                update.Parameters.AddWithValue("$id", row.Id);
                await update.ExecuteNonQueryAsync();
            }
            catch
            {
                // Se o banco falhar depois da movimentacao, tenta devolver o arquivo ao caminho original.
                if (moved && File.Exists(normalizedTarget) && !File.Exists(currentPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(currentPath)!);
                    File.Move(normalizedTarget, currentPath);
                }
                throw;
            }

            try
            {
                var oldDirectory = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrWhiteSpace(oldDirectory) &&
                    Directory.Exists(oldDirectory) &&
                    !Directory.EnumerateFileSystemEntries(oldDirectory).Any())
                {
                    Directory.Delete(oldDirectory);
                }
            }
            catch
            {
                // Limpeza de pasta vazia e opcional; a evidencia e o caminho do banco ja foram preservados.
            }
        }
    }

    private static async Task ApplyV015SchemaHardeningAsync(SqliteConnection connection)
    {
        using (var migrations = connection.CreateCommand())
        {
            migrations.CommandText = @"
CREATE TABLE IF NOT EXISTS SchemaMigrations (
    Version TEXT PRIMARY KEY,
    AppliedAtUtc TEXT NOT NULL
);";
            await migrations.ExecuteNonQueryAsync();
        }

        using (var duplicate = connection.CreateCommand())
        {
            duplicate.CommandText = @"
SELECT Code, Version, COUNT(*)
FROM Trainings
GROUP BY lower(Code), lower(Version)
HAVING COUNT(*) > 1
LIMIT 1;";
            using var reader = await duplicate.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                throw new InvalidOperationException($"Banco possui duplicidade de treinamento para codigo '{reader.GetString(0)}' e versao '{reader.GetString(1)}'. Corrija antes de aplicar v0.0.15.");
        }

        using (var hardening = connection.CreateCommand())
        {
            hardening.CommandText = @"
CREATE UNIQUE INDEX IF NOT EXISTS UX_Trainings_Code_Version
ON Trainings(Code COLLATE NOCASE, Version COLLATE NOCASE);
CREATE INDEX IF NOT EXISTS IX_Trainings_Published_Archived
ON Trainings(IsPublished, IsArchived, Title);
CREATE INDEX IF NOT EXISTS IX_TrainingCompletions_Sam_Training
ON TrainingCompletions(SamAccountName COLLATE NOCASE, TrainingId);

CREATE TRIGGER IF NOT EXISTS TR_Trainings_Validate_Insert
BEFORE INSERT ON Trainings
WHEN NEW.PassingScore < 1 OR NEW.EstimatedMinutes < 1 OR NEW.EstimatedMinutes > 480
  OR NEW.IsPublished NOT IN (0,1) OR NEW.IsArchived NOT IN (0,1) OR NEW.RequiredForAll NOT IN (0,1)
BEGIN SELECT RAISE(ABORT, 'Treinamento com valores invalidos.'); END;

CREATE TRIGGER IF NOT EXISTS TR_Trainings_Validate_Update
BEFORE UPDATE ON Trainings
WHEN NEW.PassingScore < 1 OR NEW.EstimatedMinutes < 1 OR NEW.EstimatedMinutes > 480
  OR NEW.IsPublished NOT IN (0,1) OR NEW.IsArchived NOT IN (0,1) OR NEW.RequiredForAll NOT IN (0,1)
BEGIN SELECT RAISE(ABORT, 'Treinamento com valores invalidos.'); END;

CREATE TRIGGER IF NOT EXISTS TR_Questions_Validate_Insert
BEFORE INSERT ON TrainingQuestions
WHEN NEW.Position < 1 OR NEW.CorrectIndex < 0 OR json_valid(NEW.OptionsJson)=0
  OR json_array_length(NEW.OptionsJson) < 2 OR NEW.CorrectIndex >= json_array_length(NEW.OptionsJson)
BEGIN SELECT RAISE(ABORT, 'Questao com valores invalidos.'); END;

CREATE TRIGGER IF NOT EXISTS TR_Questions_Validate_Update
BEFORE UPDATE ON TrainingQuestions
WHEN NEW.Position < 1 OR NEW.CorrectIndex < 0 OR json_valid(NEW.OptionsJson)=0
  OR json_array_length(NEW.OptionsJson) < 2 OR NEW.CorrectIndex >= json_array_length(NEW.OptionsJson)
BEGIN SELECT RAISE(ABORT, 'Questao com valores invalidos.'); END;

CREATE TRIGGER IF NOT EXISTS TR_Completions_Validate_Insert
BEFORE INSERT ON TrainingCompletions
WHEN NEW.Score < 0 OR NEW.Total < 1 OR NEW.Score > NEW.Total OR NEW.DurationSeconds < 0
BEGIN SELECT RAISE(ABORT, 'Conclusao com valores invalidos.'); END;
";
            await hardening.ExecuteNonQueryAsync();
        }

        using var mark = connection.CreateCommand();
        mark.CommandText = "INSERT OR IGNORE INTO SchemaMigrations(Version, AppliedAtUtc) VALUES('0.0.15', $now);";
        mark.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        await mark.ExecuteNonQueryAsync();
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
