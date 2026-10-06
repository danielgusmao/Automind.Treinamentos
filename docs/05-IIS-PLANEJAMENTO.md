# Planejamento IIS e armazenamento

Servidor alvo: `10.1.2.21`.

## Estrutura aprovada a partir da v0.0.5

```text
C:\Automind.Treinamentos\
|-- Web\                    # somente aplicacao publicada pelo pipeline/release
|-- Data\                   # SQLite e arquivos WAL/SHM
|-- Treinamentos\           # snapshots por slug/versao e SHA-256
|-- Evidencias\
|   |-- Colaboradores\      # pasta individual Nome - samAccountName
|   `-- Relatorios\         # PDFs consolidados
|-- Logs\                   # Audit.jsonl
`-- Backup\                 # area reservada para backups operacionais
```

`Web` nao deve mais conter dados persistentes em producao.

## Configuracao de ambiente

- `appsettings.json`: `Storage:RootPath=App_Data`, somente para desenvolvimento/local.
- `appsettings.Production.json`: `Storage:RootPath=C:\Automind.Treinamentos`.
- IIS usa ambiente Production por padrao.

## ACL minima pretendida

Identidade: `IIS AppPool\Automind.Treinamentos`.

- `Web`: leitura/execucao, sem necessidade de Modify da aplicacao.
- `Data`: Modify.
- `Treinamentos`: Modify.
- `Evidencias`: Modify.
- `Logs`: Modify.
- `Backup`: Modify.

Nao conceder Modify na raiz inteira se nao for necessario.

## Migracao do estado v0.0.4

A v0.0.4 gravava em `C:\Automind.Treinamentos\Web\App_Data`. A primeira preparacao da v0.0.5 deve:

1. parar somente o App Pool `Automind.Treinamentos`;
2. criar as pastas persistentes externas;
3. copiar `Data`, `Treinamentos`, `Evidencias`, `Logs` e `Backup` do `Web\App_Data` para as pastas novas;
4. preservar `Web\App_Data` temporariamente como rollback, sem apagar;
5. aplicar ACL minima nas pastas novas;
6. copiar o segredo Teams do App Pool `CadastroColaboradores` para `Automind.Treinamentos` sem exibir o valor;
7. iniciar somente o App Pool `Automind.Treinamentos`.

O script `deploy/Prepare-Server-v0.0.5.ps1` automatiza esse preparo e nao executa `iisreset`.

## Publicacao por pipeline/release

A pipeline deve gerar somente a aplicacao publicada. O Release deve substituir/publicar em:

```text
C:\Automind.Treinamentos\Web
```

Nunca usar a raiz `C:\Automind.Treinamentos` como pasta de limpeza/deploy, pois `Data`, `Treinamentos`, `Evidencias`, `Logs` e `Backup` sao persistentes.

## HTTPS

Ainda nao habilitado. Quando houver certificado, revisar binding IIS, `CookieSecurePolicy`, redirecionamento HTTPS e HSTS.
