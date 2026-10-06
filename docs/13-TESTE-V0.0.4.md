# Teste funcional - v0.0.4

## Objetivo

Validar somente os recursos alterados na v0.0.4 antes do push/deploy definitivo.

## 1. Versao e interface

- Confirmar `v0.0.4` na topbar e no rodape.
- Confirmar que `_informatica` nao aparece visualmente no cabecalho.
- Confirmar que o aviso visual de HTTP nao aparece no login nem no rodape.

## 2. Questoes

- Abrir Administracao -> Questoes.
- Confirmar os botoes `Editar` e `Excluir`.
- Editar uma questao, salvar e confirmar que texto, alternativas e resposta correta foram atualizados.
- Se o treinamento estiver publicado, confirmar que o snapshot e atualizado sem alterar a conclusao ja registrada de outro usuario.

## 3. Pendentes

- Confirmar os totais Elegiveis, Concluidos e Pendentes.
- Usuario pendente deve mostrar checkbox e botao `Enviar Teams`.
- Usuario concluido nao deve mostrar checkbox nem botao de envio.
- `Marcar todos os pendentes` deve selecionar apenas os pendentes.

## 4. Teams

Antes do teste real, configurar externamente `Automind__Teams__WebhookUrl` com a URL HTTPS aprovada.

- Enviar para um unico usuario pendente de teste.
- Confirmar recebimento no Teams.
- Confirmar nome do treinamento, tempo estimado e link direto.
- Abrir o link e validar que, se nao autenticado, o usuario faz login e retorna ao treinamento.
- Depois testar envio em lote para um conjunto pequeno de usuarios de teste.
- Confirmar registros no `Audit.jsonl` sem webhook ou senha.

## 5. Novo treinamento

- Abrir `Novo treinamento`.
- Confirmar guia visual, modelo de conteudo pre-preenchido e exemplo de questao.
- Criar um treinamento de teste e confirmar o fluxo para cadastro das questoes.

## 6. Criterio de aprovacao

A v0.0.4 pode seguir para a branch `release` quando os itens acima passarem sem erro funcional ou de permissao.
