# Teste v0.0.23 - Teams / SharePoint / sincronizacao

## Estado

- `VALIDADO NO AMBIENTE`: Power Automate -> Teams -> SharePoint, caminho de sucesso, em 2026-10-22.
- `PENDENTE DE VALIDACAO`: build .NET 10 no ambiente de empacotamento (SDK indisponivel aqui).
- `PENDENTE DE VALIDACAO`: portal v0.0.23 -> Microsoft Graph -> SharePoint no IIS depois da configuracao do App Registration.
- `PENDENTE DE VALIDACAO`: caminho real de falha/timeout do Teams.

## Validacao de sucesso ja executada

Resultado observado:

```text
Trigger                        OK
Analisar JSON                  OK
Postar mensagem Teams          OK
Criar item Success=Sim         OK
Criar item 1 de falha          Ignorado corretamente
Mensagem recebida no Teams     OK
Registro SharePoint            OK
```

## Teste alvo apos deploy da v0.0.23

Pre-condicoes:

1. v0.0.23 publicada;
2. `Automind__Teams__WebhookUrl` preservado;
3. identidade Graph configurada com leitura no site GSTI-NOC;
4. App Pool iniciado.

Caso principal de recuperacao da tentativa v0.0.22:

1. abrir `Administracao > Pendencias`;
2. localizar o usuario que ficou em `Enviando` na v0.0.22;
3. o portal consulta o SharePoint pelo RequestId existente no SQLite;
4. como o item ja existe com `Success=Yes`, atualizar o SQLite para `sent`;
5. a tela deve exibir `Enviado` e o horario de `SentAtUtc`.

Nao reenviar a mensagem para validar esse caso. O objetivo e reconciliar o envio ja concluido.

## Novo envio controlado

Somente depois da recuperacao acima:

1. selecionar um unico usuario autorizado;
2. enviar uma vez;
3. confirmar `Enviando` durante processamento;
4. confirmar mensagem no Teams;
5. confirmar item SharePoint;
6. atualizar Pendencias;
7. confirmar `Enviado`.

## Falha esperada de Graph

Remover/desabilitar temporariamente a configuracao somente em ambiente controlado:

```text
resultado: tentativa continua Enviando
nenhum novo envio automatico da mesma tentativa
nenhum falso Enviado
```

## Rollback

Se a v0.0.23 apresentar erro de codigo/publicacao, retornar o artifact v0.0.22. Nao apagar o item SharePoint nem o historico SQLite apenas por rollback da aplicacao.
