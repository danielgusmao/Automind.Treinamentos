# Teste v0.0.13

1. Abrir Administracao > Catalogo de treinamentos.
2. Confirmar SI-002 como Rascunho.
3. Confirmar que possui pelo menos uma questao e nota minima valida.
4. Clicar Publicar.
5. Esperado: mensagem `publicado com sucesso` e status `Publicado`.
6. No servidor, validar a criacao de `Treinamentos/<slug>/1.0.0/manifest.json` e `SHA256.txt`.
7. Clicar Despublicar somente depois de validar a publicacao.
8. Esperado: mensagem `despublicado com sucesso` e status `Rascunho`.
