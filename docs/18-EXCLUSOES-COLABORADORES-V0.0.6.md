# v0.0.6 - Exclusoes de colaboradores por treinamento

## Objetivo
Permitir que administradores removam uma pessoa da obrigacao de um treinamento sem alterar ou excluir o usuario do Active Directory.

## Regra
- A exclusao e por treinamento.
- Somente colaboradores ainda nao concluidos podem ser excluidos.
- Motivo e obrigatorio e fica persistido com data/hora e operador.
- O excluido deixa de contar como elegivel, pendente e denominador da adesao.
- O excluido nao aparece em selecao de lembretes Teams e o backend tambem bloqueia o envio.
- O treinamento deixa de aparecer em `Meus treinamentos` para o usuario excluido e o link direto e bloqueado.
- A reinclusao restaura a elegibilidade.
- Nenhuma operacao modifica o objeto no AD.

## Persistencia
Nova tabela SQLite `TrainingExclusions` com `TrainingId`, `SamAccountName`, nome/e-mail de referencia, motivo, data UTC e operador. A criacao usa `CREATE TABLE IF NOT EXISTS`, portanto a atualizacao e cumulativa e nao exige apagar o banco.
