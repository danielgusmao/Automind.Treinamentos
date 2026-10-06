# Banco e Evidencias

## Banco

SQLite: `Automind.Treinamentos.db`.

Em producao:

```text
C:\Automind.Treinamentos\Data\Automind.Treinamentos.db
```

Arquivos `-wal` e `-shm` permanecem na mesma pasta.

### Trainings

Mantem codigo, slug, titulo, descricao, versao, conteudo, nota minima, publicacao, obrigatoriedade, layout e tempo estimado.

### TrainingQuestions

Mantem posicao, pergunta, alternativas e indice da resposta correta.

### TrainingCompletions

Mantem treinamento, usuario AD, dados de identificacao, nota, inicio, conclusao, duracao, protocolo, hash do treinamento, caminho do PDF e hash do PDF.

## Snapshot de treinamento

A publicacao cria, em producao:

```text
C:\Automind.Treinamentos\Treinamentos\<slug>\<versao>\manifest.json
C:\Automind.Treinamentos\Treinamentos\<slug>\<versao>\SHA256.txt
```

## PDF individual

Gerado automaticamente na conclusao em:

```text
C:\Automind.Treinamentos\Evidencias\Colaboradores\<samAccountName>\
```

A partir da v0.0.14, novos arquivos seguem:

```text
<Codigo>_v<Versao>_<AAAAMMDD>_<Protocolo>.pdf
```

A estrutura permanece centrada no colaborador, usando somente o login AD para manter o caminho curto e estavel. Evidencias existentes sao migradas de forma nao destrutiva quando o arquivo e localizado, com atualizacao do caminho gravado no banco.

## PDF consolidado

Gerado sob demanda em:

```text
C:\Automind.Treinamentos\Evidencias\Relatorios\
```

## Auditoria

```text
C:\Automind.Treinamentos\Logs\Audit.jsonl
```

## Imutabilidade operacional

Uma conclusao existente nao e sobrescrita pelo colaborador. Alteracoes de conteudo devem preferir uma nova versao do treinamento para preservar evidencias historicas.
