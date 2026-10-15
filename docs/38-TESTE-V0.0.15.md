# Teste v0.0.15

## Validação mínima apos deploy

1. Confirmar tela exibindo `v0.0.15`.
2. Confirmar login AD valido e login inválido.
3. Confirmar que usuário comum não acessa `/Admin`.
4. Confirmar que membro de `_treinamentos` acessa Administração.
5. Confirmar que remover o usuário de `_treinamentos` retira o menu/acesso sem novo login.
6. Confirmar que uma conta AD desabilitada perde a sessão em ate 2 minutos.
7. Abrir `/health` e confirmar HTTP 200.
8. Abrir treinamento publicado, responder todas as questões e concluir.
9. Confirmar PDF em `Evidências\Colaboradores\<sam>` e versão `v0.0.15` dentro do PDF.
10. Confirmar que `manifest.json` e `SHA256.txt` existem para o treinamento/versionamento.
11. Confirmar auditoria em `Logs\Audit-AAAAMMDD.jsonl`.
12. Confirmar que botao PDF consolidado continua baixando o relatorio via POST.
13. Confirmar Teams com webhook HTTPS configurado.
14. Confirmar que `Backup` não possui `Modify` para `IIS AppPool\Automind.Treinamentos`.

## Validação de código antes do push

Executar:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\Validate-v0.0.15.ps1
```

O script tambem executa os SelfTests sem dependencia externa de framework de testes e tenta `dotnet build`, scan de dependências e Gitleaks quando essas ferramentas estiverem instaladas.

## SSL

Não testar redirecionamento HTTPS/cookie Secure nesta versão. O item permanece pendente por ausencia de certificado.
