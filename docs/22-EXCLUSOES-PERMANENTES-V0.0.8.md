# v0.0.8 - Exclusoes permanentes da base de colaboradores

## Objetivo

Separar contas que existem no Active Directory, mas nao representam pessoas, da populacao real de colaboradores que deve participar dos treinamentos.

Exemplos: caixas compartilhadas, e-mails gerais de areas, contas tecnicas e outros objetos com e-mail `@automind.com.br` que nao correspondem a uma pessoa.

## Regra aprovada

A exclusao e global e permanente enquanto estiver ativa.

Uma conta presente na lista `DirectoryExclusions`:

- nao entra em elegiveis;
- nao entra em pendentes;
- nao altera a taxa de adesao;
- nao recebe lembretes Teams;
- nao ve treinamentos em `Meus treinamentos`;
- nao consegue iniciar treinamento por link direto;
- continua existindo normalmente no Active Directory.

## Administracao

A pagina `Administracao > Exclusoes` mostra:

- nome;
- login;
- e-mail;
- tipo da conta;
- motivo;
- operador que excluiu;
- data/hora;
- acao `Reincluir`.

A pagina tambem preserva o historico de contas reincluidas com operador e data.

## Migracao da lista anterior

Na primeira inicializacao da v0.0.8, registros existentes em `TrainingExclusions` sao copiados para `DirectoryExclusions` sem apagar a tabela antiga. Assim, as exclusoes ja feitas no ambiente continuam existindo e passam a valer para todos os treinamentos.

Foi adicionada a conta conhecida:

- `Produtos` / `produtos` / `produtos@automind.com.br`;
- tipo: `E-mail geral / Caixa compartilhada`;
- motivo: e-mail de uso geral da empresa, nao representa uma pessoa.

## Banco

Nova tabela principal:

`DirectoryExclusions`

Campos relevantes:

- `SamAccountName`;
- `DisplayName`;
- `Email`;
- `Category`;
- `Reason`;
- `ExcludedAtUtc`;
- `ExcludedBy`;
- `IsActive`;
- `ReincludedAtUtc`;
- `ReincludedBy`.

A tabela e persistente no SQLite em `C:\Automind.Treinamentos\Data` e nao e apagada em deploy.

## Auditoria

A inclusao e a reinclusao sao registradas no `Audit.jsonl`.

Nenhuma operacao desta funcionalidade escreve ou exclui objetos no AD.
