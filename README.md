# Automind.Treinamentos

Versao: **v0.0.5**

Aplicacao interna ASP.NET Core MVC para treinamentos, quiz, aceite, evidencias PDF e acompanhamento de pendencias de colaboradores do Active Directory.

## Destaques da v0.0.5

- separacao definitiva entre arquivos publicados e dados persistentes no servidor;
- producao usa `C:\Automind.Treinamentos` como raiz persistente;
- `Web` contem somente a aplicacao publicada;
- banco, treinamentos, evidencias, logs e backup ficam fora de `Web`;
- compatibilidade local mantida com `App_Data` durante desenvolvimento;
- Teams passa a seguir o mesmo contrato e a mesma configuracao externa do CadColab;
- segredo lido de `Automind__Teams__WebhookUrl`, nunca versionado;
- mensagem de lembrete personalizada com primeiro nome, titulo, tempo estimado e link direto;
- rota publica autenticada de treinamento padronizada como `/Training/Start/{id}`;
- link do lembrete usa `http://treinamentos.automind.com.br/Training/Start/{id}`;
- edicao de questoes, selecao de pendentes e envio em lote da v0.0.4 mantidos;
- checkpoint e documentacao cumulativa atualizados.

## Estrutura de producao

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

O `appsettings.Production.json` aponta `Storage:RootPath` para `C:\Automind.Treinamentos`. O `appsettings.json` continua usando `App_Data` para execucao local.

## Teams

A integracao usa o mesmo Workflow/contrato do CadColab:

```json
{ "recipient": "usuario@automind.com.br", "text": "mensagem" }
```

A URL real do Workflow nao esta no projeto. O IIS deve fornecer:

```text
Automind__Teams__WebhookUrl=<segredo>
```

O pacote inclui `deploy/Prepare-Server-v0.0.5.ps1` para preparar a nova estrutura, preservar os dados existentes e copiar a configuracao do webhook do App Pool `CadastroColaboradores` para `Automind.Treinamentos` sem imprimir o segredo.

## Repositorios

GitHub:

```text
https://github.com/danielgusmao/Automind.Treinamentos.git
```

Azure DevOps:

```text
https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos
```

Branch inicial: `release`.

## Build

1. Abra `Automind.Treinamentos.sln`.
2. Restaure os pacotes NuGet.
3. Compile em Release.
4. Para pipeline, publique somente o conteudo da aplicacao para a pasta `Web` do artefato.

## Dependencias principais

- .NET 10.0 / ASP.NET Core;
- `Microsoft.Data.Sqlite 10.0.12`;
- `System.DirectoryServices 10.0.12`;
- `System.DirectoryServices.AccountManagement 10.0.12`.

## Documentacao

Comece por `docs/00-CHECKPOINT.md`.
