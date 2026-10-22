> HISTORICO v0.0.22: este documento descreve o desenho por callback. A arquitetura ativa da v0.0.23 esta em `50-TEAMS-SHAREPOINT-STATUS-V0.0.23.md`. O endpoint de callback permanece apenas para compatibilidade/rollback; o Power Automate atual grava o resultado no SharePoint.

# Teams - fila e callback - v0.0.22

## Objetivo

Evitar rajadas contra o conector Teams e registrar no portal o resultado final por destinatario.

## Contrato enviado pelo portal ao Workflow

O portal envia:

```json
{
  "recipient": "usuario@automind.com.br",
  "text": "...",
  "requestId": "<id-unico>",
  "callbackToken": "<token-unico>",
  "callbackUrl": "http://treinamentos.automind.com.br/api/teams/reminder-result"
}
```

`requestId` e `callbackToken` pertencem a uma unica tentativa e nao devem ser reutilizados.

## Power Automate

Fluxo esperado:

```text
Quando uma solicitacao de webhook do Teams e recebida
  -> Postar mensagem em um chat ou canal
       -> sucesso: POST callback com success=true
       -> falha final: POST callback com success=false
```

### Callback de sucesso

POST para o valor `callbackUrl` recebido no gatilho. Corpo JSON:

```json
{
  "requestId": "<requestId recebido>",
  "callbackToken": "<callbackToken recebido>",
  "success": true,
  "sentAtUtc": "<utcNow()>",
  "workflowRunId": "<workflow run name>",
  "messageId": "<id retornado pela acao Teams>"
}
```

A acao `Postar mensagem em um chat ou canal` retorna `id`, que e o Message ID do Teams.

### Callback de falha

Configurar para executar somente se a acao Teams falhar, expirar ou for ignorada conforme o desenho do fluxo. Corpo JSON:

```json
{
  "requestId": "<requestId recebido>",
  "callbackToken": "<callbackToken recebido>",
  "success": false,
  "workflowRunId": "<workflow run name>",
  "error": "Falha final no envio pelo Teams"
}
```

## Fila no portal

- selecao manual apenas grava na fila;
- um worker despacha uma tentativa por vez;
- intervalo padrao: 13 segundos;
- enquanto aguarda callback: `Enviando`;
- callback de sucesso: `Enviado` + horario;
- callback de falha: `Falhou`;
- uma pessoa com tentativa `queued` ou `processing` nao pode entrar novamente na fila para o mesmo treinamento.

## Observacao de seguranca

O callback usa um token aleatorio exclusivo por tentativa e e idempotente. Quando HTTPS for ativado no portal, `Portal:PublicBaseUrl` deve ser alterado para `https://...` para que o callback tambem use TLS.
