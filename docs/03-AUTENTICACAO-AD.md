# Autenticacao e Active Directory

## Login

O formulario recebe `usuario` e `senha` e valida diretamente contra `automind.com.br` usando `PrincipalContext.ValidateCredentials` com `ContextOptions.Negotiate`.

Apos sucesso, a aplicacao cria cookie proprio contendo somente atributos nao secretos: `sAMAccountName`, nome, e-mail, cargo, departamento e a role administrativa quando aplicavel. A senha nao e armazenada.

## Administracao

O grupo administrativo do projeto e `_treinamentos`. A role interna e `TreinamentosAdmin`.

A partir da v0.0.9, a autorizacao nao depende apenas da role gravada no login: a cada requisicao autenticada o backend revalida no Active Directory a participacao atual do usuario no grupo configurado. Remocao ou inclusao no grupo passa a refletir na sessao sem aguardar as 8 horas do cookie.

Falha de resolucao/consulta do grupo nao concede acesso administrativo. Claims legadas `Informatica` nao autorizam mais a area administrativa.

## Pendentes

Filtro LDAP: conta habilitada com `mail` terminando em `@automind.com.br`, menos a lista global permanente de exclusoes de contas que nao representam pessoas.

## Transporte

O ambiente ainda esta em HTTP interno. HTTPS/TLS permanece pendente e deve ser tratado antes de ampliar o uso, pois usuario e senha de AD trafegam no formulario de login.


## Atualizacao v0.0.10 - autorizacao administrativa

A role administrativa nao e mais gravada no cookie. Cada requisicao autenticada consulta diretamente o LDAP para verificar membership atual no grupo `_treinamentos`, usando `memberOf:1.2.840.113556.1.4.1941` e fail-closed em erro. A v0.0.9, baseada em `GetAuthorizationGroups()`, foi substituida por esta abordagem.
