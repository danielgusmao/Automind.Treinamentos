# Teste v0.0.7 - resumo no PDF

## Objetivo
Validar que um novo aceite gera uma evidência PDF com duas páginas e que a segunda página descreve o treinamento realizado.

## Validações
1. Publicar/iniciar o treinamento de Segurança da Informação.
2. Concluir o quiz e registrar o aceite.
3. Abrir o PDF individual.
4. Confirmar página 1 com dados do colaborador, nota, datas, duracao, protocolo e hash.
5. Confirmar página 2 com o título `RESUMO DO TREINAMENTO`.
6. Confirmar os temas: senhas/MFA; phishing/engenharia social; dados pessoais/LGPD; incidentes/TOPDESK.
7. Criar um treinamento generico preenchendo `Resumo para o PDF` e confirmar que esse texto aparece na evidência.

## Observacao
Evidências antigas permanecem imutaveis. O novo formato vale para PDFs gerados depois da publicacao da v0.0.7.
