using Automind.Treinamentos.Models;
using Automind.Treinamentos.Services;

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

var questions = new List<TrainingQuestion>
{
    new() { Id = 10, Position = 1, Text = "Q1", Options = new() { "A", "B", "C" }, CorrectIndex = 1 },
    new() { Id = 20, Position = 2, Text = "Q2", Options = new() { "A", "B" }, CorrectIndex = 0 }
};

var valid = QuizValidationService.Validate(questions, new Dictionary<long, int> { [10] = 1, [20] = 0 });
Assert(valid.IsStructurallyValid && valid.Score == 2, "Quiz valido deveria pontuar 2/2.");

var missing = QuizValidationService.Validate(questions, new Dictionary<long, int> { [10] = 1 });
Assert(!missing.IsStructurallyValid, "Quiz incompleto deve ser rejeitado.");

var forged = QuizValidationService.Validate(questions, new Dictionary<long, int> { [10] = 1, [20] = 0, [999] = 0 });
Assert(!forged.IsStructurallyValid, "ID de questao desconhecido deve ser rejeitado.");

var invalidOption = QuizValidationService.Validate(questions, new Dictionary<long, int> { [10] = 99, [20] = 0 });
Assert(!invalidOption.IsStructurallyValid, "Indice de resposta fora das opcoes deve ser rejeitado.");

Assert(StorageService.ValidatePathSegment("seguranca-da-informacao", "slug") == "seguranca-da-informacao", "Slug valido deve ser aceito.");
var blockedTraversal = false;
try { StorageService.ValidatePathSegment("..", "slug"); }
catch (InvalidOperationException) { blockedTraversal = true; }
Assert(blockedTraversal, "Segmento '..' deve ser rejeitado.");

var evidenceName = EvidenceNaming.IndividualFileName("SI-001", "1.0.0", new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc), "AM-20261008-ABCD");
Assert(evidenceName == "SI-001_v1.0.0_20261008_AM-20261008-ABCD.pdf", "Nome de evidencia mudou inesperadamente.");

Console.WriteLine("SelfTests v0.0.15: OK");
