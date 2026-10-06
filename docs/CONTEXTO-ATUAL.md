# Atualizacao v0.0.10 - autorizacao administrativa em tempo real

- Grupo administrativo oficial: `_treinamentos`.
- A v0.0.9 ainda dependia de `GetAuthorizationGroups()` e nao refletiu de forma confiavel uma inclusao feita durante a sessao.
- A v0.0.10 remove a role administrativa persistida do cookie e da sessao.
- Cada requisicao consulta diretamente o LDAP e cria `TreinamentosAdmin` apenas em memoria para aquele request.
- Menu e policy `/Admin` usam exatamente a mesma decisao.
- Membership direta e aninhada sao suportadas.
- Falha LDAP permanece fail-closed.
- `ActiveDirectory__AuthorizationServer` pode fixar um DC por configuracao de ambiente caso a replicacao entre DCs gere atraso operacional.
- HTTPS/TLS interno continua pendente para a proxima fase.

# Contexto Atual - Automind.Treinamentos

## Versao

`v0.0.10`

## Estado atual

Aplicacao interna ASP.NET Core MVC com login AD, administracao dinamica pelo grupo `_treinamentos`, SQLite persistente, treinamentos, quiz, aceite, PDFs, comparacao AD, Teams e pipeline/Release automaticos pela branch `release`.

## v0.0.9 - principal mudanca

A autorizacao administrativa usa exclusivamente o grupo AD `_treinamentos`; cookies antigos nao preservam privilegio.

A populacao de treinamento e formada por usuarios AD habilitados com e-mail `@automind.com.br`, menos a lista global `DirectoryExclusions`.

A lista e destinada a contas que nao representam pessoas, como e-mails gerais e caixas compartilhadas. Ela vale para todos os treinamentos atuais e futuros.

Administradores podem consultar a lista completa, pesquisar, ver motivo/operador/data e reincluir uma conta. O historico fica preservado.

As exclusoes anteriores da v0.0.6/v0.0.7 sao migradas para a lista global. `produtos@automind.com.br` e uma exclusao inicial conhecida.

## Regra de indicadores

Contas globalmente excluidas:

- nao contam como elegiveis;
- nao contam como pendentes;
- nao alteram adesao;
- nao recebem Teams;
- nao veem treinamentos;
- nao podem iniciar treinamento por URL direta.

## Versionamento

A v0.0.7 nao apareceu corretamente na UI porque o `VERSION.txt` foi alterado para `v0.0.7`, mas o `.csproj` continuou compilando o assembly como `0.0.6`.

A v0.0.8 corrige isso. O assembly e a fonte oficial exibida pela interface e o build/publish gera `VERSION.txt` com a mesma versao.

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

A lista de exclusoes fica no SQLite em `Data` e nao e perdida quando `Web` e substituido.

## Teams

Continua usando o mesmo Workflow do CadColab via `Automind__Teams__WebhookUrl`, fora de Git/configuracao publicada.

## Repositorios

- GitHub: `https://github.com/danielgusmao/Automind.Treinamentos.git`
- Azure DevOps: `https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos`
- branch de deploy: `release`

## Deploy

Pipeline e Release ja estao configurados para deploy automatico a cada alteracao na branch `release`. Depois de push bem-sucedido, aguardar a automacao; nao fazer deploy manual salvo falha da pipeline/Release.
