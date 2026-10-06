# Arquitetura - v0.0.5

```text
Navegador
   |
   v
ASP.NET Core MVC / IIS
   |
   +--> AD: autenticacao usuario/senha
   |
   +--> AD: consulta de usuarios elegiveis
   |
   +--> Teams Workflow: lembretes { recipient, text }
   |
   +--> SQLite: treinamentos, questoes, conclusoes
   |
   +--> Sistema de arquivos persistente
          +--> Data\Automind.Treinamentos.db
          +--> Treinamentos\<slug>\<versao>\manifest.json + SHA256.txt
          +--> Evidencias\Colaboradores\<Nome - sam>\*.pdf
          +--> Evidencias\Relatorios\*.pdf
          +--> Logs\Audit.jsonl
          +--> Backup\
```

## Componentes

- `AccountController`: login/logout AD.
- `TrainingController`: area do colaborador, execucao, rota `/Training/Start/{id}` e aceite.
- `AdminController`: funcoes restritas a `_informatica`, pendencias e Teams.
- `AdAuthenticationService`: validacao de credenciais + dados do usuario.
- `AdDirectoryService`: usuarios habilitados com e-mail `@automind.com.br`.
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

O diretorio `Web` nao faz parte da raiz logica de dados; ele e apenas o destino da publicacao.

## Fonte oficial

O banco e a fonte oficial de conclusao. Os PDFs sao evidencias derivadas e possuem hash registrado.

## Layout de treinamentos

As views de treinamento usam o `_Layout.cshtml` compartilhado. Conteudos especializados podem carregar CSS proprio pela secao `Styles`, mas nao devem criar outro `<html>/<body>` ou remover a navegacao global.
