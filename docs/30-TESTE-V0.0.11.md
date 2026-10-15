# Teste v0.0.11

## 1. Edição simples de rascunho

1. Entrar em Administração > Catálogo de treinamentos.
2. No treinamento `SI-002` em rascunho, clicar em `Editar`.
3. Alterar título, descrição, resumo, tempo, nota ou conteúdo.
4. Salvar.
5. Confirmar que o mesmo registro foi atualizado e continua como rascunho.

Resultado esperado: nenhuma nova versão e criada porque não ha evidências.

## 2. Edição de treinamento com conclusões

1. Abrir `Editar` em uma versão que já tenha pelo menos uma conclusão.
2. Confirmar o aviso de versão protegida.
3. Informar a nova versão sugerida ou outra versão diferente.
4. Salvar.

Resultado esperado:

- versão anterior permanece inalterada;
- nova versão e criada como rascunho;
- questões são copiadas;
- conclusões/evidências permanecem na versão anterior.

## 3. Protecao das questões

Abrir `Questões` de uma versão com conclusões.

Resultado esperado: perguntas aparecem para consulta, mas Editar/Excluir/Adicionar ficam bloqueados. O backend tambem rejeita tentativa direta de escrita.

## 4. Publicacao da nova versão

Depois de revisar a nova versão e suas questões, publicar.

Resultado esperado:

- nova versão fica `Publicado`;
- versão anteriormente publicada do mesmo código fica `Histórico`;
- versão histórica não exibe acao de Pendentes/Publicar;
- lembretes Teams so podem ser enviados para a versão publicada atual.

## 5. Evidências antigas

Abrir `Concluídos` da versão histórica e baixar uma evidência antiga.

Resultado esperado: PDF antigo continua disponivel e sem alteração.
