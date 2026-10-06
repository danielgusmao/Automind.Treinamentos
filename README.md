# Automind.Treinamentos

Versao: **v0.0.4**

Aplicacao interna ASP.NET Core MVC para treinamentos, quiz, aceite e evidencias em PDF.

## Destaques da v0.0.4

- edição de perguntas já cadastradas;
- seleção de pendentes, `Marcar todos` e envio Teams individual/em lote;
- mensagem Teams inclui link direto do treinamento;
- usuários concluídos não exibem seleção nem ação de lembrete;
- novo treinamento abre com modelo de conteúdo e exemplo de questão;
- `_informatica` deixou de aparecer na interface, sem alterar a autorização AD;
- topbar/rodapé exibem a versão do assembly;
- aviso visual de HTTP removido do login e do rodapé;
- treinamento continua integrado ao mesmo layout/topbar do portal;
- identidade visual Automind baseada no Cadastro de Colaboradores;
- PDF individual e consolidado mantidos.

### Teams

A integração usa Teams Workflows/Power Automate no contrato `{ recipient, text }`. A URL do webhook deve ser HTTPS, é segredo externo e deve ser fornecida por `Automind__Teams__WebhookUrl`. Não versionar essa URL.

## Abrir no Visual Studio

1. Extraia o pacote.
2. Abra `Automind.Treinamentos.sln`.
3. Restaure os pacotes NuGet.
4. Compile.
5. Execute o perfil `http`.
6. Acesse `http://localhost:5180`.

## Dependencias principais

- .NET 10.0 / ASP.NET Core;
- `Microsoft.Data.Sqlite 10.0.12`;
- `System.DirectoryServices 10.0.12`;
- `System.DirectoryServices.AccountManagement 10.0.12`.

## Atencao - HTTP

A aplicação continua sem HTTPS porque o certificado ainda nao esta disponivel. O formulario utiliza credenciais reais do AD, portanto **nao use em rede nao confiavel**. Para os testes, prefira localhost/ambiente interno controlado e conta de teste.

## Documentacao

Comece por `docs/00-CHECKPOINT.md`.
