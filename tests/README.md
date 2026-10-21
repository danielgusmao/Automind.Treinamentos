# Validacoes v0.0.20

Execute em uma maquina de desenvolvimento Windows antes do push:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\Validate-v0.0.20.ps1
```

O script valida os estados simples de Teams, o inicio do historico do zero, a responsividade da tela de pendencias e preserva as verificacoes de hardening anteriores. Quando `dotnet` estiver instalado, executa restore, build Release, SelfTests e scan de pacotes vulneraveis. Se `gitleaks` estiver instalado, tambem executa secret scan com redacao.

Nao faz deploy, nao altera AD/IIS/Microsoft 365 e nao envia mensagens Teams.
