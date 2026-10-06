# Banco e Evidências

## Banco

SQLite: `Automind.Treinamentos.db`.

### Trainings

Mantém código, slug, título, descrição, versão, conteúdo, nota mínima, publicação, obrigatoriedade e layout.

### TrainingQuestions

Mantém posição, pergunta, alternativas e índice da resposta correta.

### TrainingCompletions

Mantém:

- treinamento;
- usuário AD;
- dados de identificação;
- nota;
- data/hora UTC;
- protocolo;
- hash do treinamento;
- caminho do PDF;
- hash do PDF.

## Snapshot de treinamento

A publicação cria `manifest.json` e `SHA256.txt`.

## PDF individual

Gerado automaticamente na conclusão e gravado na pasta individual do colaborador.

## PDF consolidado

Gerado sob demanda pelo painel administrativo em `Evidencias\Relatorios`.

## Imutabilidade operacional

Na v0.0.2, uma conclusão existente não é sobrescrita pelo colaborador. Mudanças futuras de conteúdo devem preferir nova versão do treinamento, preservando evidências históricas.
