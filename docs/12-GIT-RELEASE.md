# Git - branch release

Branch inicial aprovada para `Automind.Treinamentos`: `release`.

## Pasta local correta

```text
C:\Users\daniel.gusmao\source\repos\Automind.Treinamentos
```

Nao executar estes comandos dentro de `Automind.CadastroColaboradores`.

## GitHub

```text
https://github.com/danielgusmao/Automind.Treinamentos.git
```

## Azure DevOps

```text
https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos
```

Remotos padrao:

- `origin` = GitHub;
- `azure` = Azure DevOps.

## Primeiro commit do repositorio

```powershell
Set-Location "C:\Users\daniel.gusmao\source\repos\Automind.Treinamentos"
git init
git remote add origin https://github.com/danielgusmao/Automind.Treinamentos.git
git remote add azure https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos
git add .
git commit -m "feat: estrutura persistente e Teams v0.0.5"
git branch -M release
git push -u origin release
git push -u azure release
```

## Atualizacoes seguintes

```powershell
Set-Location "C:\Users\daniel.gusmao\source\repos\Automind.Treinamentos"
git switch release
git add .
git commit -m "descricao da alteracao"
git push origin release
git push azure release
```

## Validacao

```powershell
git status
git branch --show-current
git remote -v
```

Nunca versionar `Automind__Teams__WebhookUrl`.
