# Teste v0.0.10 - autorizacao administrativa em tempo real

## Casos obrigatorios

1. Usuario fora de `_treinamentos`: atualizar qualquer pagina autenticada; deve aparecer `Colaborador`, sem `Administracao` e sem `Exclusoes`.
2. Abrir `/Admin` fora do grupo: deve resultar em acesso negado.
3. Adicionar o usuario a `_treinamentos` no AD e atualizar a pagina: deve aparecer `Administrador` e os menus administrativos.
4. Remover o usuario de `_treinamentos` e atualizar a pagina: deve voltar a `Colaborador` e `/Admin` deve ser negado.
5. Repetir inclusao/remocao sem logout: a decisao deve acompanhar o AD em requisicoes novas.
6. Validar que o cookie nao contem role `TreinamentosAdmin` persistida como fonte de autorizacao.
7. Simular falha de consulta ao AD: o usuario continua autenticado como colaborador, mas sem acesso administrativo.
8. Validar membership aninhada caso o grupo `_treinamentos` passe a conter outro grupo.

## Observacao sobre replicacao AD

A aplicacao consulta o LDAP a cada requisicao, sem cache de role. Se o dominio possuir mais de um controlador de dominio, uma alteracao feita em um DC ainda pode levar o tempo normal de replicacao para aparecer em outro DC.

Se isso for observado, configurar `ActiveDirectory__AuthorizationServer` no App Pool para o FQDN do DC definido pela infraestrutura como fonte operacional para esse grupo. Essa configuracao nao exige alterar nem republicar o codigo.
