# v0.0.9 - Autorização administrativa pelo grupo _treinamentos

## Problema encontrado

A autorização administrativa era gravada como role dentro do cookie no momento do login. Como o cookie tinha validade de ate 8 horas, um usuário que havia recebido acesso administrativo continuava vendo e acessando a area de Administração ate renovar a sessão, mesmo depois de mudar a configuracao do grupo ou remover o usuário do grupo do AD.

Tambem havia nomenclatura legada `Informatica` e valor padrao `_informatica` no projeto.

## Correcao

- Grupo administrativo padrao: `_treinamentos`.
- Role interna: `TreinamentosAdmin`.
- A cada requisicao autenticada, o sistema reconsulta no Active Directory se o `sAMAccountName` atual pertence ao grupo administrativo configurado.
- Se o usuário não pertencer mais ao grupo, a role administrativa e removida do cookie e o menu Administração desaparece.
- Se passar a pertencer ao grupo, a role e adicionada e o cookie e renovado.
- Claims legadas `Informatica` são removidas e não concedem mais acesso.
- Falha ao consultar o AD resulta em ausencia de privilegio administrativo (fail closed).
- O usuário continua autenticado como colaborador; somente a autorização administrativa e alterada.

## Configuracao

`appsettings.json`:

```json
"ActiveDirectory": {
  "AdminGroup": "_treinamentos"
}
```

A configuracao pode ser sobrescrita no ambiente por `ActiveDirectory__AdminGroup`, se essa variavel existir no App Pool.
