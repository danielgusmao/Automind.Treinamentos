# Contexto Atual - Automind.Treinamentos

## Versao

`v0.0.5`

## Estado atual

Aplicacao interna ASP.NET Core MVC com autenticacao AD, administracao autorizada pelo grupo `_informatica`, SQLite, treinamentos, quiz, aceite, evidencias PDF, comparacao de concluidos/pendentes no AD, lembretes Teams e identidade visual alinhada ao CadColab.

## Novidade principal v0.0.5

A estrutura de producao foi separada: `Web` contem somente a publicacao e os dados persistentes ficam em `Data`, `Treinamentos`, `Evidencias`, `Logs` e `Backup`, todos fora da pasta publicada. Em Production, a raiz e `C:\Automind.Treinamentos`; em desenvolvimento continua `App_Data`.

O Teams agora replica explicitamente o padrao tecnico do CadColab: mesmo contrato `{ recipient, text }`, mesmo nome de configuracao externa `Automind__Teams__WebhookUrl` e mesmo tipo de Workflow. A URL real continua fora do repositorio. O link do treinamento foi padronizado para `/Training/Start/{id}` e o lembrete usa `http://treinamentos.automind.com.br` como base publica.

## Estrutura de producao

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|-- Treinamentos\
|-- Evidencias\Colaboradores\
|-- Evidencias\Relatorios\
|-- Logs\
`-- Backup\
```

## Recursos atuais

- login AD sem persistencia de senha;
- administracao por `_informatica`, sem exibir o nome do grupo na interface;
- edicao de questoes;
- criacao de treinamentos com modelo inicial;
- selecao individual e `Marcar todos os pendentes`;
- concluido sem checkbox e sem botao de lembrete;
- Teams individual/em lote;
- rota `/Training/Start/{id}`;
- link publico configuravel por `Portal:PublicBaseUrl`;
- cronometro e tempo real calculado pelo servidor;
- evidencias PDF individuais;
- PDF consolidado;
- snapshot de treinamento e SHA-256;
- auditoria JSONL.

## Repositorios

- GitHub: `https://github.com/danielgusmao/Automind.Treinamentos.git`
- Azure DevOps: `https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos`
- branch inicial: `release`

## Proxima acao

1. Extrair a v0.0.5 para `C:\Users\daniel.gusmao\source\repos\Automind.Treinamentos`.
2. Build Release.
3. Inicializar Git na pasta correta e fazer push para `origin` e `azure` na branch `release`.
4. Criar pipeline e Release.
5. Antes da primeira publicacao v0.0.5 no servidor, executar o preparo/migracao controlado das pastas e copiar a configuracao Teams entre os App Pools.
6. Publicar somente `Web`.
7. Validar `docs/17-TESTE-V0.0.5.md`.
