# v0.0.5 - Reuso do Workflow Teams do CadColab

## Referencia adotada

O `Automind.Treinamentos` passa a seguir o mesmo padrao usado pelo ultimo `Automind.CadastroColaboradores` fornecido:

- configuracao externa `Automind__Teams__WebhookUrl`;
- `Automind:Teams:Enabled=true`;
- webhook fora do Git, banco, logs e documentacao;
- `HttpClient` nomeado `TeamsWebhook` com timeout de 15 segundos;
- POST JSON no contrato `{ recipient, text }`;
- nenhum segredo incluido no payload de auditoria.

## Mensagem aprovada

A mensagem e personalizada com o primeiro nome:

```text
Olá, Daniel. Você possui o treinamento Treinamento de Conscientização em Segurança da Informação pendente.
Tempo estimado: 8 minutos.
Acessar treinamento: http://treinamentos.automind.com.br/Training/Start/1
```

No payload real o titulo e o rotulo do link usam HTML `<strong>` e o URL usa `<a href>` para manter clicabilidade no Workflow ja existente.

## Link

`Portal:PublicBaseUrl` define a origem publica:

```text
http://treinamentos.automind.com.br
```

O link final sempre segue:

```text
/Training/Start/{TrainingId}
```

O sistema ganhou essa rota e os botoes de inicio tambem usam `Start`.

## Habilitacao no IIS

O App Pool `Automind.Treinamentos` deve receber o mesmo valor da variavel `Automind__Teams__WebhookUrl` ja existente no App Pool `CadastroColaboradores`.

O script de preparo copia esse valor em memoria entre as configuracoes do IIS e nao imprime nem grava o segredo em arquivo.
