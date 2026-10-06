## 2026-10-06 - v0.0.11 edicao e versionamento seguro de treinamentos

- Adicionada acao `Editar` no catalogo administrativo.
- `Code` e `Slug` passam a ser tratados como identificadores tecnicos imutaveis depois da criacao.
- Treinamento sem conclusoes pode ser editado diretamente.
- Treinamento com conclusoes/evidencias nao e alterado retroativamente: salvar cria nova versao em rascunho e copia as questoes.
- Nova versao sugerida automaticamente incrementa o patch quando o formato atual e `X.Y.Z`.
- Questoes de versao com conclusoes ficam somente leitura no frontend e sao bloqueadas tambem no backend.
- Adicionados `Trainings.FamilySlug` e `Trainings.IsArchived` por migracao automatica, sem apagar dados existentes.
- Ao publicar nova versao do mesmo codigo, a versao publicada anterior vira `Historico`; historicos nao recebem Pendentes/Teams nem podem ser republicados diretamente.
- `TrainingController.Completed` passou a localizar a conclusao diretamente por ID + usuario, mantendo acesso a evidencia mesmo quando a versao do treinamento se torna historica.
- O layout dedicado `security-awareness-v1` agora usa titulo e descricao vindos do banco, mas os modulos visuais continuam controlados pelo template; por isso o conteudo livre nao e oferecido para edicao nesse layout.
- A reorganizacao de `Evidencias` por treinamento/versao foi deliberadamente adiada para depois da validacao desta edicao.
- Documentos: `docs/29-EDICAO-E-VERSIONAMENTO-TREINAMENTOS-V0.0.11.md` e `docs/30-TESTE-V0.0.11.md`.

## 2026-10-06 - v0.0.10 autorizacao administrativa LDAP direta

- Evidencia real: v0.0.9 estava publicada e exibia `Colaborador`; Daniel Gusmao foi novamente incluido em `_treinamentos` no AD, mas a area administrativa nao apareceu na requisicao seguinte.
- Revisados todos os pontos de autorizacao do projeto: login/cookie (`AccountController`), autenticacao e ordem de middlewares (`Program.cs`), consulta AD (`AdAuthenticationService`), policy do backend (`AdminController`) e exibicao do menu (`_Layout.cshtml`).
- Causa arquitetural corrigida: `GetAuthorizationGroups()` deixou de ser a fonte de membership administrativa dinamica.
- Nova fonte: consulta LDAP direta ao grupo configurado, usando matching rule in chain para membership direta e aninhada.
- Nenhuma role administrativa e persistida no cookie ou sessao. O cookie guarda apenas identidade basica do colaborador.
- Em cada requisicao, roles antigas `TreinamentosAdmin`/`Informatica` sao removidas do principal em memoria; a role `TreinamentosAdmin` e recriada somente se o LDAP confirmar membership atual.
- Middleware de consulta roda depois de `UseAuthentication` e antes de `UseAuthorization`, garantindo que menu e `[Authorize(Policy = "TreinamentosAdmin")]` usem a mesma decisao.
- Falha de consulta continua fail-closed.
- Criado `ActiveDirectory:AuthorizationServer` opcional. Vazio = dominio escolhe o DC; pode ser definido via `ActiveDirectory__AuthorizationServer` no App Pool se for necessario fixar um DC por causa de replicacao.
- Documentos: `docs/26-AUTORIZACAO-ADMIN-TEMPO-REAL-V0.0.10.md` e `docs/27-TESTE-V0.0.10.md`.
- Gerada consolidacao completa: `docs/28-DOCUMENTACAO-COMPLETA-PROJETO.md`, cobrindo ambiente, AD, autorizacao, exclusoes, PDFs, Teams, pipeline, seguranca, TLS pendente e proximos passos.
- Proxima fase apos validar esta versao: HTTPS/TLS interno.

## 2026-10-06 - v0.0.9 correcao critica de autorizacao administrativa

- Grupo administrativo oficial do Automind.Treinamentos: `_treinamentos`.
- Corrigida a configuracao padrao que ainda apontava para `_informatica`.
- A role interna passou de `Informatica` para `TreinamentosAdmin`; cookies antigos com a role legada nao concedem mais acesso.
- Problema identificado: a role administrativa ficava gravada no cookie por ate 8 horas, portanto uma mudanca de grupo no AD/configuracao nao removia o acesso imediatamente.
- A partir da v0.0.9, cada requisicao autenticada revalida no AD se o usuario pertence atualmente ao grupo `_treinamentos` (ou ao grupo configurado em `ActiveDirectory:AdminGroup`).
- Usuario removido do grupo perde Administracao na requisicao seguinte sem precisar logout; usuario adicionado recebe a role na requisicao seguinte.
- Em falha de consulta ao AD, o comportamento e fail-closed: nao concede privilegio administrativo.
- Usuario fora do grupo continua podendo usar normalmente `Meus treinamentos`.
- Documentos: `docs/24-AUTORIZACAO-ADMIN-V0.0.9.md` e `docs/25-TESTE-V0.0.9.md`.
- HTTPS/TLS permanece como proxima fase; servidor `10.1.2.21` e interno e nao sera exposto a Internet.

## 2026-10-06 - v0.0.8 lista global permanente de exclusoes e versao corrigida

- Exclusoes deixam de ser uma excecao por treinamento e passam a formar uma lista global permanente de contas que nao representam pessoas.
- Nova pagina `Administracao > Exclusoes` mostra nome, login, e-mail, tipo, motivo, operador e data.
- Toda exclusao pode ser revertida por `Reincluir`; o historico de reinclusao e preservado.
- Exclusoes globais nao entram em elegiveis, pendentes, adesao, Teams nem em `Meus treinamentos`, e o backend bloqueia link direto.
- Registros existentes de `TrainingExclusions` sao migrados automaticamente para `DirectoryExclusions`, preservando a tabela anterior para historico/rollback.
- `produtos / produtos@automind.com.br` foi adicionado como conta geral inicial conhecida.
- A lista fica no SQLite persistente em `C:\Automind.Treinamentos\Data`, portanto permanece entre deploys.
- Corrigido o versionamento: a v0.0.7 tinha `VERSION.txt=v0.0.7`, mas o `.csproj` ainda compilava o assembly como `0.0.6`; por isso a UI continuou exibindo a versao anterior.
- Na v0.0.8, `Automind.Treinamentos.csproj` usa `0.0.8` para assembly/file/informational version e o build/publish gera `VERSION.txt` a partir da mesma propriedade.
- A UI continua lendo a versao do assembly, evitando depender de arquivo antigo no servidor.
- Pipeline/Release automaticos na branch `release`: apos push bem-sucedido, nao orientar deploy manual.
- Documentos: `docs/22-EXCLUSOES-PERMANENTES-V0.0.8.md` e `docs/23-TESTE-V0.0.8.md`.

## 2026-10-06 - v0.0.6 exclusao administrativa de colaboradores

- Adicionado `Excluir` apenas para colaboradores pendentes na tela de elegiveis.
- Exclusao exige motivo e vale somente para o treinamento selecionado.
- Excluidos saem dos contadores de elegiveis/pendentes/adesao, nao podem receber Teams e nao veem o treinamento em `Meus treinamentos`.
- Administracao mostra secao de excluidos com motivo, operador, data e acao `Reincluir`.
- Nao existe exclusao ou modificacao de usuario no Active Directory.
- Nova tabela `TrainingExclusions`; migracao automatica e sem perda do banco existente.
- Documento: `docs/18-EXCLUSOES-COLABORADORES-V0.0.6.md`.
- Teste: `docs/19-TESTE-V0.0.6.md`.

## 2026-10-06 - v0.0.5 estrutura persistente separada e reuso do Teams CadColab

### Decisoes aprovadas

- Separar aplicacao publicada de dados persistentes no servidor.
- Estrutura final: `Web`, `Data`, `Treinamentos`, `Evidencias`, `Logs`, `Backup` diretamente sob `C:\Automind.Treinamentos`.
- `Evidencias` permanece dividida em `Colaboradores` e `Relatorios`.
- `Web` passa a ser descartavel/substituivel por pipeline/release; dados persistentes nunca devem ser limpos pelo deploy.
- Production usa `C:\Automind.Treinamentos` como `Storage:RootPath`; desenvolvimento continua usando `App_Data`.
- A migracao inicial deve COPIAR o estado atual de `Web\App_Data` para as pastas novas e manter o antigo `App_Data` temporariamente para rollback.
- App Pool `Automind.Treinamentos` precisa de Modify somente nas pastas persistentes, nao em toda a pasta `Web`.

### Teams

- O ultimo CadColab fornecido foi usado como referencia tecnica.
- Mesmo contrato HTTP: POST JSON `{ recipient, text }`.
- Mesmo segredo externo: `Automind__Teams__WebhookUrl`.
- Mesmo `HttpClient` nomeado `TeamsWebhook` com timeout de 15 segundos.
- O segredo nao entra em Git, appsettings, docs, SQLite ou auditoria.
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
- Nao executar comandos deste projeto dentro de `Automind.CadastroColaboradores`.

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

O grupo AD `_informatica` continua sendo a autorização administrativa, mesmo que seu nome não seja exibido no cabeçalho. A remoção é somente visual.

---

# Automind.Treinamentos - CHECKPOINT

> Documento cumulativo. Sempre adicionar a entrada mais recente no topo. Nao remover decisoes aceitas sem registrar substituicao explicita.

## 2026-10-05 - v0.0.3 treinamento integrado ao layout principal

### Correcao solicitada

Foi confirmado que o arquivo de treinamento baseado no HTML fornecido nao deve abrir com um layout independente. O treinamento precisa permanecer visualmente dentro da mesma pagina/shell do portal Automind.Treinamentos.

### Alteracoes realizadas

- `SecurityAwareness.cshtml` nao usa mais `Layout = null`.
- O treinamento agora usa `_Layout.cshtml`, mantendo topbar, logo, navegacao, usuario, versao e botao `Sair`.
- A tela `Completed.cshtml` recebeu a mesma correcao e tambem permanece no layout principal.
- `_Layout.cshtml` recebeu secao opcional `Styles` para CSS especifico por pagina.
- `security-awareness.css` foi escopado em `.training-experience`, evitando alterar topbar e componentes globais.
- O conteudo do treinamento de SI, quiz 5/5, Termo de Ciencia, cronometro, validacao do servidor e geracao de PDF foram mantidos.
- Versao atualizada para `v0.0.3`.
- Nenhuma alteracao foi feita no servidor `10.1.2.21`.

### Regra permanente de UX

Treinamentos publicados devem abrir dentro do mesmo shell visual do `Automind.Treinamentos`. Um treinamento pode ter composicao interna propria, mas nao deve substituir topbar/navegacao geral por um documento HTML independente.

### Proximo passo

Build e teste local seguindo `docs/10-TESTE-V0.0.3.md`.

---

## 2026-10-05 - v0.0.2 layout, identidade visual, tempo e PDF revisados

### Pedido incorporado

- Melhorar expressivamente o layout do portal.
- Reutilizar a identidade visual e as imagens ja usadas no projeto `Automind.CadastroColaboradores` como referencia.
- Melhorar o comprovante PDF individual, incluindo a marca Automind e uma composicao visual profissional.
- Registrar e exibir o tempo do treinamento.
- Manter toda a documentacao cumulativa.

### Identidade visual aplicada

Foram incorporados ao projeto, a partir do pacote CadColab fornecido pelo usuario:

- `wwwroot/images/automind-logo-negative.png`;
- `wwwroot/images/automind-logo-positive.png`;
- `wwwroot/images/automind-symbol.png`;
- `wwwroot/images/automind-wallpaper.jpg`;
- `wwwroot/images/automind-logo-positive.jpg` derivado apenas para uso no gerador PDF.

O novo layout usa a mesma linguagem visual do Cadastro de Colaboradores: fundo claro, topbar escura, magenta/purpura, cards, hero escuro, logo oficial, wallpaper corporativo na tela de login e responsividade.

### Tempo do treinamento

- Cada treinamento agora possui `EstimatedMinutes`.
- O treinamento inicial de Seguranca da Informacao usa **8 minutos estimados**.
- O tempo real comeca quando o colaborador abre o treinamento.
- O inicio e mantido na sessao do servidor por treinamento.
- Na conclusao o servidor grava `StartedAtUtc`, `AcceptedAtUtc` e `DurationSeconds`.
- O tempo real aparece na tela de conclusao, em `Meus Treinamentos`, no painel administrativo, no PDF individual e no PDF consolidado.
- O navegador mostra um cronometro visual, mas a evidencia usa o tempo calculado no servidor.

### Banco e compatibilidade

A inicializacao ganhou atualizacao segura de esquema para bancos v0.0.1 existentes:

- `Trainings.EstimatedMinutes INTEGER NOT NULL DEFAULT 8`;
- `TrainingCompletions.StartedAtUtc TEXT NOT NULL DEFAULT ''`;
- `TrainingCompletions.DurationSeconds INTEGER NOT NULL DEFAULT 0`.

A rotina verifica `PRAGMA table_info` antes de executar `ALTER TABLE`, evitando recriacao do banco.

### PDF v0.0.2

O comprovante individual deixou de ser um bloco de texto simples e passou a ter:

- logo Automind;
- faixa institucional e identidade magenta/purpura;
- status APROVADO;
- nota;
- versao;
- tempo estimado;
- tempo realizado;
- nome, login AD, e-mail, cargo e departamento;
- inicio e conclusao;
- termo de ciencia;
- protocolo;
- declaracao;
- SHA-256 do snapshot do treinamento.

O relatorio consolidado tambem foi redesenhado em paisagem e inclui tempo realizado por colaborador e media de duracao.

### Layout do portal

- Login em tela dividida com wallpaper e marca Automind.
- Topbar baseada no CadColab, com logo, navegacao, usuario, perfil e versao.
- `Meus Treinamentos` ganhou hero, indicadores e cards mais ricos.
- Administracao ganhou dashboard, indicadores, tabelas e status visuais.
- O treinamento de SI ganhou hero, tempo estimado, cronometro da sessao, cards de modulos, quiz e aceite com melhor hierarquia visual.

### Seguranca mantida

- Senha AD continua sem persistencia.
- Usuario comum continua sem acesso administrativo.
- `_informatica` continua sendo o grupo administrativo.
- HTTP continua temporario e somente para teste interno/localhost ate existir certificado.
- Nenhuma alteracao foi feita no servidor `10.1.2.21`.

### Validacao do pacote

- O exemplo visual do novo PDF foi renderizado em PNG e inspecionado: logo, faixas, cards, tipografia, dados e rodape sem cortes ou sobreposicoes visiveis.
- Arquivo de referencia: `docs/reference/PDF-EVIDENCIA-v0.0.2-EXEMPLO.pdf` (marcado como exemplo, nao como evidencia real).
- Foi feita verificacao estatica do `csproj`, delimitadores dos arquivos C# e existencia dos assets de marca.
- O ambiente de geracao continua sem SDK `dotnet`; portanto o build final deve ser executado no Visual Studio da Automind antes de qualquer publicacao.

### Proximo passo

1. Abrir `Automind.Treinamentos.sln`.
2. Restore.
3. Build Debug.
4. Executar por HTTP local.
5. Validar login, pagina de treinamentos, cronometro e conclusao.
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
│   └── <slug>\<versao>\
│       ├── manifest.json
│       └── SHA256.txt
├── Evidencias\
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

`Treinamentos\<slug>\<versao>\manifest.json`

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
8. Validar criação local de `App_Data\Data`, `Treinamentos`, `Evidencias`, `Logs` e `Backup`.
9. Só depois discutir a publicação no servidor 10.1.2.21.

## 2026-10-06 - v0.0.7 - resumo do treinamento no PDF
- Requisito: toda evidencia individual em PDF deve explicar, de forma resumida, o que foi abordado no treinamento concluido.
- Decisao: o comprovante individual passa a ter duas paginas para nao comprimir os dados de auditoria da primeira pagina.
- Pagina 1: resultado, dados AD, datas, duracao, aceite, protocolo e integridade.
- Pagina 2: resumo do treinamento e principais temas abordados.
- `Trainings.SummaryText` foi adicionado com migracao automatica e sem apagar o banco existente.
- Novo treinamento possui campo administrativo `Resumo para o PDF`.
- Fallback: `SummaryText` -> bloco `RESUMO FINAL` do `ContentText` -> `Description` -> trecho do conteudo.
- Para `security-awareness-v1`, o resumo registra: senhas/MFA, phishing e engenharia social, dados pessoais/LGPD e reporte de incidentes via TOPDESK.
- Esta alteracao nao modifica a regra de conclusao, quiz, elegibilidade, Teams ou exclusoes por treinamento.
