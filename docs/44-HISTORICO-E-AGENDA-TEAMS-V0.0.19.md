# v0.0.19 - Historico, confirmacao e agenda de lembretes Teams

## Objetivo

Evitar reenvio cego para colaboradores que ja receberam tentativa de lembrete, tornar o estado visivel na tela de pendencias e permitir novos lembretes automaticos em intervalos configuraveis.

## Persistencia

Duas tabelas aditivas:

- `TeamsReminderHistory`: uma linha por tentativa, com treinamento, colaborador, horario UTC, origem, status, HTTP, identificadores opcionais do Workflow/Teams e erro resumido.
- `TrainingReminderSchedules`: habilitacao, intervalo, proxima execucao e ultima execucao por treinamento.

A migration e aditiva e registrada como `0.0.19`.

## Recuperacao do lote antigo

Na inicializacao, `Audit-*.jsonl` e lido procurando `action=training-teams-reminder`. Cada linha recebe uma `ImportKey` deterministica (`arquivo + linha`) para tornar a importacao idempotente. Nenhum POST ao Teams ocorre durante essa recuperacao.

Mapeamento historico:

```text
Audit success -> Aceito pelo Workflow
Audit failed  -> Falhou
```

Nao existe evidencia retroativa para transformar um HTTP 2xx antigo em entrega confirmada.

## Estados visuais

```text
Nao avisado          -> sem historico
Aceito pelo Workflow -> HTTP 2xx, sem confirmacao explicita da postagem
Confirmado           -> Workflow devolveu delivered=true ou messageId
Falhou               -> resposta/execucao falhou
Em processamento     -> tentativa reservada ainda sem resultado final
Resultado incerto    -> requisicao interrompida antes de confirmar o resultado
```

A tela mostra o ultimo registro, origem manual/automatica/historica e, quando a ultima tentativa falhou mas existe sucesso anterior, preserva a data do ultimo aviso valido. Datas sao enviadas como UTC e convertidas no navegador para o horario local.

## Contrato de confirmacao do Workflow

O request permanece:

```json
{ "recipient": "usuario@automind.com.br", "text": "mensagem" }
```

Para confirmar a postagem, o Workflow deve executar a acao do Teams antes da acao `Response` e devolver, por exemplo:

```json
{
  "success": true,
  "delivered": true,
  "workflowRunId": "<id-da-execucao>",
  "messageId": "<id-da-mensagem>"
}
```

Em falha controlada pode devolver:

```json
{ "success": false, "status": "failed", "error": "<mensagem sem segredo>" }
```

HTTP 2xx sem `delivered=true`/`messageId` continua sendo apenas `Aceito pelo Workflow`. O sistema nao implementa recibo de leitura.

## Agenda

- desativada por padrao;
- intervalo entre 1 e 365 dias;
- primeiro ciclo = momento da ativacao + intervalo;
- worker verifica a cada 15 minutos;
- somente treinamento publicado e nao historico;
- somente usuario AD elegivel, com e-mail, pendente e fora de `DirectoryExclusions`;
- ultimo aviso manual ou automatico valido entra no calculo;
- concluido deixa de receber automaticamente;
- cada ciclo/usuario recebe `CycleKey` unica para evitar duplicidade apos reinicio.

## Pendencias de ambiente

1. Alterar o Workflow real para responder depois da etapa de Teams com o contrato estruturado.
2. Validar App Pool/IIS para execucao continua do `BackgroundService`.
3. Validar a migration no banco de ambiente com backup existente antes da publicacao.

Esses tres itens sao alteracoes reais e nao foram executados durante a preparacao do pacote.
