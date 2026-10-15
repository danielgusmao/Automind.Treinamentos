# Automind.Treinamentos - documentacao completa do projeto

Estado consolidado em 06/10/2026 - atualizado ate a versão v0.0.14.

## 1. Objetivo

Portal interno de treinamentos obrigatórios para colaboradores Automind, com autenticacao Active Directory, controle administrativo, quiz, aceite, evidência PDF, acompanhamento de pendências e lembretes via Microsoft Teams.

O banco da aplicacao é a fonte de verdade das conclusões. O Active Directory fornece a populacao de colaboradores elegíveis e a identidade usada no login.

## 2. Ambiente

- Servidor IIS: `10.1.2.21` / `SV052022-6121`.
- Site IIS: `Automind.Treinamentos`.
- App Pool: `Automind.Treinamentos`.
- Identidade atual do App Pool: `ApplicationPoolIdentity`.
- URL atual: `http://treinamentos.automind.com.br`.
- Aplicacao interna; o servidor não deve ser exposto diretamente a Internet.
- Runtime alvo: `.NET 10.0 / ASP.NET Core`.
- Banco: SQLite.

## 3. Estrutura persistente no servidor

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|   `-- Automind.Treinamentos.db
|-- Treinamentos\
|-- Evidências\
|   |-- Colaboradores\
|   `-- Relatorios\
|-- Logs\
`-- Backup\
```

Regra: `Web` e substituivel pela Release. Dados persistentes nunca devem ser apagados pelo deploy.

`Storage__RootPath` em producao deve apontar para `C:\Automind.Treinamentos`.

## 4. Autenticacao

Login com usuário e senha do Active Directory `automind.com.br`.

A senha e usada apenas para `ValidateCredentials`; não é persistida, colocada em claims, banco, auditoria ou arquivos.

O cookie autenticado guarda somente identidade basica:

- `sAMAccountName`;
- nome;
- e-mail;
- cargo;
- departamento.

## 5. Autorização administrativa

Grupo oficial: `_treinamentos`.

A partir da v0.0.10 nenhuma role administrativa é persistida no cookie ou sessão.

Em cada requisicao autenticada o backend consulta diretamente o LDAP e verifica membership atual em `_treinamentos`. A verificacao suporta membership direta e grupo aninhado.

A role `TreinamentosAdmin` existe somente em memoria durante a requisicao atual. A policy do `AdminController` e o menu Razor usam a mesma decisão.

Falha de LDAP = sem privilegio administrativo (fail-closed).

Configuracao:

```json
"ActiveDirectory": {
  "Domain": "automind.com.br",
  "BaseDn": "DC=automind,DC=com,DC=br",
  "AdminGroup": "_treinamentos",
  "AuthorizationServer": "",
  "AllowedMailSuffix": "@automind.com.br"
}
```

Opcionalmente pode-se definir no App Pool:

```text
ActiveDirectory__AdminGroup=_treinamentos
ActiveDirectory__AuthorizationServer=<FQDN-do-DC>
```

O segundo e util somente se a infraestrutura decidir fixar um DC para evitar atraso de replicacao percebido durante alterações operacionais do grupo.

## 6. Populacao de colaboradores

Base inicial:

- objeto AD `user`;
- conta habilitada;
- atributo `mail` terminando em `@automind.com.br`.

Isso inclui algumas contas que tecnicamente são usuários/e-mails do AD mas não representam pessoas. Por isso existe uma lista global permanente de exclusões.

## 7. Exclusões permanentes

Tabela/fonte: `DirectoryExclusions` no SQLite persistente.

A exclusão:

- vale para todos os treinamentos atuais e futuros;
- não exclui nem modifica o objeto no AD;
- remove a conta de elegíveis, pendentes e adesão;
- impede lembretes Teams;
- remove treinamentos de `Meus treinamentos`;
- bloqueia acesso direto ao treinamento pelo backend;
- registra categoria, motivo, operador e data;
- pode ser revertida por `Reincluir`;
- preserva histórico de reinclusão.

Conta inicial conhecida: `produtos / produtos@automind.com.br`.

Categorias previstas incluem e-mail geral/caixa compartilhada, conta de serviço, terceiro/nao colaborador e outro.

## 8. Treinamentos

Administradores podem criar e editar treinamentos, definindo título, descrição, versão, tempo estimado, nota mínima, conteúdo e resumo para PDF. `Code` e `Slug` são identificadores técnicos definidos na criação e permanecem estaveis.

Questões podem ser criadas, editadas e excluidas enquanto a versão não possui evidências.

A partir da v0.0.11 uma versão que já possui conclusões é imutável. Qualquer alteração cria uma nova revisão em rascunho e copia as questões, preservando integralmente conclusões, PDFs, hashes e snapshot da versão anterior. Quando a nova revisão e publicada, a versão publicada anterior do mesmo código passa a histórica.

Primeiro treinamento: `Treinamento de Conscientizacao em Segurança da Informação`, código `SI-001`, versão `1.0.0`, estimativa de 8 minutos e nota mínima 5/5.

## 9. Conclusão e evidência

O backend recalcula a pontuacao; não confia no resultado informado pelo navegador.

A conclusão registra:

- colaborador;
- treinamento/versão;
- inicio e conclusão;
- duracao real;
- nota;
- aceite;
- protocolo;
- hash do snapshot;
- caminho e SHA-256 do PDF.

O PDF individual possui dados de evidência e uma segunda página com resumo do treinamento. O campo `SummaryText` pode ser definido pelo administrador; quando ausente, ha fallback para resumo final/descrição/conteúdo.

PDFs já emitidos não são reescritos retroativamente.

## 10. Teams

Reutiliza o mesmo Workflow do projeto CadColab.

Contrato:

```json
{ "recipient": "usuário@automind.com.br", "text": "mensagem" }
```

Segredo somente no ambiente:

```text
Automind__Teams__WebhookUrl
```

Nunca salvar a URL real do Workflow em Git, banco, docs ou logs.

A tela de pendências permite envio individual ou em lote somente para colaboradores pendentes e elegíveis.

## 11. Git, pipeline e Release

Repositorio GitHub:

```text
https://github.com/danielgusmao/Automind.Treinamentos.git
```

Repositorio Azure DevOps:

```text
https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos
```

Branch de deploy: `release`.

Pipeline e Release estao configurados para deploy automatico quando a branch `release` recebe alteração.

As mensagens de commit precisam conter versão no formato `vX.Y.Z`, pois a pipeline valida esse padrao.

Não fazer deploy manual depois de push bem-sucedido na `release`, salvo falha da automacao.

## 12. Versionamento

A versão exibida pela interface vem do assembly. O `.csproj` é a fonte oficial e o build/publish gera `VERSION.txt` com o mesmo valor.

A v0.0.7 demonstrou o risco de alterar apenas `VERSION.txt`; a UI continuou exibindo o assembly anterior. Isso foi corrigido definitivamente a partir da v0.0.8.

## 13. Segurança

- Senha AD não persistida.
- Role administrativa não persistida desde v0.0.10.
- Autorização administrativa fail-closed.
- Validação server-side do quiz e acesso.
- Webhook Teams fora do repositorio.
- Dados persistentes fora da pasta publicada.
- App Pool deve ter escrita somente nas pastas persistentes necessarias.
- Não usar `iisreset` para operações normais; atuar apenas no App Pool/site do projeto.

## 14. HTTPS/TLS - estado e decisão pendente

O site continua em HTTP porque ainda não ha certificado configurado.

O servidor e interno e não será exposto diretamente a Internet. Portanto a proxima fase deve avaliar HTTPS interno sem abrir o servidor para validação HTTP publica.

Opções a avaliar antes de instalar software:

1. certificado emitido por CA interna/AD CS, se existir PKI corporativa;
2. ACME com validação DNS-01, caso o DNS publico permita automacao segura;
3. certificado corporativo/importado manualmente com processo de renovacao controlado.

`win-acme` não deve ser instalado antes de validar qual metodo de emissão e renovacao se encaixa na infraestrutura. HTTP-01 não e a primeira escolha porque exigiria alcance externo na porta 80, contrario ao requisito de servidor interno.

## 15. Incidentes/achados importantes

- Falha inicial SQLite readonly e `manifest.json` sem permissão: origem em ACLs da pasta publicada; resolvida ao separar dados persistentes e corrigir permissoes.
- v0.0.7 não apareceu na UI: assembly permaneceu 0.0.6; corrigido na v0.0.8.
- v0.0.9 removeu role antiga do cookie, mas `GetAuthorizationGroups()` não refletiu de forma confiavel uma inclusão recente no grupo durante sessão aberta; substituido por LDAP direto na v0.0.10.
- Pipeline falhou em commit sem `vX.Y.Z`; manter versão em toda mensagem que deve disparar build/release.

## 16. Regra de documentacao

`docs/00-CHECKPOINT.md` é a fonte cumulativa oficial de retomada.

Toda alteração relevante deve:

1. adicionar a entrada mais recente no topo do checkpoint;
2. atualizar `CHANGELOG.md`;
3. atualizar `CONTEXTO-ATUAL.md` quando mudar o estado vigente;
4. adicionar documento tematico/teste quando a mudanca justificar;
5. preservar decisoes e histórico necessarios para rollback e continuidade.

## 17. Proximos passos

1. Validar v0.0.10 com inclusão/remocao de usuário em `_treinamentos` sem logout.
2. Se houver atraso por replicacao entre DCs, identificar o DC operacional e configurar `ActiveDirectory__AuthorizationServer` sem alterar código.
3. Retomar estudo de HTTPS/TLS interno e executar apenas testes de conectividade/PKI antes de instalar win-acme ou outro agente.
4. Continuar populando a lista permanente de contas que não representam pessoas.

## 14. Publicacao administrativa

A partir da v0.0.13, publicacao e despublicacao usam endpoints separados: `PublishTraining` e `UnpublishTraining`. Não éxiste mais toggle booleano no formulario.

Ao publicar, o sistema valida questões e nota mínima, grava o snapshot em `C:\Automind.Treinamentos\Treinamentos\<slug-ou-familySlug>\<versão>` e somente depois marca a versão como publicada. Se o snapshot falhar, a publicacao não e concluida.

A exclusão de treinamento de teste continua sendo uma operacao manual e não existe botao de exclusão no portal.

### Convencao de nome dos PDFs - v0.0.14

A estrutura definitiva permanece por colaborador: `C:\Automind.Treinamentos\Evidências\Colaboradores\<login>`. A proposta anterior de separar fisicamente por treinamento/versão foi cancelada. Novos PDFs usam `<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf`. A v0.0.14 tambem migra evidências existentes para o caminho curto quando o arquivo e localizado e atualiza `TrainingCompletions.EvidencePdfPath` sem alterar o conteúdo/hash do PDF.
