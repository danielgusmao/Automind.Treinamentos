# v0.0.20 - Status Teams simples e responsividade

## Decisao aprovada

O controle visual do Teams usa somente tres estados:

```text
Enviado     -> Workflow confirmou a etapa de envio ao Teams
Falhou      -> envio/confirmacao falhou
Nao enviado -> nenhum envio confirmado/falha registrada desde o inicio da v0.0.20
```

Nao existe controle de leitura da mensagem.

## Fonte de verdade

O SQLite, tabela `TeamsReminderHistory`, e a fonte oficial do status exibido. `Audit-*.jsonl` continua como auditoria tecnica e nao e importado para reconstruir estado.

## Inicio do zero

Na primeira inicializacao com migration `0.0.20`, `TeamsReminderHistory` e limpo. Isso implementa a decisao de ignorar envios anteriores. A agenda por treinamento nao e apagada.

## Confirmacao

O request do portal permanece `{ recipient, text }`. Para gravar `Enviado`, o Workflow deve responder depois da acao Teams com `delivered=true` ou `messageId`. HTTP 2xx sem confirmacao e tratado como `Falhou`.

## Tela Pendencias

- mostra status Teams e horario do ultimo envio confirmado;
- remove textos tecnicos `Aceito pelo Workflow`/`Confirmado`;
- remove a largura fixa de 1540 px;
- mantem `Ação` visivel na direita em desktop;
- esconde colunas secundarias conforme a largura;
- transforma cada linha em cartao no mobile.

## Agenda

Agenda, intervalo configuravel, elegibilidade AD, exclusoes, conclusoes e protecao por `CycleKey` permanecem inalterados. Somente envio `Sent` participa do calculo de ultimo aviso valido.
