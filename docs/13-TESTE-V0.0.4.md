# Teste funcional - v0.0.4

## Objetivo

Validar somente os recursos alterados na v0.0.4 antes do push/deploy definitivo.

## 1. Versão e interface

- Confirmar `v0.0.4` na topbar e no rodape.
- Confirmar que `_informatica` não aparece visualmente no cabecalho.
- Confirmar que o aviso visual de HTTP não aparece no login nem no rodape.

## 2. Questões

- Abrir Administração -> Questões.
- Confirmar os botoes `Editar` e `Excluir`.
- Editar uma questão, salvar e confirmar que texto, alternativas e resposta correta foram atualizados.
- Se o treinamento estiver publicado, confirmar que o snapshot e atualizado sem alterar a conclusão já registrada de outro usuário.

## 3. Pendentes

- Confirmar os totais Elegíveis, Concluídos e Pendentes.
- Usuário pendente deve mostrar checkbox e botao `Enviar Teams`.
- Usuário concluído não deve mostrar checkbox nem botao de envio.
- `Marcar todos os pendentes` deve selecionar apenas os pendentes.

## 4. Teams

Antes do teste real, configurar externamente `Automind__Teams__WebhookUrl` com a URL HTTPS aprovada.

- Enviar para um unico usuário pendente de teste.
- Confirmar recebimento no Teams.
- Confirmar nome do treinamento, tempo estimado e link direto.
- Abrir o link e validar que, se não autenticado, o usuário faz login e retorna ao treinamento.
- Depois testar envio em lote para um conjunto pequeno de usuários de teste.
- Confirmar registros no `Audit.jsonl` sem webhook ou senha.

## 5. Novo treinamento

- Abrir `Novo treinamento`.
- Confirmar guia visual, modelo de conteúdo pre-preenchido e exemplo de questão.
- Criar um treinamento de teste e confirmar o fluxo para cadastro das questões.

## 6. Criterio de aprovacao

A v0.0.4 pode seguir para a branch `release` quando os itens acima passarem sem erro funcional ou de permissão.
