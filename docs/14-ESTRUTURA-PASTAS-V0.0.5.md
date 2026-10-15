# v0.0.5 - Estrutura de pastas persistentes

## Decisão aprovada

A aplicacao publicada e os dados persistentes passam a ser fisicamente separados.

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|-- Treinamentos\
|-- Evidências\
|   |-- Colaboradores\
|   `-- Relatorios\
|-- Logs\
`-- Backup\
```

### Web

Somente binaries, `web.config`, `appsettings*.json`, `wwwroot` e demais arquivos de publicacao. Pode ser substituida pelo Release.

### Data

Banco `Automind.Treinamentos.db` e arquivos auxiliares SQLite. Nunca limpar durante deploy.

### Treinamentos

Snapshots imutaveis/logicos por `slug/versão`, incluindo `manifest.json` e `SHA256.txt`.

### Evidências/Colaboradores

Na v0.0.5 a pasta era criada no formato `Nome - samAccountName`. Essa convencao foi substituida na v0.0.14 por somente `samAccountName`/login para reduzir o comprimento dos caminhos. Os PDFs continuam agrupados por colaborador.

### Evidências/Relatorios

Relatorios consolidados gerados pela administração.

### Logs

`Audit.jsonl` protegido pela ACL da aplicacao.

### Backup

Area separada para backups operacionais. Não é pasta de artefato do pipeline.

## Desenvolvimento

Para não exigir `C:\Automind.Treinamentos` na maquina do desenvolvedor, `appsettings.json` continua usando `App_Data`. Em Production, `appsettings.Production.json` troca automaticamente a raiz para `C:\Automind.Treinamentos`.

## Rollback da migracao inicial

O script de preparo copia os dados antigos e não remove `Web\App_Data`. Se houver necessidade de rollback para v0.0.4, os dados antigos permanecem disponíveis ate a limpeza ser autorizada posteriormente.
