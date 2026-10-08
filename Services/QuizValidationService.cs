using Automind.Treinamentos.Models;

namespace Automind.Treinamentos.Services;

public static class QuizValidationService
{
    public static QuizValidationResult Validate(
        IReadOnlyCollection<TrainingQuestion> questions,
        IReadOnlyDictionary<long, int> answers)
    {
        if (questions.Count == 0)
            return new QuizValidationResult(false, 0, "Treinamento sem questoes validas.");

        var validQuestionIds = questions.Select(q => q.Id).ToHashSet();
        if (answers.Keys.Any(id => !validQuestionIds.Contains(id)))
            return new QuizValidationResult(false, 0, "Foram recebidas respostas para questoes que nao pertencem ao treinamento.");

        if (!questions.All(q => answers.ContainsKey(q.Id)))
            return new QuizValidationResult(false, 0, "Nem todas as questoes foram respondidas.");

        if (questions.Any(q =>
            !answers.TryGetValue(q.Id, out var answer) || answer < 0 || answer >= q.Options.Count))
            return new QuizValidationResult(false, 0, "Uma ou mais respostas possuem opcao invalida.");

        var score = questions.Count(q => answers[q.Id] == q.CorrectIndex);
        return new QuizValidationResult(true, score, null);
    }
}

public sealed record QuizValidationResult(bool IsStructurallyValid, int Score, string? Error);
