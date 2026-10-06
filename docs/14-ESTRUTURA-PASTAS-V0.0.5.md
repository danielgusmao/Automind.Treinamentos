# v0.0.5 - Estrutura de pastas persistentes

## Decisao aprovada

A aplicacao publicada e os dados persistentes passam a ser fisicamente separados.

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|-- Treinamentos\
|-- Evidencias\
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

Snapshots imutaveis/logicos por `slug/versao`, incluindo `manifest.json` e `SHA256.txt`.

### Evidencias/Colaboradores

Uma pasta por colaborador no formato `Nome - samAccountName`, contendo PDFs individuais de conclusao.

### Evidencias/Relatorios

Relatorios consolidados gerados pela administracao.

### Logs

`Audit.jsonl` protegido pela ACL da aplicacao.

### Backup

Area separada para backups operacionais. Nao e pasta de artefato do pipeline.

## Desenvolvimento

Para nao exigir `C:\Automind.Treinamentos` na maquina do desenvolvedor, `appsettings.json` continua usando `App_Data`. Em Production, `appsettings.Production.json` troca automaticamente a raiz para `C:\Automind.Treinamentos`.

## Rollback da migracao inicial

O script de preparo copia os dados antigos e nao remove `Web\App_Data`. Se houver necessidade de rollback para v0.0.4, os dados antigos permanecem disponiveis ate a limpeza ser autorizada posteriormente.
