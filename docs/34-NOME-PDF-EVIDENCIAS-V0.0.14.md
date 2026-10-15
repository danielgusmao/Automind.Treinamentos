# v0.0.14 - Caminhos curtos das evidências

## Decisão final

A estrutura continua centrada no colaborador, mas a pasta passa a usar somente o login AD (`sAMAccountName`). Isso reduz o caminho e mantem um identificador unico e estavel mesmo se o nome da pessoa mudar.

```text
C:\Automind.Treinamentos\Evidências\Colaboradores\<login>\
```

Exemplo:

```text
C:\Automind.Treinamentos\Evidências\Colaboradores\daniel.gusmao\
```

## Nome curto do PDF

Cada novo PDF individual continua identificando treinamento, versão, data e protocolo, mas usa o código curto do treinamento:

```text
<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf
```

Exemplo:

```text
SI-001_v1.0.0_20261006_AM-20261006-35E0.pdf
```

Limites defensivos:

- login/pasta: ate 32 caracteres;
- código: ate 20 caracteres;
- versão: ate 16 caracteres;
- protocolo: ate 32 caracteres.

## Migracao das evidências existentes

Na inicializacao, o sistema procura evidências já registradas em `TrainingCompletions` e, quando o arquivo existe, migra de forma não destrutiva para o novo caminho curto:

```text
Evidências\Colaboradores\<Nome - login>\arquivo-antigo.pdf
        ->
Evidências\Colaboradores\<login>\<Código>_v<Versão>_<AAAAMMDD>_<Protocolo>.pdf
```

O campo `TrainingCompletions.EvidencePdfPath` e atualizado apos a movimentacao. O hash do PDF não muda porque o conteúdo do arquivo não é alterado.

O sistema nunca sobrescreve um arquivo já existente no destino. Em caso de colisão, preserva o caminho atual para evitar perda de evidência.

`Evidências\Relatorios` permanece inalterada.
