# v0.0.9 - Autorizacao administrativa pelo grupo _treinamentos

## Problema encontrado

A autorizacao administrativa era gravada como role dentro do cookie no momento do login. Como o cookie tinha validade de ate 8 horas, um usuario que havia recebido acesso administrativo continuava vendo e acessando a area de Administracao ate renovar a sessao, mesmo depois de mudar a configuracao do grupo ou remover o usuario do grupo do AD.

Tambem havia nomenclatura legada `Informatica` e valor padrao `_informatica` no projeto.

## Correcao

- Grupo administrativo padrao: `_treinamentos`.
- Role interna: `TreinamentosAdmin`.
- A cada requisicao autenticada, o sistema reconsulta no Active Directory se o `sAMAccountName` atual pertence ao grupo administrativo configurado.
- Se o usuario nao pertencer mais ao grupo, a role administrativa e removida do cookie e o menu Administracao desaparece.
- Se passar a pertencer ao grupo, a role e adicionada e o cookie e renovado.
- Claims legadas `Informatica` sao removidas e nao concedem mais acesso.
- Falha ao consultar o AD resulta em ausencia de privilegio administrativo (fail closed).
- O usuario continua autenticado como colaborador; somente a autorizacao administrativa e alterada.

## Configuracao

`appsettings.json`:

```json
"ActiveDirectory": {
  "AdminGroup": "_treinamentos"
}
```

A configuracao pode ser sobrescrita no ambiente por `ActiveDirectory__AdminGroup`, se essa variavel existir no App Pool.
