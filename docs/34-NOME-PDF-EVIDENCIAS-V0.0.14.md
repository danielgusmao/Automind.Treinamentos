# v0.0.14 - Caminhos curtos das evidencias

## Decisao finalll

A estrutura continua centrada no colaborador, mas a pasta passa a usar somente o login AD (`sAMAccountName`). Isso reduz o caminho e mantem um identificador unico e estavel mesmo se o nome da pessoa mudar.

```text
C:\Automind.Treinamentos\Evidencias\Colaboradores\<login>\
```

Exemplo:

```text
C:\Automind.Treinamentos\Evidencias\Colaboradores\daniel.gusmao\
```

## Nome curto do PDF

Cada novo PDF individual continua identificando treinamento, versao, data e protocolo, mas usa o codigo curto do treinamento:

```text
<Codigo>_v<Versao>_<AAAAMMDD>_<Protocolo>.pdf
```

Exemplo:

```text
SI-001_v1.0.0_20261006_AM-20261006-35E0.pdf
```

Limites defensivos:

- login/pasta: ate 32 caracteres;
- codigo: ate 20 caracteres;
- versao: ate 16 caracteres;
- protocolo: ate 32 caracteres.

## Migracao das evidencias existentes

Na inicializacao, o sistema procura evidencias ja registradas em `TrainingCompletions` e, quando o arquivo existe, migra de forma nao destrutiva para o novo caminho curto:

```text
Evidencias\Colaboradores\<Nome - login>\arquivo-antigo.pdf
        ->
Evidencias\Colaboradores\<login>\<Codigo>_v<Versao>_<AAAAMMDD>_<Protocolo>.pdf
```

O campo `TrainingCompletions.EvidencePdfPath` e atualizado apos a movimentacao. O hash do PDF nao muda porque o conteudo do arquivo nao e alterado.

O sistema nunca sobrescreve um arquivo ja existente no destino. Em caso de colisao, preserva o caminho atual para evitar perda de evidencia.

`Evidencias\Relatorios` permanece inalterada.
