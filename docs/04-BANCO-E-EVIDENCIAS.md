# Banco e Evidências

## Banco

SQLite: `Automind.Treinamentos.db`.

Em producao:

```text
C:\Automind.Treinamentos\Data\Automind.Treinamentos.db
```

Arquivos `-wal` e `-shm` permanecem na mesma pasta.

### Trainings

Mantem código, slug, título, descrição, versão, conteúdo, nota mínima, publicacao, obrigatoriedade, layout e tempo estimado.

### TrainingQuestions

Mantem posicao, pergunta, alternativas e indice da resposta correta.

### TrainingCompletions

Mantem treinamento, usuário AD, dados de identificacao, nota, inicio, conclusão, duracao, protocolo, hash do treinamento, caminho do PDF e hash do PDF.

## Snapshot de treinamento

A publicacao cria, em producao:

```text
C:\Automind.Treinamentos\Treinamentos\<slug>\<versão>\manifest.json
C:\Automind.Treinamentos\Treinamentos\<slug>\<versão>\SHA256.txt
```

## PDF individual

Gerado automaticamente na conclusão em:

```text
C:\Automind.Treinamentos\Evidências\Colaboradores\<samAccountName>\
```

A partir da v0.0.14, novos arquivos seguem:

```text
<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf
```

A estrutura permanece centrada no colaborador, usando somente o login AD para manter o caminho curto e estavel. Evidências existentes são migradas de forma não destrutiva quando o arquivo e localizado, com atualizacao do caminho gravado no banco.

## PDF consolidado

Gerado sob demanda em:

```text
C:\Automind.Treinamentos\Evidências\Relatorios\
```

## Auditoria

```text
C:\Automind.Treinamentos\Logs\Audit.jsonl
```

## Imutabilidade operacional

Uma conclusão existente não e sobrescrita pelo colaborador. Alterações de conteúdo devem preferir uma nova versão do treinamento para preservar evidências históricas.
