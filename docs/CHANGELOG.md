# Changelog

## v0.0.6 - 2026-10-06

- Inclusao de exclusoes administrativas por treinamento.
- Motivo obrigatorio, auditoria e reinclusao.
- Excluidos deixam de compor elegiveis, pendentes, adesao e lembretes Teams.
- Bloqueio server-side de acesso ao treinamento excluido.
- Nova tabela SQLite `TrainingExclusions`.


## v0.0.5 - 2026-10-06

- Estrutura de producao separada entre `Web` e dados persistentes.
- `appsettings.Production.json` aponta para `C:\Automind.Treinamentos`.
- `App_Data` mantido somente como fallback de desenvolvimento.
- Teams alinhado ao padrao do CadColab com `IHttpClientFactory`, cliente `TeamsWebhook`, `{ recipient, text }` e `Automind__Teams__WebhookUrl`.
- Mensagem de lembrete aprovada com primeiro nome, titulo, tempo e link clicavel.
- Nova rota `/Training/Start/{id}` e botoes do portal usando `Start`.
- `Portal:PublicBaseUrl=http://treinamentos.automind.com.br`.
- Repositorio Azure DevOps e branch `release` documentados.
- Script de preparo do servidor para migracao nao destrutiva, ACLs e copia segura da configuracao Teams.

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

- Treinamento de Seguranca da Informacao integrado ao `_Layout` principal.
- Removido documento HTML independente (`Layout = null`) do treinamento.
- Topbar, navegacao, usuario, versao e logout permanecem durante o treinamento.
- Tela de conclusao tambem integrada ao layout principal.
- Adicionada secao opcional `Styles` no layout compartilhado.
- CSS do treinamento escopado para nao alterar componentes globais.
- Conteudo, quiz 5/5, aceite, cronometro e evidencia PDF mantidos.
- Documentacao e checkpoint atualizados.

## v0.0.2 - 2026-10-05

- Redesign expressivo baseado na identidade visual do CadColab.
- Inclusao de logos, simbolo e wallpaper oficiais da Automind.
- Novo login corporativo e topbar.
- Novo dashboard de colaborador e administracao.
- Tempo estimado por treinamento.
- Cronometro visual durante a execucao.
- Inicio, conclusao e duracao real gravados pelo servidor.
- Atualizacao incremental do SQLite para os novos campos de tempo.
- PDF individual redesenhado com logo e informacoes de evidencia.
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
