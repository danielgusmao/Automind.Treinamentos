# Teste - v0.0.19

## Estado

`VALIDADO LOCALMENTE` para verificacoes estaticas e SQL.

`PENDENTE DE VALIDACAO` para build .NET, IIS, AD, Workflow real e envio Teams.

## Validado localmente

- versao atualizada para `v0.0.19`;
- tabelas e indices SQL da v0.0.19 validados em banco SQLite temporario;
- importacao historica projetada para somente leitura dos `Audit-*.jsonl` e `INSERT OR IGNORE`, sem POST externo;
- agenda nasce desativada e nao existe seed que habilite envio automatico;
- controles da tela diferenciam aceite do Workflow, confirmacao, falha e resultado incerto;
- `CycleKey` possui indice unico e e usada somente em envio automatico;
- regras anteriores de AD/exclusao/conclusao permanecem como gates antes do envio;
- nenhuma mensagem Teams foi enviada e nenhum IIS/M365 foi alterado nesta rodada.

## Build

O ambiente de empacotamento nao possui SDK .NET instalado. O build nao foi executado aqui.

Antes do push, executar no Windows com .NET 10:

```powershell
dotnet restore .\Automind.Treinamentos.csproj; dotnet build .\Automind.Treinamentos.csproj -c Release --no-restore; dotnet run --project .\tests\Automind.Treinamentos.SelfTests\Automind.Treinamentos.SelfTests.csproj -c Release; powershell -ExecutionPolicy Bypass -File .\tests\Validate-v0.0.19.ps1
```

## Validacao de ambiente pendente

1. Fazer backup do SQLite conforme procedimento ja estabelecido.
2. Publicar a versao e confirmar migration `0.0.19`.
3. Abrir Pendencias e conferir importacao do lote antigo sem disparo de mensagens.
4. Atualizar o Workflow real para o contrato de confirmacao.
5. Enviar para um unico usuario de teste autorizado e confirmar `Confirmado` + horario.
6. Ativar uma agenda controlada em treinamento de teste e validar execucao sem duplicidade.
7. Validar que conclusao/exclusao impedem novo lembrete.
8. Validar App Pool ativo durante o periodo necessario para o worker.

Nao repetir envios para o lote real usado no incidente apenas para validar a feature.
