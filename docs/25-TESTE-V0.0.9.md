# Teste v0.0.9 - grupo administrativo

## Casos obrigatórios

1. Usuário fora de `_treinamentos`: autentica normalmente, ve apenas `Meus treinamentos`, não ve `Administração` nem `Exclusões` e recebe acesso negado ao abrir `/Admin` diretamente.
2. Usuário dentro de `_treinamentos`: ve e acessa Administração.
3. Remover usuário de `_treinamentos` mantendo a sessão aberta: na proxima requisicao, o acesso administrativo deve desaparecer sem exigir logout.
4. Adicionar usuário a `_treinamentos` mantendo a sessão aberta: na proxima requisicao, o acesso administrativo deve ser concedido.
5. Cookie antigo contendo role `Informatica`: não pode conceder acesso administrativo.
6. Indisponibilidade/erro na consulta ao AD: não conceder privilegio administrativo.
