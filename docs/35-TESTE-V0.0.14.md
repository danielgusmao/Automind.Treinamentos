# Teste v0.0.14

## Validacao funcional apos deploy

1. Confirmar que a aplicacao inicia normalmente e mostra `v0.0.14`.
2. Verificar se evidencias existentes foram movidas para `C:\Automind.Treinamentos\Evidencias\Colaboradores\<login>\` e continuam abrindo pelo portal.
3. Confirmar que `TrainingCompletions.EvidencePdfPath` aponta para o novo caminho quando a migracao ocorreu.
4. Concluir um treinamento com um usuario de teste que ainda nao possua conclusao dessa versao.
5. Confirmar que o PDF novo e salvo em `C:\Automind.Treinamentos\Evidencias\Colaboradores\<login>\`.
6. Confirmar o formato curto `<Codigo>_v<Versao>_<AAAAMMDD>_<Protocolo>.pdf`.
7. Baixar o comprovante pelo portal e confirmar que o nome entregue ao navegador e o mesmo nome fisico.
8. Confirmar que o hash do PDF gravado no banco continua valido.
9. Confirmar que `Evidencias\Relatorios` nao foi alterada.

## Exemplo esperado

```text
C:\Automind.Treinamentos\Evidencias\Colaboradores\daniel.gusmao\SI-001_v1.0.0_20261006_AM-20261006-35E0.pdf
```
