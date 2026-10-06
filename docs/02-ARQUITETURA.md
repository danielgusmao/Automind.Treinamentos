# Arquitetura - v0.0.3

```text
Navegador
   |
   v
ASP.NET Core MVC
   |
   +--> AD: autenticação usuário/senha
   |
   +--> AD: consulta de usuários elegíveis
   |
   +--> SQLite: treinamentos, questões, conclusões
   |
   +--> Sistema de arquivos
          +--> snapshots de treinamentos
          +--> PDF por colaborador
          +--> PDFs consolidados
          +--> Audit.jsonl
```

## Componentes

- `AccountController`: login/logout AD.
- `TrainingController`: área do colaborador, execução e aceite.
- `AdminController`: funções restritas a `_informatica`.
- `AdAuthenticationService`: `ValidateCredentials` + dados do usuário.
- `AdDirectoryService`: usuários habilitados com e-mail `@automind.com.br`.
- `TrainingRepository`: acesso SQLite.
- `TrainingSnapshotService`: snapshot + SHA-256 do treinamento.
- `EvidenceService`: PDFs individuais e consolidados.
- `AuditService`: JSONL de auditoria.
- `SimplePdfService`: gerador PDF corporativo com identidade Automind, logo, tempos, protocolo e relatorios consolidados.

## Fonte oficial

O banco é a fonte oficial para conclusão. O PDF é evidência exportável e possui hash salvo no banco.


## Layout de treinamentos

As views de treinamento usam o `_Layout.cshtml` compartilhado. Conteudos especializados podem carregar CSS proprio pela secao opcional `Styles`, mas nao devem criar outro `<html>/<body>` ou remover a navegacao global.
