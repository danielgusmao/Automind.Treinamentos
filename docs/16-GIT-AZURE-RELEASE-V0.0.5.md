# v0.0.5 - GitHub, Azure DevOps e Release

## Repositorios

GitHub:

```text
https://github.com/danielgusmao/Automind.Treinamentos.git
```

Azure DevOps:

```text
https://danielgusmao@dev.azure.com/danielgusmao/Automind.Treinamentos/_git/Automind.Treinamentos
```

Branch inicial: `release`.

## Diretriz de pipeline

Build recomendado:

```text
dotnet restore
dotnet build -c Release --no-restore
dotnet publish -c Release --no-build -o <artifact>/Web
```

O artefato de publicacao deve representar somente `Web`.

## Diretriz de Release

Destino da aplicacao:

```text
C:\Automind.Treinamentos\Web
```

Nao limpar/deletar:

```text
C:\Automind.Treinamentos\Data
C:\Automind.Treinamentos\Treinamentos
C:\Automind.Treinamentos\Evidencias
C:\Automind.Treinamentos\Logs
C:\Automind.Treinamentos\Backup
```

O segredo Teams e configuracao de ACL pertencem ao servidor, nao ao artefato.
