# Contexto Atual - Automind.Treinamentos

## Versao

`v0.0.6`

## Estado atual

Aplicacao interna ASP.NET Core MVC com autenticacao AD, administracao autorizada pelo grupo `_informatica`, SQLite, treinamentos, quiz, aceite, evidencias PDF, comparacao de concluidos/pendentes no AD, lembretes Teams e identidade visual alinhada ao CadColab.

## Novidade principal v0.0.6

Administradores podem excluir colaboradores pendentes da obrigacao de um treinamento sem alterar o objeto no Active Directory. A exclusao exige motivo, registra operador e data, remove o colaborador dos contadores de elegiveis/pendentes/adesao e impede lembretes Teams e acesso ao treinamento enquanto a excecao estiver ativa. A tela administrativa mantem uma lista separada de excluidos com opcao de reinclusao.

A estrutura de producao permanece separada: `Web` contem somente a publicacao e os dados persistentes ficam em `Data`, `Treinamentos`, `Evidencias`, `Logs` e `Backup`, todos fora da pasta publicada. O Teams continua usando o mesmo Workflow do CadColab e a configuracao externa `Automind__Teams__WebhookUrl`.

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
- exclusao por treinamento com motivo e reinclusao;
- excluidos nao contam como elegiveis/pendentes e nao recebem Teams;
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

1. Extrair a v0.0.6 para `C:\Users\daniel.gusmao\source\repos\Automind.Treinamentos`.
2. Build Release.
3. Inicializar Git na pasta correta e fazer push para `origin` e `azure` na branch `release`.
4. Criar pipeline e Release.
5. Antes da primeira publicacao v0.0.6 no servidor, executar o preparo/migracao controlado das pastas e copiar a configuracao Teams entre os App Pools.
6. Publicar somente `Web`.
7. Validar `docs/19-TESTE-V0.0.6.md`.

## Atualizacao v0.0.7 - PDF com resumo
- Evidencia individual agora possui segunda pagina com resumo e temas do treinamento.
- Campo persistente `Trainings.SummaryText` adicionado com migracao automatica.
- Cadastro administrativo de treinamento possui `Resumo para o PDF`.
- Primeiro treinamento resume senhas/MFA, phishing/engenharia social, dados pessoais/LGPD e incidentes/TOPDESK.
- PDFs antigos nao sao regenerados automaticamente; novos aceites usam o formato v0.0.7.
