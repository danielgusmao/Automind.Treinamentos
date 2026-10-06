# v0.0.13 - Publicacao explicita

## Problema

Na v0.0.12, a tela usava um unico POST `TogglePublish` com um campo oculto booleano. Em producao, o clique em `Publicar` no SI-002 chegou ao backend como despublicacao, comprovado pela mensagem `despublicado com sucesso` e pela ausencia da pasta de snapshot.

## Correcao

- `POST /Admin/PublishTraining` publica.
- `POST /Admin/UnpublishTraining` despublica.
- Nao existe mais parametro booleano para decidir a operacao.
- O snapshot e criado antes da atualizacao do estado publicado.
- Se o snapshot falhar, o treinamento nao e marcado como publicado.

## Resultado esperado

Ao publicar SI-002:

1. a tela deve exibir `publicado com sucesso`;
2. o status deve mudar para `Publicado`;
3. deve existir `C:\Automind.Treinamentos\Treinamentos\<slug>\1.0.0\manifest.json`;
4. deve existir `SHA256.txt` na mesma pasta.

A exclusao manual de treinamentos de teste nao faz parte desta versao.
