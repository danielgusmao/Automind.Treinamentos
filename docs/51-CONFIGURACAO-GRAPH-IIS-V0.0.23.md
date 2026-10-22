# v0.0.23 - Configuracao Microsoft Graph / SharePoint / IIS

## Objetivo

Dar ao processo `Automind.Treinamentos` permissao somente de leitura para consultar os resultados Teams gravados no SharePoint Online.

Nao ha callback da nuvem para o portal. A aplicacao faz uma chamada HTTPS de saida para Microsoft Graph.

## Caminhos administrativos

Entra / identidade da aplicacao:

```text
Microsoft Entra admin center
  -> Identity
  -> Applications
  -> App registrations
  -> New registration
  -> Automind.Treinamentos.SharePointStatus
```

Permissao Graph:

```text
App registration
  -> API permissions
  -> Add a permission
  -> Microsoft Graph
  -> Application permissions
  -> Sites.Selected
  -> Grant admin consent
```

Credencial:

```text
App registration
  -> Certificates & secrets
  -> Client secrets
  -> New client secret
```

O valor do segredo e copiado uma unica vez para o mecanismo seguro do ambiente/Release e nao deve entrar no repositorio.

SharePoint:

```text
https://automindautomacao.sharepoint.com/sites/GSTI-NOC
  -> Site contents
  -> AutomindTreinamentosTeamsStatus
```

Lista direta:

```text
https://automindautomacao.sharepoint.com/sites/GSTI-NOC/Lists/AutomindTreinamentosTeamsStatus/AllItems.aspx
```

Power Automate:

```text
Treinamento Automind
Flow ID: b637f2a0-9f0d-4a0b-b63a-4f5af169fd18
```

IIS / processo do portal:

```text
Servidor da aplicacao
  -> IIS
  -> Application Pools
  -> Automind.Treinamentos
  -> configuracao do processo / environmentVariables
```

Arquivo de configuracao do IIS que materializa configuracoes de App Pool no servidor:

```text
%windir%\System32\inetsrv\config\applicationHost.config
```

Nao editar esse arquivo manualmente por conveniencia. Preferir IIS/AppCmd/Release conforme o padrao operacional do servidor, mantendo os segredos fora do artifact.

## Registro de aplicativo Entra

Criar um App Registration dedicado, por exemplo:

```text
Automind.Treinamentos.SharePointStatus
```

Tipo recomendado:

```text
Single tenant
```

Permissao Microsoft Graph de aplicativo recomendada:

```text
Sites.Selected
```

Conceder admin consent.

Depois, conceder ao aplicativo somente `read` no site:

```text
https://automindautomacao.sharepoint.com/sites/GSTI-NOC
```

Nao conceder acesso a todos os sites se nao houver necessidade.

## Concessao do site selecionado

Resolver o Site ID:

```http
GET https://graph.microsoft.com/v1.0/sites/automindautomacao.sharepoint.com:/sites/GSTI-NOC?$select=id
```

Com uma identidade administrativa autorizada a conceder a permissao, criar a permissao:

```http
POST https://graph.microsoft.com/v1.0/sites/{SITE_ID}/permissions
Content-Type: application/json

{
  "roles": ["read"],
  "grantedToIdentities": [
    {
      "application": {
        "id": "<CLIENT_ID>",
        "displayName": "Automind.Treinamentos.SharePointStatus"
      }
    }
  ]
}
```

Essa etapa e administrativa e deve ser executada somente por pessoa autorizada.

## Credencial

A v0.0.23 suporta client credentials com ClientSecret.

O segredo deve existir somente no ambiente do IIS/segredo da Release. Nao gravar em:

- Git;
- appsettings publicado;
- docs;
- logs;
- checkpoint;
- comandos versionados.

Quando houver processo corporativo para certificado, a autenticacao pode ser endurecida em uma versao posterior sem alterar a lista SharePoint.

## Variaveis de ambiente do App Pool

App Pool:

```text
Automind.Treinamentos
```

Variaveis obrigatorias:

```text
Automind__TeamsStatus__Enabled=true
Automind__TeamsStatus__TenantId=<TENANT_ID>
Automind__TeamsStatus__ClientId=<CLIENT_ID>
Automind__TeamsStatus__ClientSecret=<SEGREDO_NO_AMBIENTE>
```

Variaveis com padrao ja configurado no pacote:

```text
Automind__TeamsStatus__SiteHostname=automindautomacao.sharepoint.com
Automind__TeamsStatus__SitePath=/sites/GSTI-NOC
Automind__TeamsStatus__ListName=AutomindTreinamentosTeamsStatus
Automind__TeamsStatus__SyncIntervalSeconds=15
Automind__TeamsStatus__BatchSize=50
```

Webhook existente continua separado:

```text
Automind__Teams__WebhookUrl=<SEGREDO_NO_AMBIENTE>
```

## Caminho no IIS

A configuracao fica no App Pool, em variaveis de ambiente do processo. Nao precisa alterar binding, DNS, firewall de entrada ou publicar o portal externamente.

Binding atual preservado:

```text
http 10.1.2.21:80:treinamentos.automind.com.br
```

HTTPS do portal continua fora deste escopo ate existir certificado.

## Fluxo OAuth usado pelo codigo

```text
POST https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token
scope=https://graph.microsoft.com/.default
grant_type=client_credentials
```

O token e mantido somente em memoria e renovado antes de expirar.

Depois o portal resolve e consulta:

```text
GET /v1.0/sites/automindautomacao.sharepoint.com:/sites/GSTI-NOC?$select=id
GET /v1.0/sites/{siteId}/lists/AutomindTreinamentosTeamsStatus?$select=id,displayName
GET /v1.0/sites/{siteId}/lists/{listId}/items?...&$filter=fields/Title eq '<RequestId>'
```

Campos lidos:

```text
Title
Recipient
Success
SentAtUtc
WorkflowRunId
MessageId
Error
```

O portal nao grava nem apaga item SharePoint.

## Comportamento sem configuracao

Se TenantId/ClientId/ClientSecret nao estiverem configurados:

- envio Teams continua funcionando;
- SharePoint continua recebendo resultados pelo Power Automate;
- portal nao consegue reconciliar o resultado;
- UI pode permanecer `Enviando`;
- tela administrativa mostra aviso de sincronizacao nao configurada;
- anti-duplicidade continua bloqueando reenvio da tentativa ativa.

## Rollback

Rollback de codigo:

```text
retornar para v0.0.22
```

Rollback de configuracao:

- remover/desabilitar `Automind__TeamsStatus__Enabled`;
- revogar a permissao Sites.Selected do aplicativo no site;
- remover ClientSecret da configuracao do App Pool.

A lista SharePoint pode permanecer; nao e necessario excluir historico para rollback.

## Referencias Microsoft

- OAuth client credentials: `https://learn.microsoft.com/entra/identity-platform/v2-oauth2-client-creds-grant-flow`
- Selected permissions: `https://learn.microsoft.com/graph/permissions-selected-overview`
- SharePoint sites no Graph: `https://learn.microsoft.com/graph/api/resources/sharepoint`
- List items no Graph: `https://learn.microsoft.com/graph/api/listitem-list`
- Sites.Selected / site permissions: `https://learn.microsoft.com/sharepoint/dev/sp-add-ins-modernize/understanding-rsc-for-msgraph-and-sharepoint-online`
