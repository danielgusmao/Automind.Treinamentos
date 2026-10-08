# Validacoes v0.0.15

Execute em uma maquina de desenvolvimento Windows antes do push:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\Validate-v0.0.15.ps1
```

O script faz verificacoes estaticas do hardening v0.0.15 e, quando `dotnet` estiver instalado, executa restore, build Release e scan de pacotes vulneraveis. Se `gitleaks` estiver instalado, tambem executa secret scan com redacao.

O script nao faz deploy, nao altera AD e nao escreve no banco de producao.

Os SelfTests nao usam framework/NuGet adicional e validam quiz, rejeicao de IDs forjados/opcoes invalidas, protecao de segmentos de caminho e convencao de nome da evidencia.
