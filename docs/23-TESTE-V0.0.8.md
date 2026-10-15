# Teste v0.0.8

## Build

1. Abrir `Automind.Treinamentos.sln`.
2. Executar Restore.
3. Compilar em `Release`.
4. Confirmar que não existem erros.

## Versão

- confirmar no topo e no rodape: `v0.0.8`;
- confirmar no artifact publicado: `VERSION.txt` com `v0.0.8`;
- a UI usa a versão do assembly como fonte oficial.

## Exclusões permanentes

1. Acessar `Administração > Exclusões`.
2. Confirmar que as exclusões anteriores foram migradas e aparecem com nome/login/e-mail/motivo.
3. Confirmar `produtos@automind.com.br` na lista ativa.
4. Reincluir uma conta de teste e confirmar que ela volta aos indicadores/treinamentos.
5. Excluir novamente a conta de teste e confirmar que desaparece de elegíveis/pendentes.
6. Confirmar que uma conta excluida não recebe Teams e não abre `/Training/Start/{id}`.
7. Confirmar que o histórico de reinclusão permanece visivel.

## Persistencia

Depois de novo deploy, confirmar que a lista continua armazenada no banco em `C:\Automind.Treinamentos\Data`.

## Deploy

Push na branch `release` deve acionar a pipeline/Release automaticos. Não fazer copia manual apos push bem-sucedido.
