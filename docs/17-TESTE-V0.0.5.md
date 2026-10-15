# Teste funcional - v0.0.5

Validar apenas os pontos modificados nesta versão.

1. Build Release sem erro.
2. Confirmar `v0.0.5` na interface.
3. Em desenvolvimento, confirmar uso de `App_Data`.
4. Em Production, confirmar `Storage:RootPath=C:\Automind.Treinamentos`.
5. Confirmar rota `http://treinamentos.automind.com.br/Training/Start/1` autenticando e abrindo o primeiro treinamento.
6. Confirmar que a tela Pendentes habilita Teams somente quando `Automind__Teams__WebhookUrl` estiver configurado.
7. Enviar um lembrete real para um usuário pendente controlado e confirmar mensagem, tempo e link.
8. Confirmar que concluído continua sem checkbox e sem botao de Teams.
9. Confirmar que um novo aceite grava PDF fora de `Web`, em `Evidências\Colaboradores`.
10. Confirmar `Audit.jsonl` em `Logs`.
11. Confirmar SQLite em `Data`.
12. Confirmar snapshot em `Treinamentos`.

Não repetir o teste basico do Workflow Teams, pois o mesmo fluxo do CadColab já foi validado. O objetivo aqui e somente validar a integracao do novo aplicativo com a configuracao existente.
