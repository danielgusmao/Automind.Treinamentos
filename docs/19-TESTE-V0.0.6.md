# Teste funcional - v0.0.6

1. Publicar a v0.0.6 preservando `Data`, `Treinamentos`, `Evidências`, `Logs` e `Backup`.
2. Confirmar `v0.0.6` no topo.
3. Abrir Administração > Pendências do treinamento.
4. Em um colaborador pendente, clicar `Excluir`, informar um motivo e confirmar.
5. Confirmar que o colaborador sai de elegíveis/pendentes e aparece em `Excluidos deste treinamento`.
6. Confirmar que o contador `Excluidos` aumenta e a adesão usa apenas os elegíveis restantes.
7. Confirmar que o excluido não recebe checkbox nem pode ser alvo de Teams.
8. Login com o colaborador excluido: o treinamento não deve aparecer em `Meus treinamentos`; link direto deve voltar ao catálogo com aviso.
9. Clicar `Reincluir` e confirmar que o colaborador volta a ser elegivel/pendente.
10. Confirmar que nenhuma alteração foi feita no objeto do Active Directory.
