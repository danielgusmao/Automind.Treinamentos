# CHECKPOINT - Automind.Treinamentos

## 2026-10-15 - v0.0.18 - Revisão ortográfica geral e ajuste final de gradiente

- Baseada na v0.0.17, sem alteração de autenticação, autorização AD, regras de negócio, banco SQLite, Services, Teams, PDFs, publicação ou evidências.
- Revisados textos do portal para português brasileiro, com correção de acentos e grafias em telas de login, administração, treinamento e mensagens auxiliares.
- Corrigidos especificamente termos solicitados como capacitação, evidência e conclusão, além de ajustes em gestão, catálogo, módulos, pendências, código e identificação técnica.
- Aviso de credenciais da tela de login atualizado para “Credenciais não são armazenadas. A senha é utilizada apenas para validação no Active Directory.”
- Fundo do bloco “Treinar. Comprovar. Evoluir.” ajustado para o gradiente institucional já adotado nos banners principais.
- Banners principais e hero do treinamento dedicado seguem com símbolo Automind ampliado e sem barras decorativas adicionais.
- Versão atualizada para `v0.0.18` em `Automind.Treinamentos.csproj`, `VERSION.txt`, `README.md`, `docs/CHANGELOG.md` e `docs/CONTEXTO-ATUAL.md`.
- Evidência desta rodada documentada em `docs/43-REVISAO-ORTOGRAFIA-V0.0.18.md`.

## 2026-10-14 - v0.0.17 - Refinamento visual final do login e banners

- Atualização baseada integralmente na v0.0.16 e limitada à camada visual, versionamento e documentação.
- Mantida a decisão anterior de não alterar autenticação, autorização AD, regras de negócio, banco SQLite, Controllers, Services, Models, Teams, PDFs, publicação, evidências ou estrutura de produção.
- Textos de destaque que usavam rosa claro (`#ff6c96`) foram padronizados no magenta institucional `#fa094e`.
- A cor informada no pedido como `#fa094` foi normalizada para `#fa094e`, pois hexadecimal CSS de cinco dígitos não é válido e `#fa094e` já é a cor institucional adotada pelo projeto.
- Login: removidas as duas faixas/linhas neon inclinadas; o fundo em gradiente aprovado foi preservado sem alteração de composição textual ou fluxo de autenticação.
- Hero principal do portal e hero administrativo: removidas as barras degradê decorativas de fundo; o símbolo Automind permanece como único elemento gráfico e foi ampliado para preencher mais a área direita do banner.
- Treinamento dedicado de Segurança da Informação recebeu o mesmo tratamento: sem barras decorativas, símbolo Automind maior e texto de destaque em `#fa094e`.
- Responsividade existente foi preservada; em telas menores o símbolo continua oculto conforme regra já existente.
- Versão atualizada para `v0.0.17` em `Automind.Treinamentos.csproj` e `VERSION.txt`.
- Documentação consolidada em `README.md`, `docs/CHANGELOG.md`, `docs/CONTEXTO-ATUAL.md`, `docs/41-REFINO-VISUAL-V0.0.17.md` e `docs/42-TESTE-V0.0.17.md`.
- Branch de publicação permanece `release`; push bem-sucedido deve seguir a pipeline/Release automática já configurada, sem deploy manual salvo falha da automação.

## 2026-10-13 - v0.0.16 - Ajuste visual global de destaque e gradiente

- Revisão exclusivamente visual, sem alteração de regra de negocio, autenticação, autorização, banco, controllers, services, publicacao, evidências ou fluxos.
- Hero/capa principal reformulado em todo o sistema seguindo a referencia aprovada da opcao 2: simbolo Automind maior e mais destacado na lateral direita, sem se misturar com o fundo.
- Gradiente institucional corrigido para usar `#fa094e -> #4d1056` nos pontos de destaque.
- Atualizados layout global, cards, botoes, chips, destaques, modais, campos com icone e tela de login para manter a mesma linguagem visual no sistema todo.
- Ajustado tambem o treinamento dedicado de Segurança da Informação para seguir a mesma identidade visual, preservando comportamento e JavaScript.
- Escopo alterado apenas em `wwwroot/css/site.css`, `wwwroot/css/security-awareness.css` e `VERSION.txt`.

## 2026-10-08 - v0.0.15 - Hardening, consistencia, performance e limpeza

- Pacote baseado na v0.0.14 com layout institucional preservado.
- Logica funcional do portal mantida; alterações concentradas em segurança, consistencia, validação, observabilidade, performance e limpeza de código.
- Rate limit adicionado ao login.
- Sessão de usuário comum revalida conta habilitada/elegivel no AD em cache curto de 2 minutos.
- Autorização administrativa continua em `_treinamentos`, server-side, com membership revalidada e fail-closed.
- Timeouts LDAP e caches curtos reduzem consultas repetidas.
- Quiz passa a rejeitar respostas incompletas, IDs desconhecidos e indices inválidos no servidor.
- Gabarito não e mais enviado no HTML do treinamento dedicado.
- Conclusão/PDF não e desfeita por falha posterior de auditoria.
- Snapshot e validado contra o estado atual e escrito atomicamente.
- SQLite recebe UNIQUE `Code+Version`, triggers de integridade, indices adicionais e `SchemaMigrations`.
- PDF usa versão real do assembly.
- Auditoria diaria com retencao configuravel e correlation id.
- `/health` adicionado.
- App Pool perde escrita em `Backup`; script de servidor cria backup SQLite antes da atualizacao.
- N+1 removido da página inicial e do total de conclusões do dashboard.
- Removidos metodos/model legado sem consumidores e atributo HttpPost duplicado.
- PDF consolidado persistente agora usa POST + antiforgery.
- SSL/HTTPS continua PENDENTE por ausencia de certificado; URL e cookies continuam HTTP nesta versão por decisão operacional.
- Documentos: `docs/37-HARDENING-E-OTIMIZACOES-V0.0.15.md` e `docs/38-TESTE-V0.0.15.md`.

## 2026-10-07 - v0.0.14 - Revisão visual institucional sem alteração de lógica

- Aplicado redesign completo da camada visual baseado no site institucional da Automind.
- Identidade: linhas retas, sem cantos arredondados, cabeçalho escuro, logo negativa sobre fundos escuros e paleta plum/purple/magenta.
- Reformulados visualmente login, layout global, catálogo administrativo, lista de treinamentos e treinamento dedicado de Segurança da Informação.
- Placeholder do usuário no login alterado de `daniel.gusmao` para `nome.sobrenome`.
- Nenhum arquivo de backend, autorização, autenticação, banco, serviços, controllers, publicação, evidências, Teams ou deploy foi alterado.
- Rotas, nomes de campos, IDs funcionais e ações Razor foram preservados.
- JavaScript de validação/quiz do treinamento dedicado permaneceu idêntico.
- Versão continua **v0.0.14**; trata-se de revisão exclusivamente visual do pacote já validado funcionalmente.
- Documento: `docs/36-REDESIGN-INSTITUCIONAL-V0.0.14.md`.

## 2026-10-06 - v0.0.14 - Caminhos curtos das evidências por colaborador

- Decisão final: manter todas as evidências de uma pessoa juntas, sem separar fisicamente por treinamento/versão.
- Para reduzir risco de caminhos longos em copia/backup, a pasta individual deixa de usar `Nome - login` e passa a usar somente o login AD (`sAMAccountName`).
- Estrutura: `Evidências\Colaboradores\<login>\`. Exemplo: `Evidências\Colaboradores\daniel.gusmao\`.
- O nome do PDF tambem usa formato curto: `<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf`.
- Exemplo: `SI-001_v1.0.0_20261006_AM-20261006-35E0.pdf`.
- Limites defensivos: login 32, código 20, versão 16 e protocolo 32 caracteres.
- Evidências existentes registradas em `TrainingCompletions` são migradas na inicializacao quando o arquivo e localizado: arquivo e movido para a pasta curta, renomeado pelo padrao curto e `EvidencePdfPath` e atualizado.
- O conteúdo e o hash do PDF não são alterados pela migracao.
- O sistema nunca sobrescreve arquivo existente no destino; em colisão preserva o caminho atual.
- `Evidências\Relatorios` permanece inalterada.
- Versão do aplicativo continua **v0.0.14**; esta revisão substitui o empacotamento anterior da propria v0.0.14.
- Documentos: `docs/34-NOME-PDF-EVIDENCIAS-V0.0.14.md` e `docs/35-TESTE-V0.0.14.md`.

## 2026-10-06 - v0.0.13 - Publicacao explicita sem toggle booleano

- Falha observada em producao na v0.0.12: clicar em `Publicar` no SI-002 executava o caminho de despublicacao e exibia `despublicado com sucesso`.
- Confirmado no código que o catálogo usava um unico endpoint `TogglePublish(id, bool publish)` e dependia do bind de um campo booleano oculto.
- Removido o toggle booleano da interface e do controller para eliminar ambiguidade de binding.
- Criados endpoints POST separados: `PublishTraining(long id)` e `UnpublishTraining(long id)`.
- O botao `Publicar` agora chama somente `PublishTraining`; o botao `Despublicar` chama somente `UnpublishTraining`.
- Ao publicar, o snapshot e gravado antes da alteração de estado em `C:\Automind.Treinamentos\Treinamentos\<familySlug-ou-slug>\<versão>`.
- Publicacao valida questões e nota mínima; falha de snapshot/banco e mostrada ao administrador.
- Nenhum botao de exclusão de treinamento foi criado; exclusão de treinamento de teste continua sendo procedimento manual e controlado.
- A reorganizacao de `Evidências` por treinamento/versão continua pendente ate a publicacao ser validada.

## 2026-10-06 - v0.0.12 - Correcao do fluxo Publicar/Despublicar

- Corrigido o fluxo do botao **Publicar** no catálogo administrativo.
- O POST agora envia antiforgery token explicitamente.
- Treinamento sem questões não retorna mais BadRequest sem contexto: o administrador e direcionado para a tela de questões com mensagem clara.
- Nota mínima maior que a quantidade de questões tambem gera mensagem de validação.
- Falhas ao gerar snapshot, gravar banco ou auditar são exibidas no catálogo em vez de parecer que o clique não teve efeito.
- Publicacao/despublicacao bem sucedida exibe confirmacao.
- A reorganizacao das evidências por `<slug>/<versão>/Colaboradores|Relatorios` permanece pendente ate concluir a validação da edição/publicacao.

## 2026-10-06 - v0.0.11 edição e versionamento seguro de treinamentos

- Adicionada acao `Editar` no catálogo administrativo.
- `Code` e `Slug` passam a ser tratados como identificadores técnicos imutaveis depois da criação.
- Treinamento sem conclusões pode ser editado diretamente.
- Treinamento com conclusões/evidências não é alterado retroativamente: salvar cria nova versão em rascunho e copia as questões.
- Nova versão sugerida automaticamente incrementa o patch quando o formato atual e `X.Y.Z`.
- Questões de versão com conclusões ficam somente leitura no frontend e são bloqueadas tambem no backend.
- Adicionados `Trainings.FamilySlug` e `Trainings.IsArchived` por migracao automatica, sem apagar dados existentes.
- Ao publicar nova versão do mesmo código, a versão publicada anterior vira `Histórico`; históricos não recebem Pendentes/Teams nem podem ser republicados diretamente.
- `TrainingController.Completed` passou a localizar a conclusão diretamente por ID + usuário, mantendo acesso a evidência mesmo quando a versão do treinamento se torna histórica.
- O layout dedicado `security-awareness-v1` agora usa título e descrição vindos do banco, mas os módulos visuais continuam controlados pelo template; por isso o conteúdo livre não e oferecido para edição nesse layout.
- A reorganizacao de `Evidências` por treinamento/versão foi deliberadamente adiada para depois da validação desta edição.
- Documentos: `docs/29-EDICAO-E-VERSIONAMENTO-TREINAMENTOS-V0.0.11.md` e `docs/30-TESTE-V0.0.11.md`.

## 2026-10-06 - v0.0.10 autorização administrativa LDAP direta

- Evidência real: v0.0.9 estava publicada e exibia `Colaborador`; Daniel Gusmao foi novamente incluido em `_treinamentos` no AD, mas a area administrativa não apareceu na requisicao seguinte.
- Revisados todos os pontos de autorização do projeto: login/cookie (`AccountController`), autenticacao e ordem de middlewares (`Program.cs`), consulta AD (`AdAuthenticationService`), policy do backend (`AdminController`) e exibicao do menu (`_Layout.cshtml`).
- Causa arquitetural corrigida: `GetAuthorizationGroups()` deixou de ser a fonte de membership administrativa dinamica.
- Nova fonte: consulta LDAP direta ao grupo configurado, usando matching rule in chain para membership direta e aninhada.
- Nenhuma role administrativa é persistida no cookie ou sessão. O cookie guarda apenas identidade basica do colaborador.
- Em cada requisicao, roles antigas `TreinamentosAdmin`/`Informatica` são removidas do principal em memoria; a role `TreinamentosAdmin` e recriada somente se o LDAP confirmar membership atual.
- Middleware de consulta roda depois de `UseAuthentication` e antes de `UseAuthorization`, garantindo que menu e `[Authorize(Policy = "TreinamentosAdmin")]` usem a mesma decisão.
- Falha de consulta continua fail-closed.
- Criado `ActiveDirectory:AuthorizationServer` opcional. Vazio = dominio escolhe o DC; pode ser definido via `ActiveDirectory__AuthorizationServer` no App Pool se for necessario fixar um DC por causa de replicacao.
- Documentos: `docs/26-AUTORIZACAO-ADMIN-TEMPO-REAL-V0.0.10.md` e `docs/27-TESTE-V0.0.10.md`.
- Gerada consolidacao completa: `docs/28-DOCUMENTACAO-COMPLETA-PROJETO.md`, cobrindo ambiente, AD, autorização, exclusões, PDFs, Teams, pipeline, segurança, TLS pendente e proximos passos.
- Proxima fase apos validar esta versão: HTTPS/TLS interno.

## 2026-10-06 - v0.0.9 correcao critica de autorização administrativa

- Grupo administrativo oficial do Automind.Treinamentos: `_treinamentos`.
- Corrigida a configuracao padrao que ainda apontava para `_informatica`.
- A role interna passou de `Informatica` para `TreinamentosAdmin`; cookies antigos com a role legada não concedem mais acesso.
- Problema identificado: a role administrativa ficava gravada no cookie por ate 8 horas, portanto uma mudanca de grupo no AD/configuracao não removia o acesso imediatamente.
- A partir da v0.0.9, cada requisicao autenticada revalida no AD se o usuário pertence atualmente ao grupo `_treinamentos` (ou ao grupo configurado em `ActiveDirectory:AdminGroup`).
- Usuário removido do grupo perde Administração na requisicao seguinte sem precisar logout; usuário adicionado recebe a role na requisicao seguinte.
- Em falha de consulta ao AD, o comportamento e fail-closed: não concede privilegio administrativo.
- Usuário fora do grupo continua podendo usar normalmente `Meus treinamentos`.
- Documentos: `docs/24-AUTORIZACAO-ADMIN-V0.0.9.md` e `docs/25-TESTE-V0.0.9.md`.
- HTTPS/TLS permanece como proxima fase; servidor `10.1.2.21` e interno e não será exposto a Internet.

## 2026-10-06 - v0.0.8 lista global permanente de exclusões e versão corrigida

- Exclusões deixam de ser uma excecao por treinamento e passam a formar uma lista global permanente de contas que não representam pessoas.
- Nova página `Administração > Exclusões` mostra nome, login, e-mail, tipo, motivo, operador e data.
- Toda exclusão pode ser revertida por `Reincluir`; o histórico de reinclusão e preservado.
- Exclusões globais não entram em elegíveis, pendentes, adesão, Teams nem em `Meus treinamentos`, e o backend bloqueia link direto.
- Registros existentes de `TrainingExclusions` são migrados automaticamente para `DirectoryExclusions`, preservando a tabela anterior para histórico/rollback.
- `produtos / produtos@automind.com.br` foi adicionado como conta geral inicial conhecida.
- A lista fica no SQLite persistente em `C:\Automind.Treinamentos\Data`, portanto permanece entre deploys.
- Corrigido o versionamento: a v0.0.7 tinha `VERSION.txt=v0.0.7`, mas o `.csproj` ainda compilava o assembly como `0.0.6`; por isso a UI continuou exibindo a versão anterior.
- Na v0.0.8, `Automind.Treinamentos.csproj` usa `0.0.8` para assembly/file/informational version e o build/publish gera `VERSION.txt` a partir da mesma propriedade.
- A UI continua lendo a versão do assembly, evitando depender de arquivo antigo no servidor.
- Pipeline/Release automaticos na branch `release`: apos push bem-sucedido, não orientar deploy manual.
- Documentos: `docs/22-EXCLUSOES-PERMANENTES-V0.0.8.md` e `docs/23-TESTE-V0.0.8.md`.

## 2026-10-06 - v0.0.6 exclusão administrativa de colaboradores

- Adicionado `Excluir` apenas para colaboradores pendentes na tela de elegíveis.
- Exclusão exige motivo e vale somente para o treinamento selecionado.
- Excluidos saem dos contadores de elegíveis/pendentes/adesão, não podem receber Teams e não veem o treinamento em `Meus treinamentos`.
- Administração mostra secao de excluidos com motivo, operador, data e acao `Reincluir`.
- Não éxiste exclusão ou modificacao de usuário no Active Directory.
- Nova tabela `TrainingExclusions`; migracao automatica e sem perda do banco existente.
- Documento: `docs/18-EXCLUSOES-COLABORADORES-V0.0.6.md`.
- Teste: `docs/19-TESTE-V0.0.6.md`.

## 2026-10-06 - v0.0.5 estrutura persistente separada e reuso do Teams CadColab

### Decisoes aprovadas

- Separar aplicacao publicada de dados persistentes no servidor.
- Estrutura final: `Web`, `Data`, `Treinamentos`, `Evidências`, `Logs`, `Backup` diretamente sob `C:\Automind.Treinamentos`.
- `Evidências` permanece dividida em `Colaboradores` e `Relatorios`.
- `Web` passa a ser descartavel/substituivel por pipeline/release; dados persistentes nunca devem ser limpos pelo deploy.
- Production usa `C:\Automind.Treinamentos` como `Storage:RootPath`; desenvolvimento continua usando `App_Data`.
- A migracao inicial deve COPIAR o estado atual de `Web\App_Data` para as pastas novas e manter o antigo `App_Data` temporariamente para rollback.
- App Pool `Automind.Treinamentos` precisa de Modify somente nas pastas persistentes, não em toda a pasta `Web`.

### Teams

- O ultimo CadColab fornecido foi usado como referencia tecnica.
- Mesmo contrato HTTP: POST JSON `{ recipient, text }`.
- Mesmo segredo externo: `Automind__Teams__WebhookUrl`.
- Mesmo `HttpClient` nomeado `TeamsWebhook` com timeout de 15 segundos.
- O segredo não entra em Git, appsettings, docs, SQLite ou auditoria.
- O preparo do servidor deve copiar o valor da variavel do App Pool `CadastroColaboradores` para `Automind.Treinamentos` sem imprimir o valor.
- Mensagem aprovada: primeiro nome + treinamento pendente + tempo estimado + link direto.
- Base publica: `http://treinamentos.automind.com.br`.
- Rota de acesso padrao: `/Training/Start/{id}`.
- O primeiro treinamento gera `http://treinamentos.automind.com.br/Training/Start/1`.

### Git / Azure

- GitHub: `https://github.com/danielgusmao/Automind.Treinamentos.git`.
- Azure DevOps: `https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos`.
- Branch inicial: `release`.
- Pasta local correta: `C:\Users\daniel.gusmao\source\repos\Automind.Treinamentos`.
- Não éxecutar comandos deste projeto dentro de `Automind.CadastroColaboradores`.

### Documentos novos

- `docs/14-ESTRUTURA-PASTAS-V0.0.5.md`.
- `docs/15-TEAMS-CADCOLAB-V0.0.5.md`.
- `docs/16-GIT-AZURE-RELEASE-V0.0.5.md`.
- `docs/17-TESTE-V0.0.5.md`.
- `deploy/Prepare-Server-v0.0.5.ps1`.

---

## 2026-10-06 - v0.0.4 administração de questões, lembretes Teams e modelo de treinamento

### Alterações solicitadas e incorporadas

- A tela de questões ganhou ação **Editar** para alterar pergunta, alternativas e resposta correta.
- Ao editar, adicionar ou excluir uma questão de treinamento já publicado, o snapshot é regravado para refletir o conteúdo atual.
- A tela de pendências ganhou seleção individual somente para colaboradores pendentes.
- Colaboradores já concluídos não exibem checkbox de seleção nem botão de envio Teams.
- Foi adicionado **Marcar todos os pendentes** / **Desmarcar todos**.
- Foi adicionado envio Teams individual e em lote para os selecionados.
- O lembrete Teams contém nome do treinamento, tempo estimado e link absoluto para abrir diretamente o treinamento.
- O backend revalida o AD e o estado de conclusão antes de enviar, evitando envio para usuário já concluído por manipulação da tela.
- O envio usa o mesmo contrato aprovado no CadColab para Teams Workflows: `{ recipient, text }`.
- A URL do webhook continua segredo externo ao Git e deve vir de `Automind__Teams__WebhookUrl`; somente HTTPS é aceito e, sem URL válida, a UI mantém os controles visíveis porém desabilitados.
- Cada tentativa de lembrete é registrada no `Audit.jsonl` sem senha ou segredo.
- O cadastro de novo treinamento agora abre com um modelo editável de conteúdo organizado em módulos, objetivos, boas práticas, exemplo prático, resposta a incidentes e resumo.
- A mesma tela mostra um exemplo explícito de como montar questões e gabarito.
- O texto `_informatica` foi removido da apresentação visual do usuário; a autorização pelo grupo AD permanece inalterada no backend.
- O aviso visual `HTTP interno de teste · HTTPS pendente de certificado` foi removido do rodapé e substituído pela versão da aplicação.
- O aviso `v0.0.2/v0.0.3 - ambiente de teste HTTP...` foi removido da tela inicial de login.
- A versão visível passa a vir da versão do assembly (`AppVersionInfo`) e foi atualizada para `v0.0.4`.
- Branch inicial definida para este projeto: `release`.
- Roteiro de validação da versão criado em `docs/13-TESTE-V0.0.4.md`.
- Repositório GitHub confirmado: `https://github.com/danielgusmao/Automind.Treinamentos.git`.
- URL do repositório Azure DevOps deste projeto ainda precisa ser informada; não reutilizar URL de outro projeto.

### Estado de infraestrutura já observado no teste anterior

- Site IIS: `Automind.Treinamentos` em `C:\Automind.Treinamentos\Web`.
- App Pool: `Automind.Treinamentos`, `No Managed Code`, `Integrated`, `ApplicationPoolIdentity`.
- O erro HTTP 500.30 anterior foi causado por falta de escrita no `App_Data`.
- Foi concedido `Modify` somente para `IIS AppPool\Automind.Treinamentos` em `C:\Automind.Treinamentos\Web\App_Data` e descendentes.
- Depois da correção, o site respondeu HTTP 200 e o ASP.NET Core registrou inicialização bem-sucedida.

### Configuração externa necessária para Teams

Não gravar o webhook em `appsettings.json`, Git, documentação, SQLite ou auditoria.

Variáveis esperadas pelo aplicativo:

```text
Automind__Teams__Enabled=true
Automind__Teams__WebhookUrl=<segredo configurado somente no ambiente do IIS>
```

### Regra permanente

O grupo AD `_informatica` continua sendo a autorização administrativa, mesmo que seu nome não sejá exibido no cabeçalho. A remoção é somente visual.

---

# Automind.Treinamentos - CHECKPOINT

> Documento cumulativo. Sempre adicionar a entrada mais recente no topo. Não remover decisoes aceitas sem registrar substituicao explicita.

## 2026-10-05 - v0.0.3 treinamento integrado ao layout principal

### Correcao solicitada

Foi confirmado que o arquivo de treinamento baseado no HTML fornecido não deve abrir com um layout independente. O treinamento precisa permanecer visualmente dentro da mesma página/shell do portal Automind.Treinamentos.

### Alterações realizadas

- `SecurityAwareness.cshtml` não usa mais `Layout = null`.
- O treinamento agora usa `_Layout.cshtml`, mantendo topbar, logo, navegacao, usuário, versão e botao `Sair`.
- A tela `Completed.cshtml` recebeu a mesma correcao e tambem permanece no layout principal.
- `_Layout.cshtml` recebeu secao opcional `Styles` para CSS especifico por página.
- `security-awareness.css` foi escopado em `.training-experience`, evitando alterar topbar e componentes globais.
- O conteúdo do treinamento de SI, quiz 5/5, Termo de Ciência, cronometro, validação do servidor e geracao de PDF foram mantidos.
- Versão atualizada para `v0.0.3`.
- Nenhuma alteração foi feita no servidor `10.1.2.21`.

### Regra permanente de UX

Treinamentos publicados devem abrir dentro do mesmo shell visual do `Automind.Treinamentos`. Um treinamento pode ter composicao interna propria, mas não deve substituir topbar/navegacao geral por um documento HTML independente.

### Proximo passo

Build e teste local seguindo `docs/10-TESTE-V0.0.3.md`.

---

## 2026-10-05 - v0.0.2 layout, identidade visual, tempo e PDF revisados

### Pedido incorporado

- Melhorar expressivamente o layout do portal.
- Reutilizar a identidade visual e as imagens já usadas no projeto `Automind.CadastroColaboradores` como referencia.
- Melhorar o comprovante PDF individual, incluindo a marca Automind e uma composicao visual profissional.
- Registrar e exibir o tempo do treinamento.
- Manter toda a documentacao cumulativa.

### Identidade visual aplicada

Foram incorporados ao projeto, a partir do pacote CadColab fornecido pelo usuário:

- `wwwroot/images/automind-logo-negative.png`;
- `wwwroot/images/automind-logo-positive.png`;
- `wwwroot/images/automind-symbol.png`;
- `wwwroot/images/automind-wallpaper.jpg`;
- `wwwroot/images/automind-logo-positive.jpg` derivado apenas para uso no gerador PDF.

O novo layout usa a mesma linguagem visual do Cadastro de Colaboradores: fundo claro, topbar escura, magenta/purpura, cards, hero escuro, logo oficial, wallpaper corporativo na tela de login e responsividade.

### Tempo do treinamento

- Cada treinamento agora possui `EstimatedMinutes`.
- O treinamento inicial de Segurança da Informação usa **8 minutos estimados**.
- O tempo real comeca quando o colaborador abre o treinamento.
- O inicio e mantido na sessão do servidor por treinamento.
- Na conclusão o servidor grava `StartedAtUtc`, `AcceptedAtUtc` e `DurationSeconds`.
- O tempo real aparece na tela de conclusão, em `Meus Treinamentos`, no painel administrativo, no PDF individual e no PDF consolidado.
- O navegador mostra um cronometro visual, mas a evidência usa o tempo calculado no servidor.

### Banco e compatibilidade

A inicializacao ganhou atualizacao segura de esquema para bancos v0.0.1 existentes:

- `Trainings.EstimatedMinutes INTEGER NOT NULL DEFAULT 8`;
- `TrainingCompletions.StartedAtUtc TEXT NOT NULL DEFAULT ''`;
- `TrainingCompletions.DurationSeconds INTEGER NOT NULL DEFAULT 0`.

A rotina verifica `PRAGMA table_info` antes de executar `ALTER TABLE`, evitando recriação do banco.

### PDF v0.0.2

O comprovante individual deixou de ser um bloco de texto simples e passou a ter:

- logo Automind;
- faixa institucional e identidade magenta/purpura;
- status APROVADO;
- nota;
- versão;
- tempo estimado;
- tempo realizado;
- nome, login AD, e-mail, cargo e departamento;
- inicio e conclusão;
- termo de ciencia;
- protocolo;
- declaracao;
- SHA-256 do snapshot do treinamento.

O relatorio consolidado tambem foi redesenhado em paisagem e inclui tempo realizado por colaborador e media de duracao.

### Layout do portal

- Login em tela dividida com wallpaper e marca Automind.
- Topbar baseada no CadColab, com logo, navegacao, usuário, perfil e versão.
- `Meus Treinamentos` ganhou hero, indicadores e cards mais ricos.
- Administração ganhou dashboard, indicadores, tabelas e status visuais.
- O treinamento de SI ganhou hero, tempo estimado, cronometro da sessão, cards de módulos, quiz e aceite com melhor hierarquia visual.

### Segurança mantida

- Senha AD continua sem persistencia.
- Usuário comum continua sem acesso administrativo.
- `_informatica` continua sendo o grupo administrativo.
- HTTP continua temporario e somente para teste interno/localhost ate existir certificado.
- Nenhuma alteração foi feita no servidor `10.1.2.21`.

### Validação do pacote

- O exemplo visual do novo PDF foi renderizado em PNG e inspecionado: logo, faixas, cards, tipografia, dados e rodape sem cortes ou sobreposicoes visiveis.
- Arquivo de referencia: `docs/reference/PDF-EVIDENCIA-v0.0.2-EXEMPLO.pdf` (marcado como exemplo, não como evidência real).
- Foi feita verificacao estatica do `csproj`, delimitadores dos arquivos C# e existencia dos assets de marca.
- O ambiente de geracao continua sem SDK `dotnet`; portanto o build final deve ser executado no Visual Studio da Automind antes de qualquer publicacao.

### Proximo passo

1. Abrir `Automind.Treinamentos.sln`.
2. Restore.
3. Build Debug.
4. Executar por HTTP local.
5. Validar login, página de treinamentos, cronometro e conclusão.
6. Abrir o PDF individual gerado e conferir logo, dados e tempo realizado.
7. Somente depois discutir publicacao no IIS.

---


## 2026-10-05 - v0.0.1 gerada para primeiro teste local

### Estado atual

- Projeto criado: `Automind.Treinamentos`.
- Versão da aplicação: `v0.0.1`.
- Tecnologia: ASP.NET Core MVC em `.NET 10.0` para Windows.
- HTTPS **não habilitado nesta versão**, por decisão operacional: ainda não há certificado disponível para o novo site.
- `UseHttpsRedirection()` não é usado na v0.0.1.
- URL de desenvolvimento prevista: `http://localhost:5180`.
- Ainda **não houve alteração no servidor 10.1.2.21**.
- Ainda **não houve criação de Site/App Pool no IIS**.
- Pacote atual é somente código-fonte para abrir, restaurar, compilar e testar no Visual Studio.

### Regra de segurança temporária

A tela de login envia usuário e senha do AD para a própria aplicação para validação no domínio. Sem HTTPS, o transporte HTTP não protege essas credenciais contra captura na rede.

**Até existir HTTPS, testar preferencialmente em `localhost` ou em ambiente interno controlado com conta de teste. Não publicar externamente e não usar em rede não confiável.**

A senha:

- não é salva no SQLite;
- não é gravada em arquivo;
- não entra em log;
- não entra em claims/cookie;
- é usada somente para `ValidateCredentials` no AD.

### Autenticação e autorização definidas

- Domínio: `automind.com.br`.
- Login do colaborador: usuário + senha do Active Directory.
- Conta AD precisa estar habilitada para entrar.
- Usuário comum vê apenas `Meus Treinamentos`.
- Membro do grupo AD `_informatica` recebe perfil administrativo.
- Em caso de erro ao resolver os grupos do usuário, **não** elevar privilégio administrativo.

### Usuários considerados para pendências

No painel administrativo, a lista de elegíveis vem do Active Directory com a regra:

- objeto do tipo `user`;
- conta habilitada;
- atributo `mail` terminando em `@automind.com.br`.

O sistema compara essa lista com as conclusões gravadas no banco para mostrar quem concluiu e quem ainda está pendente.

### Primeiro treinamento

Primeiro treinamento: `Treinamento de Conscientização em Segurança da Informação`.

Fonte visual/conteúdo: `docs/reference/treinamento_si_original.html`.

SHA-256 do arquivo de referência:

`7CA5CC5FCBE0B4CA39E0A123BB6C44FF76895D4BBCE4C29A145318358D3995A3`

Regras preservadas:

- 3 módulos: Senhas/MFA, Phishing/Engenharia Social, Dados Pessoais/LGPD;
- quiz com 5 questões;
- nota mínima inicial: 5/5;
- somente depois da aprovação o Termo de Ciência é liberado na interface;
- o servidor recalcula a nota novamente antes de aceitar a conclusão;
- nome, cargo/área e e-mail deixam de ser digitados manualmente e passam a vir da sessão autenticada/AD;
- aceite gera protocolo, data/hora do servidor e PDF individual.

### Evidências

O banco SQLite é a fonte oficial de registro. PDFs são evidências derivadas.

Estrutura planejada para produção no servidor:

```text
C:\Automind.Treinamentos\
├── Web\
├── Data\
│   └── Automind.Treinamentos.db
├── Treinamentos\
│   └── <slug>\<versão>\
│       ├── manifest.json
│       └── SHA256.txt
├── Evidências\
│   ├── Colaboradores\
│   │   └── <Nome> - <samAccountName>\
│   │       └── <data> - <treinamento> - <protocolo>.pdf
│   └── Relatorios\
│       └── Consolidado-<treinamento>-<data>.pdf
├── Logs\
│   └── Audit.jsonl
└── Backup\
```

Para desenvolvimento, a v0.0.1 usa por padrão `App_Data` dentro do projeto. Em produção, o `Storage:RootPath` será alterado para `C:\Automind.Treinamentos`.

### Evidência individual

Cada conclusão gera um PDF em pasta própria do colaborador contendo, no mínimo:

- treinamento;
- versão;
- nome;
- login AD;
- e-mail;
- cargo;
- departamento;
- resultado;
- aceite;
- data/hora UTC e local;
- protocolo;
- SHA-256 do snapshot do treinamento.

O banco também grava o SHA-256 do PDF gerado.

### Pasta geral dos treinamentos

Ao publicar um treinamento, é criado um snapshot em:

`Treinamentos\<slug>\<versão>\manifest.json`

E um arquivo `SHA256.txt` com o hash do `manifest.json`.

A conclusão fica vinculada ao hash da versão do treinamento usada como evidência.

### Administração `_informatica`

A v0.0.1 inclui:

- dashboard básico;
- lista de treinamentos;
- cadastro de novo treinamento;
- cadastro/exclusão de questões;
- publicar/despublicar treinamento;
- lista de concluídos;
- lista de usuários AD concluídos/pendentes;
- abertura do PDF individual;
- geração de PDF consolidado do treinamento.

Novo treinamento cadastrado pela interface usa conteúdo em **texto simples**, não HTML arbitrário. Isso evita transformar o painel em um ponto de injeção de scripts.

### Banco inicial

Arquivo: `Automind.Treinamentos.db`.

Tabelas principais:

- `Trainings`;
- `TrainingQuestions`;
- `TrainingCompletions`.

Restrição atual: uma conclusão por `TrainingId + SamAccountName`. Para reciclagem/novo ciclo obrigatório, criar nova versão como novo treinamento/registro versionado em release futura.

### Auditoria

Arquivo previsto: `Logs\Audit.jsonl`.

Eventos já modelados:

- login com sucesso/falha;
- logout;
- criação de treinamento;
- inclusão/exclusão de questão;
- publicar/despublicar;
- conclusão de treinamento;
- falha ao registrar conclusão;
- geração de relatório consolidado.

Nenhuma senha deve aparecer na auditoria.

### Publicação futura no servidor 10.1.2.21

Ainda pendente e **não autorizada/executada** nesta etapa:

- definir identidade técnica/App Pool específica para `Automind.Treinamentos`;
- criar pasta final `C:\Automind.Treinamentos`;
- criar ACLs mínimas;
- criar App Pool;
- criar site/binding HTTP;
- definir host/DNS;
- validar consulta ao AD pela identidade do App Pool;
- backup/rollback antes de qualquer alteração real;
- posteriormente habilitar HTTPS quando houver certificado.

### Validação deste pacote

O ambiente usado para gerar o pacote não possui o SDK `dotnet`, portanto o projeto **não pôde ser compilado aqui**. O primeiro build deve ser feito no Visual Studio da Automind com `.NET 10.0` instalado.

Ao aparecer qualquer erro no primeiro build, corrigir antes de avançar para IIS.

### Próximo passo

1. Extrair o pacote na máquina de desenvolvimento.
2. Abrir `Automind.Treinamentos.sln`.
3. Restaurar os pacotes NuGet.
4. Compilar em `Debug`.
5. Executar pelo perfil HTTP em `http://localhost:5180`.
6. Testar primeiro com uma conta AD de teste comum.
7. Testar depois com um membro de `_informatica`.
8. Validar criação local de `App_Data\Data`, `Treinamentos`, `Evidências`, `Logs` e `Backup`.
9. Só depois discutir a publicação no servidor 10.1.2.21.

## 2026-10-06 - v0.0.7 - resumo do treinamento no PDF
- Requisito: toda evidência individual em PDF deve explicar, de forma resumida, o que foi abordado no treinamento concluído.
- Decisão: o comprovante individual passa a ter duas páginas para não comprimir os dados de auditoria da primeira página.
- Página 1: resultado, dados AD, datas, duracao, aceite, protocolo e integridade.
- Página 2: resumo do treinamento e principais temas abordados.
- `Trainings.SummaryText` foi adicionado com migracao automatica e sem apagar o banco existente.
- Novo treinamento possui campo administrativo `Resumo para o PDF`.
- Fallback: `SummaryText` -> bloco `RESUMO FINAL` do `ContentText` -> `Description` -> trecho do conteúdo.
- Para `security-awareness-v1`, o resumo registra: senhas/MFA, phishing e engenharia social, dados pessoais/LGPD e reporte de incidentes via TOPDESK.
- Esta alteração não modifica a regra de conclusão, quiz, elegibilidade, Teams ou exclusões por treinamento.
