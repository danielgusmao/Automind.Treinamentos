using System.ComponentModel.DataAnnotations;

namespace Automind.Treinamentos.Models;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class TrainingListItemViewModel
{
    public Training Training { get; set; } = new();
    public TrainingCompletion? Completion { get; set; }
}

public sealed class TrainingTakeViewModel
{
    public Training Training { get; set; } = new();
    public List<TrainingQuestion> Questions { get; set; } = new();
    public TrainingCompletion? ExistingCompletion { get; set; }
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Department { get; set; } = "";
    public DateTime StartedAtUtc { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class TrainingSubmitViewModel
{
    public long TrainingId { get; set; }
    public Dictionary<long, int> Answers { get; set; } = new();
    [Required]
    public bool Accepted { get; set; }
}

public sealed class AdminTrainingCreateViewModel
{
    [Required] public string Code { get; set; } = "";
    [Required] public string Slug { get; set; } = "";
    [Required] public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    [Required] public string Version { get; set; } = "1.0.0";
    public string ContentText { get; set; } = """
MODULO 1 - TITULO DO MODULO
Objetivo: explique em uma frase o que o colaborador deve aprender.

Conteudo:
- Apresente o conceito principal em linguagem simples.
- Liste as boas praticas que devem ser seguidas.
- Explique o que nao deve ser feito.

Exemplo pratico:
Descreva uma situacao real do dia a dia da Automind e qual e a conduta esperada.

MODULO 2 - TITULO DO MODULO
Objetivo: descreva o objetivo deste segundo bloco.

Conteudo:
- Ponto importante 1.
- Ponto importante 2.
- Ponto importante 3.

O que fazer em caso de duvida ou incidente:
Informe o canal, setor ou procedimento que o colaborador deve utilizar.

RESUMO FINAL
Reforce em poucas linhas os comportamentos que o colaborador deve levar para o trabalho.
""";
    [Range(1, 100)] public int PassingScore { get; set; } = 1;
    [Range(1, 480)] public int EstimatedMinutes { get; set; } = 10;
    public bool RequiredForAll { get; set; } = true;
}

public sealed class AdminQuestionCreateViewModel
{
    public long TrainingId { get; set; }
    [Required] public string Text { get; set; } = "";
    [Required] public string OptionA { get; set; } = "";
    [Required] public string OptionB { get; set; } = "";
    [Required] public string OptionC { get; set; } = "";
    public string? OptionD { get; set; }
    [Range(0,3)] public int CorrectIndex { get; set; }
}

public sealed class PendingUserViewModel
{
    public AdUser User { get; set; } = new();
    public bool Completed { get; set; }
}

public sealed class AdminQuestionEditViewModel
{
    public long TrainingId { get; set; }
    public long QuestionId { get; set; }
    [Required] public string Text { get; set; } = "";
    [Required] public string OptionA { get; set; } = "";
    [Required] public string OptionB { get; set; } = "";
    [Required] public string OptionC { get; set; } = "";
    public string? OptionD { get; set; }
    [Range(0,3)] public int CorrectIndex { get; set; }
}
