# Teste v0.0.10 - autorização administrativa em tempo real

## Casos obrigatórios

1. Usuário fora de `_treinamentos`: atualizar qualquer página autenticada; deve aparecer `Colaborador`, sem `Administração` e sem `Exclusões`.
2. Abrir `/Admin` fora do grupo: deve resultar em acesso negado.
3. Adicionar o usuário a `_treinamentos` no AD e atualizar a página: deve aparecer `Administrador` e os menus administrativos.
4. Remover o usuário de `_treinamentos` e atualizar a página: deve voltar a `Colaborador` e `/Admin` deve ser negado.
5. Repetir inclusão/remocao sem logout: a decisão deve acompanhar o AD em requisicoes novas.
6. Validar que o cookie não contem role `TreinamentosAdmin` persistida como fonte de autorização.
7. Simular falha de consulta ao AD: o usuário continua autenticado como colaborador, mas sem acesso administrativo.
8. Validar membership aninhada caso o grupo `_treinamentos` passe a conter outro grupo.

## Observacao sobre replicacao AD

A aplicacao consulta o LDAP a cada requisicao, sem cache de role. Se o dominio possuir mais de um controlador de dominio, uma alteração feita em um DC ainda pode levar o tempo normal de replicacao para aparecer em outro DC.

Se isso for observado, configurar `ActiveDirectory__AuthorizationServer` no App Pool para o FQDN do DC definido pela infraestrutura como fonte operacional para esse grupo. Essa configuracao não exige alterar nem republicar o código.
