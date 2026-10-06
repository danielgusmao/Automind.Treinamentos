# Autenticação e Active Directory

## Login

O formulário recebe `usuário` e `senha` e valida diretamente contra `automind.com.br` usando `PrincipalContext.ValidateCredentials` com `ContextOptions.Negotiate`.

Após sucesso, a aplicação cria cookie próprio contendo somente atributos não secretos:

- `sAMAccountName`;
- nome de exibição;
- e-mail;
- cargo;
- departamento;
- role `Informatica` quando aplicável.

A senha não é armazenada.

## Administração

O usuário só recebe a role `Informatica` quando o AD confirma participação no grupo `_informatica`.

Falha de resolução de grupo não concede acesso administrativo.

## Pendentes

Filtro LDAP planejado:

- pessoa/user;
- `mail=*@automind.com.br`;
- conta não desabilitada.

## HTTP temporário

Sem HTTPS, usuário e senha trafegam sem confidencialidade de transporte. A v0.0.2 deve ser usada apenas para teste interno controlado até o certificado estar disponível.
