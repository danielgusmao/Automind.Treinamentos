# Automind.Treinamentos

Versão: **v0.0.18**

Aplicacao interna ASP.NET Core MVC para treinamentos, quiz, aceite, evidências PDF, acompanhamento de pendências no Active Directory e comunicacao via Teams.

## Destaques da v0.0.18
- refinamento exclusivamente visual sobre a v0.0.16, sem alteração de regra de negócio;
- textos de destaque que estavam em rosa claro passam a usar o magenta institucional `#fa094e`;
- tela de login preserva o fundo aprovado, mas remove as linhas/faixas neon inclinadas;
- banners/hero removem as barras degradê decorativas do fundo;
- símbolo Automind passa a ser o único elemento gráfico do hero, ampliado para preencher melhor a área visual;
- treinamento dedicado de Segurança da Informação recebe o mesmo refinamento visual;
- versão de assembly, `VERSION.txt` e documentação atualizados para v0.0.18.

## Base visual preservada da v0.0.16
- composição aprovada da opção 2;
- gradiente institucional `#fa094e -> #4d1056` nos componentes de destaque;
- botões, cards, chips, destaques e blocos visuais permanecem harmonizados;
- nenhuma regra de negócio foi alterada.


- hardening de login e sessão AD;
- rate limit no login;
- validação server-side reforcada do quiz;
- snapshots validados/atomicos;
- integridade SQLite reforcada;
- auditoria diaria com correlation id;
- health check `/health`;
- consultas AD/SQLite otimizadas;
- menor privilegio na pasta Backup;
- limpeza de código legado sem uso;
- HTTPS continua pendente ate existir certificado.


- administração autorizada exclusivamente pelo grupo AD `_treinamentos`;
- role administrativa não é persistida em cookie nem em sessão;
- membership administrativa e consultada diretamente no LDAP a cada requisicao autenticada;
- consulta usa o grupo real no AD e suporta membership direta/aninhada;
- cookies antigos com `TreinamentosAdmin` ou `Informatica` não mantem acesso administrativo indevido;
- a exclusão de contas deixa de ser por treinamento e passa a ser **permanente e global**;
- nova página administrativa **Exclusões**, acessivel pelo topo e pelo painel;
- lista mostra nome, login, e-mail, tipo da conta, motivo, operador e data;
- uma exclusão ativa vale para todos os treinamentos atuais e futuros;
- exclusões não entram em elegíveis, pendentes, adesão nem lembretes Teams;
- conta excluida não recebe treinamento em `Meus treinamentos` e o link direto e bloqueado no backend;
- `Reincluir` restaura a conta para todos os treinamentos;
- histórico de reinclusoes fica preservado para auditoria;
- exclusões antigas da v0.0.6/v0.0.7 são migradas automaticamente para a lista global;
- `produtos / produtos@automind.com.br` entra como exclusão inicial conhecida, classificada como e-mail geral;
- nenhuma exclusão altera ou apaga objetos no Active Directory;
- PDF individual continua com a segunda página de resumo introduzida na v0.0.7;
- novas evidências individuais usam nome curto com código do treinamento, versão, data e protocolo;
- a pasta de cada colaborador usa somente o login AD (`sAMAccountName`) para reduzir o comprimento do caminho;
- evidências existentes são migradas para o caminho curto sem alterar o conteúdo/hash do PDF;
- estrutura persistente separada entre `Web`, `Data`, `Treinamentos`, `Evidências`, `Logs` e `Backup` continua mantida;
- Teams continua reutilizando o mesmo Workflow do CadColab por `Automind__Teams__WebhookUrl`.

## Estrutura de producao

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|-- Treinamentos\
|-- Evidências\
|   |-- Colaboradores\
|   `-- Relatorios\
|-- Logs\
`-- Backup\
```

`Web` e descartavel/substituivel pelo Release. Banco, evidências, snapshots, logs e backups ficam fora da pasta publicada.

## Lista permanente de exclusões

A fonte inicial de colaboradores continua sendo o AD: usuários habilitados com e-mail `@automind.com.br`.

Antes de calcular indicadores ou enviar lembretes, o sistema remove as contas presentes em `DirectoryExclusions`.

Exemplos de classificacao:

- `E-mail geral / Caixa compartilhada`;
- `Conta de serviço`;
- `Terceiro / Não colaborador`;
- `Outro`.

A lista e gerenciada em `Administração > Exclusões`.

## Teams

A integracao usa o mesmo Workflow/contrato do CadColab:

```json
{ "recipient": "usuário@automind.com.br", "text": "mensagem" }
```

A URL real do Workflow não esta no projeto. O IIS fornece:

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

Push na `release` deve acionar a pipeline/Release configurada pelo projeto. Não fazer deploy manual apos push bem-sucedido, salvo falha da automacao.

## Build

1. Abra `Automind.Treinamentos.sln`.
2. Restore dos pacotes NuGet.
3. Build em Release.
4. Commit e push na branch `release` para GitHub e Azure.

## Dependências principais

- .NET 10.0 / ASP.NET Core;
- `Microsoft.Data.Sqlite 10.0.12`;
- `System.DirectoryServices 10.0.12`;
- `System.DirectoryServices.AccountManagement 10.0.12`.

## Documentacao

Comece por `docs/00-CHECKPOINT.md`.

- PDF com resumo: `docs/20-PDF-RESUMO-V0.0.7.md`;
- exclusões permanentes: `docs/22-EXCLUSOES-PERMANENTES-V0.0.8.md`;
- exclusões v0.0.8: `docs/23-TESTE-V0.0.8.md`;
- histórico da tentativa v0.0.9: `docs/24-AUTORIZACAO-ADMIN-V0.0.9.md`;
- autorização administrativa atual: `docs/26-AUTORIZACAO-ADMIN-TEMPO-REAL-V0.0.10.md`;
- teste de autorização v0.0.10: `docs/27-TESTE-V0.0.10.md`;
- consolidacao completa do projeto: `docs/28-DOCUMENTACAO-COMPLETA-PROJETO.md`;
- edição/versionamento v0.0.11: `docs/29-EDICAO-E-VERSIONAMENTO-TREINAMENTOS-V0.0.11.md`;
- teste v0.0.11: `docs/30-TESTE-V0.0.11.md`;
- publicacao explicita v0.0.13: `docs/32-CORRECAO-PUBLICACAO-EXPLICITA-V0.0.13.md`;
- teste v0.0.13: `docs/33-TESTE-V0.0.13.md`.
- nome dos PDFs v0.0.14: `docs/34-NOME-PDF-EVIDENCIAS-V0.0.14.md`;
- teste v0.0.14: `docs/35-TESTE-V0.0.14.md`.
- hardening/otimizações v0.0.15: `docs/37-HARDENING-E-OTIMIZACOES-V0.0.15.md`;
- teste v0.0.15: `docs/38-TESTE-V0.0.15.md`;
- status da auditoria: `docs/39-STATUS-AUDITORIA-POS-V0.0.15.md`;
- revisão visual global v0.0.16: `docs/40-REVISAO-VISUAL-GRADIENTE-V0.0.16.md`;
- refinamento visual v0.0.17: `docs/41-REFINO-VISUAL-V0.0.17.md`;
- validação v0.0.17: `docs/42-TESTE-V0.0.17.md`;
- revisão ortográfica e ajuste final de gradiente v0.0.18: `docs/43-REVISAO-ORTOGRAFIA-V0.0.18.md`.


## Autorização administrativa

Administração autorizada exclusivamente pelo grupo AD `_treinamentos`, com consulta LDAP direta a cada requisicao autenticada e sem persistencia de role administrativa no cookie.


## Edição e versionamento

A partir da v0.0.11 o catálogo permite editar treinamentos. Se a versão ainda não possui conclusões, a alteração e direta. Se já existem evidências, o sistema preserva a versão anterior e cria uma nova revisão em rascunho com as questões copiadas.

## Publicacao de treinamentos

A partir da v0.0.13, publicar e despublicar são operações POST distintas. O sistema não usa mais um parametro booleano de toggle para decidir a operacao. Ao publicar, o snapshot técnico e criado em `Treinamentos/<slug-ou-familySlug>/<versão>` antes de marcar o treinamento como publicado.

## Nome dos arquivos de evidência

A partir da v0.0.14, novos PDFs individuais seguem:

```text
<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf
```

As evidências continuam centralizadas por colaborador, mas a pasta usa somente o login AD: `Evidências\Colaboradores\<login>`. Evidências existentes registradas no banco são migradas para o caminho curto quando o arquivo e localizado, com atualizacao de `EvidencePdfPath` e sem alterar o hash do PDF.
