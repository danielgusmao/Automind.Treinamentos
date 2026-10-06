# Teste v0.0.8

## Build

1. Abrir `Automind.Treinamentos.sln`.
2. Executar Restore.
3. Compilar em `Release`.
4. Confirmar que nao existem erros.

## Versao

- confirmar no topo e no rodape: `v0.0.8`;
- confirmar no artifact publicado: `VERSION.txt` com `v0.0.8`;
- a UI usa a versao do assembly como fonte oficial.

## Exclusoes permanentes

1. Acessar `Administracao > Exclusoes`.
2. Confirmar que as exclusoes anteriores foram migradas e aparecem com nome/login/e-mail/motivo.
3. Confirmar `produtos@automind.com.br` na lista ativa.
4. Reincluir uma conta de teste e confirmar que ela volta aos indicadores/treinamentos.
5. Excluir novamente a conta de teste e confirmar que desaparece de elegiveis/pendentes.
6. Confirmar que uma conta excluida nao recebe Teams e nao abre `/Training/Start/{id}`.
7. Confirmar que o historico de reinclusao permanece visivel.

## Persistencia

Depois de novo deploy, confirmar que a lista continua armazenada no banco em `C:\Automind.Treinamentos\Data`.

## Deploy

Push na branch `release` deve acionar a pipeline/Release automaticos. Nao fazer copia manual apos push bem-sucedido.
