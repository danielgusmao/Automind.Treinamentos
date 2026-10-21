# Teste - v0.0.20

## Estado

`VALIDADO LOCALMENTE` para verificacoes estaticas no pacote.

`VALIDADO HISTORICAMENTE` para build/SelfTests da v0.0.19 apos a correcao de fuso fornecida pelo usuario.

`PENDENTE DE VALIDACAO` para build v0.0.20 no Windows, migration no SQLite real, Workflow real, IIS e envio Teams.

## Validado localmente

- versao atualizada para `v0.0.20`;
- importacao de auditoria antiga removida;
- migration `0.0.20` limpa `TeamsReminderHistory` uma unica vez;
- somente `Sent` conta como aviso entregue;
- HTTP 2xx sem confirmacao retorna `Failed`;
- tela usa apenas `Enviado`, `Falhou` e `Nao enviado`;
- largura fixa de 1540 px removida;
- coluna de acao fixa em desktop e layout mobile em cartoes;
- agenda e protecao `CycleKey` preservadas;
- nenhum envio Teams, IIS, AD, M365 ou banco real foi alterado na preparacao do pacote.

## Validacao antes do push

```powershell
dotnet restore .\Automind.Treinamentos.csproj; dotnet build .\Automind.Treinamentos.csproj -c Release --no-restore; dotnet run --project .\tests\Automind.Treinamentos.SelfTests\Automind.Treinamentos.SelfTests.csproj -c Release; powershell -ExecutionPolicy Bypass -File .\tests\Validate-v0.0.20.ps1
```

## Ambiente

Antes de publicar, fazer backup do SQLite. A migration v0.0.20 apaga somente o historico Teams para iniciar o controle do zero. Depois da publicacao, ajustar o Workflow real e validar com um unico destinatario autorizado antes de habilitar a agenda.
