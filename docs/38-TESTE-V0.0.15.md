# Teste v0.0.15

## Validacao minima apos deploy

1. Confirmar tela exibindo `v0.0.15`.
2. Confirmar login AD valido e login invalido.
3. Confirmar que usuario comum nao acessa `/Admin`.
4. Confirmar que membro de `_treinamentos` acessa Administracao.
5. Confirmar que remover o usuario de `_treinamentos` retira o menu/acesso sem novo login.
6. Confirmar que uma conta AD desabilitada perde a sessao em ate 2 minutos.
7. Abrir `/health` e confirmar HTTP 200.
8. Abrir treinamento publicado, responder todas as questoes e concluir.
9. Confirmar PDF em `Evidencias\Colaboradores\<sam>` e versao `v0.0.15` dentro do PDF.
10. Confirmar que `manifest.json` e `SHA256.txt` existem para o treinamento/versionamento.
11. Confirmar auditoria em `Logs\Audit-AAAAMMDD.jsonl`.
12. Confirmar que botao PDF consolidado continua baixando o relatorio via POST.
13. Confirmar Teams com webhook HTTPS configurado.
14. Confirmar que `Backup` nao possui `Modify` para `IIS AppPool\Automind.Treinamentos`.

## Validacao de codigo antes do push

Executar:

```powershell
powershell -ExecutionPolicy Bypass -File .\tests\Validate-v0.0.15.ps1
```

O script tambem executa os SelfTests sem dependencia externa de framework de testes e tenta `dotnet build`, scan de dependencias e Gitleaks quando essas ferramentas estiverem instaladas.

## SSL

Nao testar redirecionamento HTTPS/cookie Secure nesta versao. O item permanece pendente por ausencia de certificado.
