# v0.0.8 - Exclusões permanentes da base de colaboradores

## Objetivo

Separar contas que existem no Active Directory, mas não representam pessoas, da populacao real de colaboradores que deve participar dos treinamentos.

Exemplos: caixas compartilhadas, e-mails gerais de areas, contas técnicas e outros objetos com e-mail `@automind.com.br` que não correspondem a uma pessoa.

## Regra aprovada

A exclusão e global e permanente enquanto estiver ativa.

Uma conta presente na lista `DirectoryExclusions`:

- não entra em elegíveis;
- não entra em pendentes;
- não altera a taxa de adesão;
- não recebe lembretes Teams;
- não ve treinamentos em `Meus treinamentos`;
- não consegue iniciar treinamento por link direto;
- continua existindo normalmente no Active Directory.

## Administração

A página `Administração > Exclusões` mostra:

- nome;
- login;
- e-mail;
- tipo da conta;
- motivo;
- operador que excluiu;
- data/hora;
- acao `Reincluir`.

A página tambem preserva o histórico de contas reincluidas com operador e data.

## Migracao da lista anterior

Na primeira inicializacao da v0.0.8, registros existentes em `TrainingExclusions` são copiados para `DirectoryExclusions` sem apagar a tabela antiga. Assim, as exclusões já feitas no ambiente continuam existindo e passam a valer para todos os treinamentos.

Foi adicionada a conta conhecida:

- `Produtos` / `produtos` / `produtos@automind.com.br`;
- tipo: `E-mail geral / Caixa compartilhada`;
- motivo: e-mail de uso geral da empresa, não representa uma pessoa.

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

A tabela e persistente no SQLite em `C:\Automind.Treinamentos\Data` e não e apagada em deploy.

## Auditoria

A inclusão e a reinclusão são registradas no `Audit.jsonl`.

Nenhuma operacao desta funcionalidade escreve ou exclui objetos no AD.
