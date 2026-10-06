# v0.0.10 - autorizacao administrativa em tempo real

## Problema observado na v0.0.9

A v0.0.9 removeu a dependencia do cookie antigo, mas ainda usava `UserPrincipal.GetAuthorizationGroups()` para descobrir se o usuario pertencia ao grupo `_treinamentos`.

Esse mecanismo e apropriado para montar grupos de autorizacao, porem nao e a melhor fonte para uma checagem operacional que precisa refletir mudancas de membership durante uma sessao aberta. Ele tambem pode falhar durante a enumeracao quando ha grupos/SIDs que nao resolvem corretamente.

Sintoma real observado: o usuario podia ser incluido novamente em `_treinamentos` no Active Directory e continuar aparecendo como `Colaborador` na pagina seguinte.

## Decisao da v0.0.10

A autorizacao administrativa deixou de ser uma propriedade persistida da autenticacao.

O cookie passa a guardar apenas a identidade do colaborador:

- `sAMAccountName`;
- nome;
- e-mail;
- cargo;
- departamento.

Nenhuma role administrativa e salva no cookie ou na sessao.

Em cada requisicao autenticada:

1. o sistema remove da identidade em memoria qualquer role antiga `TreinamentosAdmin` ou `Informatica`;
2. consulta diretamente o LDAP;
3. localiza o grupo configurado em `ActiveDirectory:AdminGroup`;
4. verifica se o usuario habilitado pertence a esse grupo usando o matching rule in chain `1.2.840.113556.1.4.1941`;
5. adiciona `TreinamentosAdmin` somente na identidade em memoria da requisicao atual;
6. a policy MVC `TreinamentosAdmin` e avaliada depois dessa consulta.

Com isso, o menu e o backend usam a mesma decisao de autorizacao no mesmo request.

## Grupo oficial

```text
_treinamentos
```

Configuracao:

```json
"ActiveDirectory": {
  "AdminGroup": "_treinamentos",
  "AuthorizationServer": ""
}
```

`AuthorizationServer` e opcional. Quando vazio, a consulta usa `automind.com.br` e o DC e escolhido pelo dominio. Se for necessario fixar a consulta em um controlador de dominio especifico, a producao pode definir no App Pool:

```text
ActiveDirectory__AuthorizationServer=<FQDN-do-DC>
```

Nao hardcodar IP/DC no codigo enquanto o controlador preferencial nao for validado.

## Comportamento de seguranca

- Usuario fora do grupo: sem menu e sem acesso a `/Admin`.
- Usuario dentro do grupo: menu e backend administrativo liberados.
- Cookie antigo com role administrativa: ignorado/removido na requisicao.
- Falha LDAP: fail-closed, sem privilegio administrativo.
- Conta AD desabilitada: nao recebe privilegio administrativo.
- Membership direta ou via grupo aninhado: suportada.

## Arquivos principais

- `Services/AdAdminAuthorizationService.cs`
- `Services/AdAuthenticationService.cs`
- `Controllers/AccountController.cs`
- `Program.cs`
- `Services/Options.cs`
- `appsettings.json`
