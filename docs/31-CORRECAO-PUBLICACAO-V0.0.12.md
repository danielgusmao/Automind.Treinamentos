# v0.0.12 - Correcao de publicacao

## Problema
O catalogo exibia o botao Publicar, mas algumas validacoes do backend retornavam HTTP 400 diretamente. Para o usuario isso podia parecer que o clique nao tinha efeito.

## Ajustes
- antiforgery token explicito no formulario de Publicar/Despublicar;
- treinamento sem questoes direciona para a tela de questoes com mensagem;
- nota minima invalida direciona para a tela de questoes com mensagem;
- excecoes de snapshot/banco/auditoria passam a aparecer no catalogo;
- confirmacao visual apos publicar/despublicar.

## Regra
Um treinamento so pode ser publicado quando possui pelo menos uma questao e `PassingScore <= quantidade de questoes`.
