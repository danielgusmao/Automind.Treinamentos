# Automind.Treinamentos

Versao: **v0.0.12**

Aplicacao interna ASP.NET Core MVC para treinamentos, quiz, aceite, evidencias PDF, acompanhamento de pendencias no Active Directory e comunicacao via Teams.

## Destaques acumulados da v0.0.12

- administracao autorizada exclusivamente pelo grupo AD `_treinamentos`;
- role administrativa nao e persistida em cookie nem em sessao;
- membership administrativa e consultada diretamente no LDAP a cada requisicao autenticada;
- consulta usa o grupo real no AD e suporta membership direta/aninhada;
- cookies antigos com `TreinamentosAdmin` ou `Informatica` nao mantem acesso administrativo indevido;
- a exclusao de contas deixa de ser por treinamento e passa a ser **permanente e global**;
- nova pagina administrativa **Exclusoes**, acessivel pelo topo e pelo painel;
- lista mostra nome, login, e-mail, tipo da conta, motivo, operador e data;
- uma exclusao ativa vale para todos os treinamentos atuais e futuros;
- exclusoes nao entram em elegiveis, pendentes, adesao nem lembretes Teams;
- conta excluida nao recebe treinamento em `Meus treinamentos` e o link direto e bloqueado no backend;
- `Reincluir` restaura a conta para todos os treinamentos;
- historico de reinclusoes fica preservado para auditoria;
- exclusoes antigas da v0.0.6/v0.0.7 sao migradas automaticamente para a lista global;
- `produtos / produtos@automind.com.br` entra como exclusao inicial conhecida, classificada como e-mail geral;
- nenhuma exclusao altera ou apaga objetos no Active Directory;
- PDF individual continua com a segunda pagina de resumo introduzida na v0.0.7;
- estrutura persistente separada entre `Web`, `Data`, `Treinamentos`, `Evidencias`, `Logs` e `Backup` continua mantida;
- Teams continua reutilizando o mesmo Workflow do CadColab por `Automind__Teams__WebhookUrl`.

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

`Web` e descartavel/substituivel pelo Release. Banco, evidencias, snapshots, logs e backups ficam fora da pasta publicada.

## Lista permanente de exclusoes

A fonte inicial de colaboradores continua sendo o AD: usuarios habilitados com e-mail `@automind.com.br`.

Antes de calcular indicadores ou enviar lembretes, o sistema remove as contas presentes em `DirectoryExclusions`.

Exemplos de classificacao:

- `E-mail geral / Caixa compartilhada`;
- `Conta de servico`;
- `Terceiro / Nao colaborador`;
- `Outro`.

A lista e gerenciada em `Administracao > Exclusoes`.

## Teams

A integracao usa o mesmo Workflow/contrato do CadColab:

```json
{ "recipient": "usuario@automind.com.br", "text": "mensagem" }
```

A URL real do Workflow nao esta no projeto. O IIS fornece:

```text
Automind__Teams__WebhookUrl=<segredo>
```

## Repositorios

GitHub:

```text
https://github.com/danielgusmao/Automind.Treinamentos.git
```

Azure DevOps:

```text
https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos
```

Branch de deploy: `release`.

Push na `release` deve acionar a pipeline/Release configurada pelo projeto. Nao fazer deploy manual apos push bem-sucedido, salvo falha da automacao.

## Build

1. Abra `Automind.Treinamentos.sln`.
2. Restore dos pacotes NuGet.
3. Build em Release.
4. Commit e push na branch `release` para GitHub e Azure.

## Dependencias principais

- .NET 10.0 / ASP.NET Core;
- `Microsoft.Data.Sqlite 10.0.12`;
- `System.DirectoryServices 10.0.12`;
- `System.DirectoryServices.AccountManagement 10.0.12`.

## Documentacao

Comece por `docs/00-CHECKPOINT.md`.

- PDF com resumo: `docs/20-PDF-RESUMO-V0.0.7.md`;
- exclusoes permanentes: `docs/22-EXCLUSOES-PERMANENTES-V0.0.8.md`;
- exclusoes v0.0.8: `docs/23-TESTE-V0.0.8.md`;
- historico da tentativa v0.0.9: `docs/24-AUTORIZACAO-ADMIN-V0.0.9.md`;
- autorizacao administrativa atual: `docs/26-AUTORIZACAO-ADMIN-TEMPO-REAL-V0.0.10.md`;
- teste de autorizacao v0.0.10: `docs/27-TESTE-V0.0.10.md`;
- consolidacao completa do projeto: `docs/28-DOCUMENTACAO-COMPLETA-PROJETO.md`;
- edicao/versionamento v0.0.12: `docs/29-EDICAO-E-VERSIONAMENTO-TREINAMENTOS-V0.0.11.md`;
- teste v0.0.12: `docs/30-TESTE-V0.0.11.md`.


## Autorizacao administrativa

Administracao autorizada exclusivamente pelo grupo AD `_treinamentos`, com consulta LDAP direta a cada requisicao autenticada e sem persistencia de role administrativa no cookie.


## Edicao e versionamento

A partir da v0.0.12 o catalogo permite editar treinamentos. Se a versao ainda nao possui conclusoes, a alteracao e direta. Se ja existem evidencias, o sistema preserva a versao anterior e cria uma nova revisao em rascunho com as questoes copiadas.
