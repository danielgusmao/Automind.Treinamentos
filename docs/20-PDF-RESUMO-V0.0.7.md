# PDF de evidência com resumo - v0.0.7

## Objetivo
A evidência individual precisa demonstrar não apenas que o colaborador concluiu o treinamento, mas tambem quais assuntos foram apresentados.

## Estrutura do PDF
### Página 1 - evidência de conclusão
- treinamento, versão, nota e aprovacao;
- tempo estimado e tempo realizado;
- nome, login AD, e-mail, cargo e departamento;
- inicio e conclusão;
- termo de ciencia;
- protocolo;
- SHA-256 do treinamento.

### Página 2 - resumo do treinamento
- título e código;
- resumo textual do treinamento;
- principais temas abordados;
- versão, tempo estimado e resultado.

## Fonte do resumo
Ordem de prioridade:
1. `Trainings.SummaryText`;
2. texto abaixo do marcador `RESUMO FINAL` em `ContentText`;
3. `Description`;
4. trecho normalizado de `ContentText`.

## Primeiro treinamento
O treinamento de Segurança da Informação registra no PDF os seguintes pontos:
- senhas fortes e unicas e autenticacao multifator (MFA);
- phishing e engenharia social;
- dados pessoais e LGPD;
- resposta a incidentes e abertura imediata de chamado no TOPDESK.

## Banco
A coluna `SummaryText` e criada por migracao idempotente no startup. Bancos v0.0.6 existentes não precisam ser apagados.
