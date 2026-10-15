# v0.0.6 - Exclusões de colaboradores por treinamento

## Objetivo
Permitir que administradores removam uma pessoa da obrigacao de um treinamento sem alterar ou excluir o usuário do Active Directory.

## Regra
- A exclusão e por treinamento.
- Somente colaboradores ainda não concluídos podem ser excluidos.
- Motivo e obrigatório e fica persistido com data/hora e operador.
- O excluido deixa de contar como elegivel, pendente e denominador da adesão.
- O excluido não aparece em selecao de lembretes Teams e o backend tambem bloqueia o envio.
- O treinamento deixa de aparecer em `Meus treinamentos` para o usuário excluido e o link direto e bloqueado.
- A reinclusão restaura a elegibilidade.
- Nenhuma operacao modifica o objeto no AD.

## Persistencia
Nova tabela SQLite `TrainingExclusions` com `TrainingId`, `SamAccountName`, nome/e-mail de referencia, motivo, data UTC e operador. A criação usa `CREATE TABLE IF NOT EXISTS`, portanto a atualizacao e cumulativa e não exige apagar o banco.
