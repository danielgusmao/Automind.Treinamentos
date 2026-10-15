# Arquitetura - estado atual v0.0.10

```text
Navegador
   |
   v
ASP.NET Core MVC / IIS
   |
   +--> AD: autenticacao usuário/senha
   |
   +--> AD: consulta de usuários elegíveis
   |
   +--> Teams Workflow: lembretes { recipient, text }
   |
   +--> SQLite: treinamentos, questões, conclusões
   |
   +--> Sistema de arquivos persistente
          +--> Data\Automind.Treinamentos.db
          +--> Treinamentos\<slug>\<versão>\manifest.json + SHA256.txt
          +--> Evidências\Colaboradores\<Nome - sam>\*.pdf
          +--> Evidências\Relatorios\*.pdf
          +--> Logs\Audit.jsonl
          +--> Backup\
```

## Componentes

- `AccountController`: login/logout AD.
- `TrainingController`: area do colaborador, execucao, rota `/Training/Start/{id}` e aceite.
- `AdminController`: funcoes restritas ao grupo AD `_treinamentos` por meio da policy `TreinamentosAdmin`, pendências e Teams.
- `AdAuthenticationService`: validação de credenciais + dados do usuário + revalidação da membership administrativa no AD.
- `AdDirectoryService`: usuários habilitados com e-mail `@automind.com.br`.
- `TrainingRepository`: acesso SQLite.
- `TrainingSnapshotService`: snapshot + SHA-256 do treinamento.
- `EvidenceService`: PDFs individuais e consolidados.
- `AuditService`: JSONL de auditoria.
- `TeamsWebhookService`: mesmo contrato do CadColab, segredo externo ao Git.
- `StorageService`: resolve a raiz de armazenamento e cria somente as pastas necessarias.
- `SimplePdfService`: gerador PDF corporativo com identidade Automind, logo, tempos, protocolo e relatorios consolidados.

## Ambientes

### Development

`Storage:RootPath=App_Data`, relativo ao projeto.

### Production

`Storage:RootPath=C:\Automind.Treinamentos`.

O diretorio `Web` não faz parte da raiz logica de dados; ele e apenas o destino da publicacao.

## Fonte oficial

O banco é a fonte oficial de conclusão. Os PDFs são evidências derivadas e possuem hash registrado.

## Layout de treinamentos

As views de treinamento usam o `_Layout.cshtml` compartilhado. Conteúdos especializados podem carregar CSS proprio pela secao `Styles`, mas não devem criar outro `<html>/<body>` ou remover a navegacao global.


## Autorização administrativa v0.0.10

`UseAuthentication` restaura apenas a identidade do cookie. Em seguida, middleware proprio remove qualquer role administrativa persistida/legada, consulta `AdAdminAuthorizationService` no LDAP e acrescenta `TreinamentosAdmin` somente ao principal em memoria da requisicao. `UseAuthorization` roda depois dessa etapa. Portanto menu Razor e `[Authorize(Policy = "TreinamentosAdmin")]` usam a mesma decisão atual.
