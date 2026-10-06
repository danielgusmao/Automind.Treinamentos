# Roteiro de teste local - v0.0.1

## Pré-requisitos

- Windows unido ao domínio/alcance do AD Automind;
- Visual Studio com workload ASP.NET;
- SDK .NET 10.0;
- acesso ao NuGet.

## Teste 1 - build

- abrir `Automind.Treinamentos.sln`;
- Restore NuGet;
- Build Debug;
- nenhuma alteração em servidor necessária.

## Teste 2 - inicialização

Executar perfil `http`.

Esperado:

- abrir `http://localhost:5180`;
- redirecionar para `/Account/Login`;
- criar `App_Data` automaticamente.

## Teste 3 - usuário comum

Usar uma conta AD de teste habilitada.

Esperado:

- login aceito;
- abrir `Meus Treinamentos`;
- não exibir menu Administração;
- treinamento Segurança da Informação disponível.

## Teste 4 - primeiro treinamento

- responder propositalmente uma questão errada;
- verificar que o aceite não é liberado;
- corrigir e obter 5/5;
- marcar ciência;
- registrar aceite.

Esperado:

- protocolo criado pelo servidor;
- conclusão no SQLite;
- PDF em `App_Data\Evidencias\Colaboradores\<Nome> - <login>`;
- snapshot em `App_Data\Treinamentos\seguranca-da-informacao\1.0.0`;
- registro no `Audit.jsonl`.

## Teste 5 - `_informatica`

Entrar com usuário pertencente ao grupo `_informatica`.

Esperado:

- menu Administração;
- consulta de usuários AD elegíveis;
- lista concluídos/pendentes;
- PDF consolidado;
- cadastro de novo treinamento em rascunho.

## Parada de segurança

Se o primeiro build ou teste AD retornar erro inesperado, parar antes de qualquer publicação em IIS.
