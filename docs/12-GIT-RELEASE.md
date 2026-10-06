# Git - branch release

Branch inicial aprovada para `Automind.Treinamentos`: `release`.

## GitHub

Repositorio informado:

```text
https://github.com/danielgusmao/Automind.Treinamentos.git
```

Na raiz local do projeto, depois do build aprovado:

```powershell
git init
git remote remove origin 2>$null
git remote add origin https://github.com/danielgusmao/Automind.Treinamentos.git
git switch -C release
git add .
git commit -m "feat: edicao de questoes e lembretes Teams v0.0.4"
git push -u origin release
```

Se o repositorio local ja tiver historico e a branch `release` existir, preferir:

```powershell
git switch release
git add .
git commit -m "feat: edicao de questoes e lembretes Teams v0.0.4"
git push origin release
```

## Azure DevOps

O remoto deve se chamar `azure`. Antes do primeiro push, confirmar o endereco exato do repositorio Azure DevOps; ele ainda nao foi informado para `Automind.Treinamentos`. Nao reutilizar URL de outro projeto.

Depois que o remoto `azure` estiver configurado:

```powershell
git push -u azure release
```

Validacao dos remotes:

```powershell
git remote -v
```

Nao colocar `Automind__Teams__WebhookUrl` no repositorio.
