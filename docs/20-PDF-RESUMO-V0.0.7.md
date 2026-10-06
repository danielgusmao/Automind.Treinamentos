# PDF de evidencia com resumo - v0.0.7

## Objetivo
A evidencia individual precisa demonstrar nao apenas que o colaborador concluiu o treinamento, mas tambem quais assuntos foram apresentados.

## Estrutura do PDF
### Pagina 1 - evidencia de conclusao
- treinamento, versao, nota e aprovacao;
- tempo estimado e tempo realizado;
- nome, login AD, e-mail, cargo e departamento;
- inicio e conclusao;
- termo de ciencia;
- protocolo;
- SHA-256 do treinamento.

### Pagina 2 - resumo do treinamento
- titulo e codigo;
- resumo textual do treinamento;
- principais temas abordados;
- versao, tempo estimado e resultado.

## Fonte do resumo
Ordem de prioridade:
1. `Trainings.SummaryText`;
2. texto abaixo do marcador `RESUMO FINAL` em `ContentText`;
3. `Description`;
4. trecho normalizado de `ContentText`.

## Primeiro treinamento
O treinamento de Seguranca da Informacao registra no PDF os seguintes pontos:
- senhas fortes e unicas e autenticacao multifator (MFA);
- phishing e engenharia social;
- dados pessoais e LGPD;
- resposta a incidentes e abertura imediata de chamado no TOPDESK.

## Banco
A coluna `SummaryText` e criada por migracao idempotente no startup. Bancos v0.0.6 existentes nao precisam ser apagados.
