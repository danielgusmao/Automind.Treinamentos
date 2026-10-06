# Teste v0.0.11

## 1. Edicao simples de rascunho

1. Entrar em Administracao > Catalogo de treinamentos.
2. No treinamento `SI-002` em rascunho, clicar em `Editar`.
3. Alterar titulo, descricao, resumo, tempo, nota ou conteudo.
4. Salvar.
5. Confirmar que o mesmo registro foi atualizado e continua como rascunho.

Resultado esperado: nenhuma nova versao e criada porque nao ha evidencias.

## 2. Edicao de treinamento com conclusoes

1. Abrir `Editar` em uma versao que ja tenha pelo menos uma conclusao.
2. Confirmar o aviso de versao protegida.
3. Informar a nova versao sugerida ou outra versao diferente.
4. Salvar.

Resultado esperado:

- versao anterior permanece inalterada;
- nova versao e criada como rascunho;
- questoes sao copiadas;
- conclusoes/evidencias permanecem na versao anterior.

## 3. Protecao das questoes

Abrir `Questoes` de uma versao com conclusoes.

Resultado esperado: perguntas aparecem para consulta, mas Editar/Excluir/Adicionar ficam bloqueados. O backend tambem rejeita tentativa direta de escrita.

## 4. Publicacao da nova versao

Depois de revisar a nova versao e suas questoes, publicar.

Resultado esperado:

- nova versao fica `Publicado`;
- versao anteriormente publicada do mesmo codigo fica `Historico`;
- versao historica nao exibe acao de Pendentes/Publicar;
- lembretes Teams so podem ser enviados para a versao publicada atual.

## 5. Evidencias antigas

Abrir `Concluidos` da versao historica e baixar uma evidencia antiga.

Resultado esperado: PDF antigo continua disponivel e sem alteracao.
