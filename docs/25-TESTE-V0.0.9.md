# Teste v0.0.9 - grupo administrativo

## Casos obrigatorios

1. Usuario fora de `_treinamentos`: autentica normalmente, ve apenas `Meus treinamentos`, nao ve `Administracao` nem `Exclusoes` e recebe acesso negado ao abrir `/Admin` diretamente.
2. Usuario dentro de `_treinamentos`: ve e acessa Administracao.
3. Remover usuario de `_treinamentos` mantendo a sessao aberta: na proxima requisicao, o acesso administrativo deve desaparecer sem exigir logout.
4. Adicionar usuario a `_treinamentos` mantendo a sessao aberta: na proxima requisicao, o acesso administrativo deve ser concedido.
5. Cookie antigo contendo role `Informatica`: nao pode conceder acesso administrativo.
6. Indisponibilidade/erro na consulta ao AD: nao conceder privilegio administrativo.
