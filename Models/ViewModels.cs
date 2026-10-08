using System.ComponentModel.DataAnnotations;

namespace Automind.Treinamentos.Models;

public sealed class LoginViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = "";

    [Required]
    [StringLength(256)]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = "";

    [StringLength(2048)]
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
    [Required, StringLength(40), RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9._-]*$", ErrorMessage = "Codigo invalido.")] public string Code { get; set; } = "";
    [Required, StringLength(80), RegularExpression(@"^[a-z0-9][a-z0-9-]*$", ErrorMessage = "Use apenas letras minusculas, numeros e hifen no slug.")] public string Slug { get; set; } = "";
    [Required, StringLength(180)] public string Title { get; set; } = "";
    [StringLength(800)] public string Description { get; set; } = "";
    [StringLength(4000)] public string SummaryText { get; set; } = "";
    [Required, StringLength(40), RegularExpression(@"^[0-9]+\.[0-9]+\.[0-9]+(?:[-+][A-Za-z0-9.-]+)?$", ErrorMessage = "Use uma versao no formato 1.0.0.")] public string Version { get; set; } = "1.0.0";
    [StringLength(50000)]
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


public sealed class AdminTrainingEditViewModel
{
    public long Id { get; set; }
    [Required, StringLength(40)] public string Code { get; set; } = "";
    [Required, StringLength(80)] public string Slug { get; set; } = "";
    [Required, StringLength(180)] public string Title { get; set; } = "";
    [StringLength(800)] public string Description { get; set; } = "";
    [StringLength(4000)] public string SummaryText { get; set; } = "";
    [Required, StringLength(40), RegularExpression(@"^[0-9]+\.[0-9]+\.[0-9]+(?:[-+][A-Za-z0-9.-]+)?$", ErrorMessage = "Use uma versao no formato 1.0.0.")] public string Version { get; set; } = "1.0.0";
    [StringLength(50000)] public string ContentText { get; set; } = "";
    [Range(1, 100)] public int PassingScore { get; set; } = 1;
    [Range(1, 480)] public int EstimatedMinutes { get; set; } = 10;
    public bool RequiredForAll { get; set; } = true;

    // Somente exibicao/controle de fluxo. O servidor recalcula estes valores no POST.
    public bool HasCompletions { get; set; }
    public int CompletionCount { get; set; }
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public string LayoutKey { get; set; } = "generic";
    public string CurrentVersion { get; set; } = "";
}

public sealed class AdminQuestionCreateViewModel
{
    public long TrainingId { get; set; }
    [Required, StringLength(500)] public string Text { get; set; } = "";
    [Required, StringLength(300)] public string OptionA { get; set; } = "";
    [Required, StringLength(300)] public string OptionB { get; set; } = "";
    [Required, StringLength(300)] public string OptionC { get; set; } = "";
    [StringLength(300)] public string? OptionD { get; set; }
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
    [Required, StringLength(500)] public string Text { get; set; } = "";
    [Required, StringLength(300)] public string OptionA { get; set; } = "";
    [Required, StringLength(300)] public string OptionB { get; set; } = "";
    [Required, StringLength(300)] public string OptionC { get; set; } = "";
    [StringLength(300)] public string? OptionD { get; set; }
    [Range(0,3)] public int CorrectIndex { get; set; }
}
