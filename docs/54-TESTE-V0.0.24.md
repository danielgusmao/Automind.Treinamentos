# Teste v0.0.24 - status Teams via Power Automate

## Evidencia ja validada

`VALIDADO NO AMBIENTE` em 2026-10-23, no servidor onde roda `Automind.Treinamentos`:

- chamada HTTPS de saida para o fluxo `Automind.Treinamentos - Consultar Status Teams`;
- consulta do `RequestId` existente no SharePoint;
- retorno `found=true`;
- `recipient=daniel.gusmao@automind.com.br`;
- `success=true`;
- `sentAtUtc=2026-10-22T20:07:24Z`;
- `workflowRunId` e `messageId` retornados;
- `error` vazio.

A URL HTTP real do fluxo nao foi registrada neste documento.

## Validacoes da aplicacao v0.0.24

### A. Configuracao

Configurar somente no ambiente/IIS:

```text
Automind__TeamsStatus__LookupUrl=<SEGREDO_NO_AMBIENTE>
```

Esperado: o aviso de sincronizacao nao configurada desaparece da tela de Pendencias.

### B. Reconciliacao do envio ja existente

1. Abrir `Administracao -> Pendencias` do treinamento.
2. Nao reenviar Daniel antes de observar o status.
3. Esperado: tentativa existente `processing` e reconciliada pelo RequestId.
4. Esperado na linha de Daniel: `Enviado`.
5. Esperado: `Enviado em` mostra a data/hora correspondente ao `SentAtUtc` devolvido pelo fluxo.
6. Esperado: botao disponivel como `Reenviar Teams`.

### C. Reenvio manual

1. Clicar `Reenviar Teams`.
2. Esperado: nova tentativa entra na fila mesmo que outra ainda esteja `queued/processing`.
3. Esperado: botao continua disponivel.
4. Cada clique autorizado cria `RequestId` diferente.
5. Fila envia uma tentativa por vez respeitando 13 segundos.
6. Apos o Power Automate gravar sucesso no SharePoint, o ultimo sucesso passa a atualizar a data/hora exibida.

### D. Falha

`PENDENTE DE VALIDACAO`: provocar uma falha/timeout controlado da acao Teams e confirmar `Success=Nao -> failed` sem afetar o ultimo sucesso historico.

### E. Build

`PENDENTE DE VALIDACAO`: build .NET 10 no pipeline/agente com SDK instalado. O ambiente de empacotamento nao possui `dotnet`.
