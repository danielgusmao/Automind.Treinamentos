# CONTEXTO ATUAL - Automind.Treinamentos

Versão de aplicacao preparada: **v0.0.20**.

## v0.0.20 - status Teams simples e responsividade

A tela `Pendências do treinamento` mostra somente `Enviado`, `Falhou` ou `Não enviado`. `Enviado` exige confirmação explícita do Workflow após a ação do Teams e exibe a data/hora do último envio confirmado.

A migration `0.0.20` limpa uma única vez `TeamsReminderHistory`, conforme decisão de iniciar o controle do zero. Auditorias `Audit-*.jsonl` antigas não são mais importadas. O SQLite permanece como fonte oficial do status exibido.

A tabela de pendências foi revisada para não cortar a coluna `Ação`: largura fixa de 1540 px removida, ações fixas à direita em desktop e linhas em formato de cartão em telas pequenas.

Agenda e proteção contra duplicidade permanecem preservadas. A agenda continua desativada por padrão e depende do Workflow real devolver a confirmação de envio e do processo ASP.NET permanecer ativo no IIS.

## v0.0.18 - estado atual

A v0.0.18 consolida a revisão visual anterior e adiciona revisão ortográfica completa dos textos do portal em português brasileiro. Foram corrigidos acentos, termos como capacitação, evidência, conclusão, gestão, catálogo, módulos, pendências e mensagens de apoio. O bloco “Treinar. Comprovar. Evoluir.” teve o gradiente ajustado para o padrão institucional já aprovado, e os banners continuam destacando apenas o símbolo Automind em escala maior. O mesmo padrão foi mantido no treinamento dedicado de Segurança da Informação.

A base visual da v0.0.16 permanece: opção 2 aprovada e gradiente institucional `#fa094e -> #4d1056` em componentes de destaque.

Mudança mais recente: revisão ortográfica geral e ajuste final de gradiente na v0.0.18. Não houve alteração de autenticação, autorização, AD, SQLite, Teams, PDFs, evidências, publicação ou regras de negócio.

## v0.0.16 - revisão visual global

A v0.0.16 adotou a opção 2 aprovada e padronizou `#fa094e -> #4d1056` em hero, botões, cards, chips, destaques e blocos visuais em todo o sistema, sem alterar comportamento funcional.

## v0.0.15 - estado preservado

A v0.0.15 preserva o layout institucional e as regras funcionais da v0.0.14, adicionando hardening, validação, consistencia de evidências, performance, observabilidade e limpeza de código. O unico item da auditoria deliberadamente não ativado e HTTPS/TLS, porque o servidor ainda não possui certificado.


Mudanca mais recente: hardening, consistencia de evidências, performance, observabilidade e limpeza de código na v0.0.15; HTTPS permanece pendente por ausencia de certificado.

## Revisão visual institucional - 2026-10-07

A v0.0.14 recebeu uma revisão somente de interface, baseada no site institucional da Automind. A lógica funcional permanece exatamente a mesma. O layout usa geometria reta, cabeçalho escuro com logo negativa e paleta institucional plum/purple/magenta. O exemplo do campo de usuário no login agora é `nome.sobrenome`.



## Evidências - v0.0.14

Novos PDFs individuais seguem `<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf`. A pasta individual usa somente o login AD: `Evidências/Colaboradores/<login>`. Evidências existentes são migradas para esse padrao quando localizadas, com atualizacao do caminho no banco e sem alterar o hash do PDF.

## Publicacao - v0.0.13

A publicacao e despublicacao usam endpoints separados. O frontend não envia mais um booleano `publish` para um endpoint de toggle. Ao publicar com sucesso, o sistema gera `Treinamentos/<slug-ou-familySlug>/<versão>/manifest.json` e `SHA256.txt` antes de marcar a versão como publicada.


## Edição de treinamentos - v0.0.11

O catálogo administrativo possui `Editar`. Versoes sem conclusões podem ser alteradas diretamente. Quando já existem evidências, a versão fica imutável e o salvamento cria uma nova versão em rascunho com copia das questões. A versão publicada anterior torna-se histórica quando a nova revisão e publicada.

A proposta de reorganizar `Evidências` por treinamento/versão foi cancelada. A estrutura definitiva permanece centrada no colaborador: `Evidências/Colaboradores/<login>`. O nome curto usa código do treinamento, versão, data e protocolo.

# Atualizacao v0.0.11 - autorização administrativa em tempo real

- Grupo administrativo oficial: `_treinamentos`.
- A v0.0.9 ainda dependia de `GetAuthorizationGroups()` e não refletiu de forma confiavel uma inclusão feita durante a sessão.
- A v0.0.10 remove a role administrativa persistida do cookie e da sessão.
- Cada requisicao consulta diretamente o LDAP e cria `TreinamentosAdmin` apenas em memoria para aquele request.
- Menu e policy `/Admin` usam exatamente a mesma decisão.
- Membership direta e aninhada são suportadas.
- Falha LDAP permanece fail-closed.
- `ActiveDirectory__AuthorizationServer` pode fixar um DC por configuracao de ambiente caso a replicacao entre DCs gere atraso operacional.
- HTTPS/TLS interno continua pendente para a proxima fase.

# Contexto Atual - Automind.Treinamentos

## Versão

`v0.0.10`

## Estado atual

Aplicacao interna ASP.NET Core MVC com login AD, administração dinamica pelo grupo `_treinamentos`, SQLite persistente, treinamentos, quiz, aceite, PDFs, comparacao AD, Teams e pipeline/Release automaticos pela branch `release`.

## v0.0.9 - principal mudanca

A autorização administrativa usa exclusivamente o grupo AD `_treinamentos`; cookies antigos não preservam privilegio.

A populacao de treinamento e formada por usuários AD habilitados com e-mail `@automind.com.br`, menos a lista global `DirectoryExclusions`.

A lista e destinada a contas que não representam pessoas, como e-mails gerais e caixas compartilhadas. Ela vale para todos os treinamentos atuais e futuros.

Administradores podem consultar a lista completa, pesquisar, ver motivo/operador/data e reincluir uma conta. O histórico fica preservado.

As exclusões anteriores da v0.0.6/v0.0.7 são migradas para a lista global. `produtos@automind.com.br` e uma exclusão inicial conhecida.

## Regra de indicadores

Contas globalmente excluidas:

- não contam como elegíveis;
- não contam como pendentes;
- não alteram adesão;
- não recebem Teams;
- não veem treinamentos;
- não podem iniciar treinamento por URL direta.

## Versionamento

A v0.0.7 não apareceu corretamente na UI porque o `VERSION.txt` foi alterado para `v0.0.7`, mas o `.csproj` continuou compilando o assembly como `0.0.6`.

A v0.0.8 corrige isso. O assembly é a fonte oficial exibida pela interface e o build/publish gera `VERSION.txt` com a mesma versão.

## Estrutura de producao

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|-- Treinamentos\
|-- Evidências\Colaboradores\
|-- Evidências\Relatorios\
|-- Logs\
`-- Backup\
```

A lista de exclusões fica no SQLite em `Data` e não e perdida quando `Web` e substituido.

## Teams

Continua usando o mesmo Workflow do CadColab via `Automind__Teams__WebhookUrl`, fora de Git/configuracao publicada.

## Repositorios

- GitHub: `https://github.com/danielgusmao/Automind.Treinamentos.git`
- Azure DevOps: `https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos`
- branch de deploy: `release`

## Deploy

Pipeline e Release já estao configurados para deploy automatico a cada alteração na branch `release`. Depois de push bem-sucedido, aguardar a automacao; não fazer deploy manual salvo falha da pipeline/Release.

## Regra visual permanente - tela inicial/login (v0.0.21)

- A tela inicial/login do Automind.Treinamentos usa fundo preto/escuro sólido (`#121019`).
- O gradiente institucional rosa/magenta/roxo é apenas elemento de destaque (botões, bordas, marca e pequenos acentos), nunca o fundo principal da tela inicial.
- Esta é uma exceção visual aprovada do projeto e prevalece sobre regras genéricas do design system Automind.
- Não reintroduzir fundo rosa/gradiente na tela inicial sem pedido explícito do Daniel.
