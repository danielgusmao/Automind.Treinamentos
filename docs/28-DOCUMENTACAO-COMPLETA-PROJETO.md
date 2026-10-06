# Automind.Treinamentos - documentacao completa do projeto

Estado consolidado em 06/10/2026 - atualizado ate a versao v0.0.14.

## 1. Objetivo

Portal interno de treinamentos obrigatorios para colaboradores Automind, com autenticacao Active Directory, controle administrativo, quiz, aceite, evidencia PDF, acompanhamento de pendencias e lembretes via Microsoft Teams.

O banco da aplicacao e a fonte de verdade das conclusoes. O Active Directory fornece a populacao de colaboradores elegiveis e a identidade usada no login.

## 2. Ambiente

- Servidor IIS: `10.1.2.21` / `SV052022-6121`.
- Site IIS: `Automind.Treinamentos`.
- App Pool: `Automind.Treinamentos`.
- Identidade atual do App Pool: `ApplicationPoolIdentity`.
- URL atual: `http://treinamentos.automind.com.br`.
- Aplicacao interna; o servidor nao deve ser exposto diretamente a Internet.
- Runtime alvo: `.NET 10.0 / ASP.NET Core`.
- Banco: SQLite.

## 3. Estrutura persistente no servidor

```text
C:\Automind.Treinamentos\
|-- Web\
|-- Data\
|   `-- Automind.Treinamentos.db
|-- Treinamentos\
|-- Evidencias\
|   |-- Colaboradores\
|   `-- Relatorios\
|-- Logs\
`-- Backup\
```

Regra: `Web` e substituivel pela Release. Dados persistentes nunca devem ser apagados pelo deploy.

`Storage__RootPath` em producao deve apontar para `C:\Automind.Treinamentos`.

## 4. Autenticacao

Login com usuario e senha do Active Directory `automind.com.br`.

A senha e usada apenas para `ValidateCredentials`; nao e persistida, colocada em claims, banco, auditoria ou arquivos.

O cookie autenticado guarda somente identidade basica:

- `sAMAccountName`;
- nome;
- e-mail;
- cargo;
- departamento.

## 5. Autorizacao administrativa

Grupo oficial: `_treinamentos`.

A partir da v0.0.10 nenhuma role administrativa e persistida no cookie ou sessao.

Em cada requisicao autenticada o backend consulta diretamente o LDAP e verifica membership atual em `_treinamentos`. A verificacao suporta membership direta e grupo aninhado.

A role `TreinamentosAdmin` existe somente em memoria durante a requisicao atual. A policy do `AdminController` e o menu Razor usam a mesma decisao.

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

O segundo e util somente se a infraestrutura decidir fixar um DC para evitar atraso de replicacao percebido durante alteracoes operacionais do grupo.

## 6. Populacao de colaboradores

Base inicial:

- objeto AD `user`;
- conta habilitada;
- atributo `mail` terminando em `@automind.com.br`.

Isso inclui algumas contas que tecnicamente sao usuarios/e-mails do AD mas nao representam pessoas. Por isso existe uma lista global permanente de exclusoes.

## 7. Exclusoes permanentes

Tabela/fonte: `DirectoryExclusions` no SQLite persistente.

A exclusao:

- vale para todos os treinamentos atuais e futuros;
- nao exclui nem modifica o objeto no AD;
- remove a conta de elegiveis, pendentes e adesao;
- impede lembretes Teams;
- remove treinamentos de `Meus treinamentos`;
- bloqueia acesso direto ao treinamento pelo backend;
- registra categoria, motivo, operador e data;
- pode ser revertida por `Reincluir`;
- preserva historico de reinclusao.

Conta inicial conhecida: `produtos / produtos@automind.com.br`.

Categorias previstas incluem e-mail geral/caixa compartilhada, conta de servico, terceiro/nao colaborador e outro.

## 8. Treinamentos

Administradores podem criar e editar treinamentos, definindo titulo, descricao, versao, tempo estimado, nota minima, conteudo e resumo para PDF. `Code` e `Slug` sao identificadores tecnicos definidos na criacao e permanecem estaveis.

Questoes podem ser criadas, editadas e excluidas enquanto a versao nao possui evidencias.

A partir da v0.0.11 uma versao que ja possui conclusoes e imutavel. Qualquer alteracao cria uma nova revisao em rascunho e copia as questoes, preservando integralmente conclusoes, PDFs, hashes e snapshot da versao anterior. Quando a nova revisao e publicada, a versao publicada anterior do mesmo codigo passa a historica.

Primeiro treinamento: `Treinamento de Conscientizacao em Seguranca da Informacao`, codigo `SI-001`, versao `1.0.0`, estimativa de 8 minutos e nota minima 5/5.

## 9. Conclusao e evidencia

O backend recalcula a pontuacao; nao confia no resultado informado pelo navegador.

A conclusao registra:

- colaborador;
- treinamento/versao;
- inicio e conclusao;
- duracao real;
- nota;
- aceite;
- protocolo;
- hash do snapshot;
- caminho e SHA-256 do PDF.

O PDF individual possui dados de evidencia e uma segunda pagina com resumo do treinamento. O campo `SummaryText` pode ser definido pelo administrador; quando ausente, ha fallback para resumo final/descricao/conteudo.

PDFs ja emitidos nao sao reescritos retroativamente.

## 10. Teams

Reutiliza o mesmo Workflow do projeto CadColab.

Contrato:

```json
{ "recipient": "usuario@automind.com.br", "text": "mensagem" }
```

Segredo somente no ambiente:

```text
Automind__Teams__WebhookUrl
```

Nunca salvar a URL real do Workflow em Git, banco, docs ou logs.

A tela de pendencias permite envio individual ou em lote somente para colaboradores pendentes e elegiveis.

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

Pipeline e Release estao configurados para deploy automatico quando a branch `release` recebe alteracao.

As mensagens de commit precisam conter versao no formato `vX.Y.Z`, pois a pipeline valida esse padrao.

Nao fazer deploy manual depois de push bem-sucedido na `release`, salvo falha da automacao.

## 12. Versionamento

A versao exibida pela interface vem do assembly. O `.csproj` e a fonte oficial e o build/publish gera `VERSION.txt` com o mesmo valor.

A v0.0.7 demonstrou o risco de alterar apenas `VERSION.txt`; a UI continuou exibindo o assembly anterior. Isso foi corrigido definitivamente a partir da v0.0.8.

## 13. Seguranca

- Senha AD nao persistida.
- Role administrativa nao persistida desde v0.0.10.
- Autorizacao administrativa fail-closed.
- Validacao server-side do quiz e acesso.
- Webhook Teams fora do repositorio.
- Dados persistentes fora da pasta publicada.
- App Pool deve ter escrita somente nas pastas persistentes necessarias.
- Nao usar `iisreset` para operacoes normais; atuar apenas no App Pool/site do projeto.

## 14. HTTPS/TLS - estado e decisao pendente

O site continua em HTTP porque ainda nao ha certificado configurado.

O servidor e interno e nao sera exposto diretamente a Internet. Portanto a proxima fase deve avaliar HTTPS interno sem abrir o servidor para validacao HTTP publica.

Opcoes a avaliar antes de instalar software:

1. certificado emitido por CA interna/AD CS, se existir PKI corporativa;
2. ACME com validacao DNS-01, caso o DNS publico permita automacao segura;
3. certificado corporativo/importado manualmente com processo de renovacao controlado.

`win-acme` nao deve ser instalado antes de validar qual metodo de emissao e renovacao se encaixa na infraestrutura. HTTP-01 nao e a primeira escolha porque exigiria alcance externo na porta 80, contrario ao requisito de servidor interno.

## 15. Incidentes/achados importantes

- Falha inicial SQLite readonly e `manifest.json` sem permissao: origem em ACLs da pasta publicada; resolvida ao separar dados persistentes e corrigir permissoes.
- v0.0.7 nao apareceu na UI: assembly permaneceu 0.0.6; corrigido na v0.0.8.
- v0.0.9 removeu role antiga do cookie, mas `GetAuthorizationGroups()` nao refletiu de forma confiavel uma inclusao recente no grupo durante sessao aberta; substituido por LDAP direto na v0.0.10.
- Pipeline falhou em commit sem `vX.Y.Z`; manter versao em toda mensagem que deve disparar build/release.

## 16. Regra de documentacao

`docs/00-CHECKPOINT.md` e a fonte cumulativa oficial de retomada.

Toda alteracao relevante deve:

1. adicionar a entrada mais recente no topo do checkpoint;
2. atualizar `CHANGELOG.md`;
3. atualizar `CONTEXTO-ATUAL.md` quando mudar o estado vigente;
4. adicionar documento tematico/teste quando a mudanca justificar;
5. preservar decisoes e historico necessarios para rollback e continuidade.

## 17. Proximos passos

1. Validar v0.0.10 com inclusao/remocao de usuario em `_treinamentos` sem logout.
2. Se houver atraso por replicacao entre DCs, identificar o DC operacional e configurar `ActiveDirectory__AuthorizationServer` sem alterar codigo.
3. Retomar estudo de HTTPS/TLS interno e executar apenas testes de conectividade/PKI antes de instalar win-acme ou outro agente.
4. Continuar populando a lista permanente de contas que nao representam pessoas.

## 14. Publicacao administrativa

A partir da v0.0.13, publicacao e despublicacao usam endpoints separados: `PublishTraining` e `UnpublishTraining`. Nao existe mais toggle booleano no formulario.

Ao publicar, o sistema valida questoes e nota minima, grava o snapshot em `C:\Automind.Treinamentos\Treinamentos\<slug-ou-familySlug>\<versao>` e somente depois marca a versao como publicada. Se o snapshot falhar, a publicacao nao e concluida.

A exclusao de treinamento de teste continua sendo uma operacao manual e nao existe botao de exclusao no portal.

### Convencao de nome dos PDFs - v0.0.14

A estrutura definitiva permanece por colaborador: `C:\Automind.Treinamentos\Evidencias\Colaboradores\<login>`. A proposta anterior de separar fisicamente por treinamento/versao foi cancelada. Novos PDFs usam `<Codigo>_v<Versao>_<AAAAMMDD>_<Protocolo>.pdf`. A v0.0.14 tambem migra evidencias existentes para o caminho curto quando o arquivo e localizado e atualiza `TrainingCompletions.EvidencePdfPath` sem alterar o conteudo/hash do PDF.
