# v0.0.12 - Correcao de publicacao

## Problema
O catálogo exibia o botao Publicar, mas algumas validações do backend retornavam HTTP 400 diretamente. Para o usuário isso podia parecer que o clique não tinha efeito.

## Ajustes
- antiforgery token explicito no formulario de Publicar/Despublicar;
- treinamento sem questões direciona para a tela de questões com mensagem;
- nota mínima invalida direciona para a tela de questões com mensagem;
- excecoes de snapshot/banco/auditoria passam a aparecer no catálogo;
- confirmacao visual apos publicar/despublicar.

## Regra
Um treinamento so pode ser publicado quando possui pelo menos uma questão e `PassingScore <= quantidade de questões`.
