# Changelog

## v0.0.18 - 2026-10-15

- Revisão ortográfica geral do portal para português brasileiro.
- Correção de acentuação e termos como capacitação, evidência, conclusão, gestão, catálogo, módulos, pendências, código, técnico e mensagens auxiliares.
- Ajuste do aviso de credenciais na tela de login e de mensagens administrativas/complementares.
- Gradiente do bloco de login “Treinar. Comprovar. Evoluir.” alinhado ao padrão institucional.
- Símbolo Automind ampliado e mantido como elemento principal dos banners, incluindo o treinamento dedicado.
- Nenhuma alteração em regra de negócio, AD, banco, Teams, PDFs, Controllers, Services ou Models funcionais.

## v0.0.17 - 2026-10-14

- Refinamento exclusivamente visual baseado na v0.0.16.
- Textos rosa claro de destaque padronizados em `#fa094e`.
- Removidas as linhas/faixas neon decorativas da primeira tela de login, mantendo o fundo existente.
- Removidas as barras degradê decorativas dos banners/hero.
- Símbolo Automind ampliado para preencher melhor os banners, sem adicionar novos elementos gráficos.
- Mesmo refinamento aplicado ao hero do treinamento dedicado de Segurança da Informação.
- Nenhuma alteração em regra de negócio, AD, banco, Teams, PDFs, Controllers, Services ou Models.

## v0.0.16 - 2026-10-13

- Aplicada a composição visual aprovada da opção 2.
- Gradiente institucional consolidado em `#fa094e -> #4d1056`.
- Ajustes visuais globais em hero, login, botões, cards, chips, destaques e treinamento dedicado.
- Nenhuma regra de negócio alterada.

## v0.0.15 - 2026-10-08

- Hardening de login, sessão AD, headers e webhook Teams.
- Validação server-side reforcada no quiz e nos campos administrativos.
- Snapshot/evidência mais consistentes e escrita atomica.
- Integridade SQLite reforcada com indice unico, triggers e registro de migration.
- Auditoria diaria, correlation id e health check.
- Otimizações de consultas SQLite/AD e remocao de código legado sem uso.
- Backup pre-deploy e menor privilegio para a pasta Backup.
- PDF consolidado via POST + antiforgery.
- HTTPS permanece pendente por ausencia de certificado.

## v0.0.14 - revisão visual 2026-10-07

- Redesign exclusivamente visual baseado no site institucional da Automind.
- Removidos cantos arredondados e adotadas linhas retas/geométricas.
- Logo negativa obrigatória nos fundos escuros.
- Paleta e blocos de conteúdo alinhados ao padrão institucional.
- Login passa a exibir `nome.sobrenome` como exemplo de usuário.
- Backend, regras de acesso, banco, serviços, publicação, quiz, evidências e Teams permanecem inalterados.

## v0.0.14 - 2026-10-06

- Mantem evidências individuais organizadas por colaborador e cancela a proposta de separar por treinamento/versão.
- Pasta do colaborador passa a usar somente o login AD: `Evidências\Colaboradores\<samAccountName>`.
- Novos PDFs usam o formato curto `<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf`.
- Limites defensivos reduzem risco de caminhos longos em copia e backup.
- Evidências existentes são migradas quando localizadas; `TrainingCompletions.EvidencePdfPath` e atualizado e o hash/conteúdo do PDF permanece inalterado.
- Em colisão de arquivo no destino, nenhum arquivo e sobrescrito.

## v0.0.13 - 2026-10-06
- Corrigido bug em que `Publicar` podia chegar ao backend como `publish=false` e executar despublicacao.
- Removido `TogglePublish(long id, bool publish)`.
- Criados POSTs separados `PublishTraining` e `UnpublishTraining`.
- Snapshot continua sendo criado antes de marcar a versão como publicada.
- Nenhuma exclusão de treinamento foi adicionada na UI.

## v0.0.12 - 2026-10-06
- Corrigido fluxo de Publicar/Despublicar com feedback de validação e erro.
- Antiforgery token explicito no formulario de publicacao.
- Treinamento sem questões direciona o administrador para cadastrar questões.

## v0.0.11 - 2026-10-06

- Edição de treinamento pelo catálogo.
- Protecao de versoes que já possuem evidências.
- Criacao automatica de nova revisão em rascunho com copia de questões.
- Bloqueio de edição de questões de versoes com conclusões.
- Estados de versão `Publicado`, `Rascunho` e `Histórico`.
- Migracao aditiva com `FamilySlug` e `IsArchived`.

## v0.0.10 - 2026-10-06

- Remove qualquer role administrativa persistida em cookie/sessão.
- Substitui `GetAuthorizationGroups()` por consulta LDAP direta do grupo `_treinamentos` em cada requisicao autenticada.
- Membership direta e aninhada suportadas pelo matching rule in chain.
- Menu administrativo e policy backend passam a usar a mesma decisão atual do AD no request.
- Adiciona `ActiveDirectory:AuthorizationServer` opcional para fixar um DC sem alterar código.
- Mantem comportamento fail-closed em erro de AD.

## v0.0.9 - 2026-10-06

- Corrige autorização administrativa para usar o grupo AD `_treinamentos`.
- Revalida membership administrativa a cada requisicao autenticada.
- Remove privilegio administrativo imediatamente quando o usuário deixa o grupo.
- Ignora/remove a role legada `Informatica` de cookies antigos.
- Politica administrativa renomeada para `TreinamentosAdmin`.

# Changelog

## v0.0.18 - 2026-10-15

- Revisão ortográfica geral do portal para português brasileiro.
- Correção de acentuação e termos como capacitação, evidência, conclusão, gestão, catálogo, módulos, pendências, código, técnico e mensagens auxiliares.
- Ajuste do aviso de credenciais na tela de login e de mensagens administrativas/complementares.
- Gradiente do bloco de login “Treinar. Comprovar. Evoluir.” alinhado ao padrão institucional.
- Símbolo Automind ampliado e mantido como elemento principal dos banners, incluindo o treinamento dedicado.
- Nenhuma alteração em regra de negócio, AD, banco, Teams, PDFs, Controllers, Services ou Models funcionais.

## v0.0.8 - 2026-10-06

- Nova lista global e permanente de contas excluidas da populacao de treinamentos.
- Tela administrativa `Exclusões` com nome, login, e-mail, categoria, motivo, operador, data, pesquisa e reinclusão.
- Histórico de reinclusoes preservado.
- Exclusões globais aplicadas a todos os treinamentos, indicadores, Teams e acesso direto.
- Migracao automatica dos registros anteriores de `TrainingExclusions` para `DirectoryExclusions`.
- Inclusão inicial de `produtos@automind.com.br` como e-mail geral que não representa pessoa.
- Correcao do versionamento do assembly: `0.0.8` passa a ser a fonte oficial da UI.
- `VERSION.txt` do artifact passa a ser gerado pelo build/publish a partir da mesma versão do projeto.
- PDF com resumo da v0.0.7 mantido.

## v0.0.6 - 2026-10-06

- Inclusão de exclusões administrativas por treinamento.
- Motivo obrigatório, auditoria e reinclusão.
- Excluidos deixam de compor elegíveis, pendentes, adesão e lembretes Teams.
- Bloqueio server-side de acesso ao treinamento excluido.
- Nova tabela SQLite `TrainingExclusions`.


## v0.0.5 - 2026-10-06

- Estrutura de producao separada entre `Web` e dados persistentes.
- `appsettings.Production.json` aponta para `C:\Automind.Treinamentos`.
- `App_Data` mantido somente como fallback de desenvolvimento.
- Teams alinhado ao padrao do CadColab com `IHttpClientFactory`, cliente `TeamsWebhook`, `{ recipient, text }` e `Automind__Teams__WebhookUrl`.
- Mensagem de lembrete aprovada com primeiro nome, título, tempo e link clicavel.
- Nova rota `/Training/Start/{id}` e botoes do portal usando `Start`.
- `Portal:PublicBaseUrl=http://treinamentos.automind.com.br`.
- Repositorio Azure DevOps e branch `release` documentados.
- Script de preparo do servidor para migracao não destrutiva, ACLs e copia segura da configuracao Teams.

## v0.0.4 - 2026-10-06

- Edição de questões existentes.
- Atualização de snapshot após adicionar, editar ou excluir questões de treinamento publicado.
- Seleção de colaboradores pendentes com `Marcar todos`.
- Checkbox e envio Teams ocultos para usuários já concluídos.
- Envio Teams individual e em lote com link direto do treinamento.
- Integração com Teams Workflows via `{ recipient, text }`, webhook externo ao Git.
- Modelo editável de conteúdo e exemplo de questão no cadastro de novo treinamento.
- `_informatica` removido da apresentação visual, mantendo autorização AD.
- Avisos visuais de HTTP removidos; versão exibida por `AppVersionInfo`.
- Branch inicial documentada como `release`.

## v0.0.3 - 2026-10-05

- Treinamento de Segurança da Informação integrado ao `_Layout` principal.
- Removido documento HTML independente (`Layout = null`) do treinamento.
- Topbar, navegacao, usuário, versão e logout permanecem durante o treinamento.
- Tela de conclusão tambem integrada ao layout principal.
- Adicionada secao opcional `Styles` no layout compartilhado.
- CSS do treinamento escopado para não alterar componentes globais.
- Conteúdo, quiz 5/5, aceite, cronometro e evidência PDF mantidos.
- Documentacao e checkpoint atualizados.

## v0.0.2 - 2026-10-05

- Redesign expressivo baseado na identidade visual do CadColab.
- Inclusão de logos, simbolo e wallpaper oficiais da Automind.
- Novo login corporativo e topbar.
- Novo dashboard de colaborador e administração.
- Tempo estimado por treinamento.
- Cronometro visual durante a execucao.
- Inicio, conclusão e duracao real gravados pelo servidor.
- Atualizacao incremental do SQLite para os novos campos de tempo.
- PDF individual redesenhado com logo e informações de evidência.
- PDF consolidado redesenhado em paisagem e com duracao.
- Documentacao atualizada.

## v0.0.1 - 2026-10-05

- Estrutura inicial ASP.NET Core MVC.
- Login com validação no Active Directory.
- Autorização administrativa pelo grupo `_informatica`.
- Área `Meus Treinamentos`.
- Primeiro treinamento de Segurança da Informação.
- Quiz 5/5 e aceite.
- SQLite inicial.
- Snapshot versionado do treinamento com SHA-256.
- PDF individual por colaborador.
- PDF consolidado por treinamento.
- Consulta de usuários AD elegíveis e lista de pendentes.
- Auditoria JSONL.
- Cadastro básico de novos treinamentos e questões.
- Perfil HTTP para desenvolvimento, sem HTTPS por enquanto.

## v0.0.7 - 2026-10-06
- PDF individual de evidência passa a ter 2 páginas.
- Nova página "Resumo do treinamento" com descrição objetiva do conteúdo realizado.
- Primeiro treinamento documenta senhas/MFA, phishing/engenharia social, dados pessoais/LGPD e resposta a incidentes/TOPDESK.
- Novo campo `SummaryText` em `Trainings`, com migracao automatica de banco existente.
- Cadastro de novo treinamento ganhou o campo "Resumo para o PDF".
- Para treinamentos genericos sem resumo preenchido, o sistema tenta usar `RESUMO FINAL`, depois `Description` e por fim o proprio conteúdo.
- Rodape do PDF atualizado para v0.0.7.
