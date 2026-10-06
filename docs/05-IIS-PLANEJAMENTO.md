# Planejamento de IIS - ainda não executado

Servidor alvo: `10.1.2.21`.

## Estrutura final pretendida

- Aplicação publicada: `C:\Automind.Treinamentos\Web`.
- Dados: `C:\Automind.Treinamentos\Data`.
- Treinamentos: `C:\Automind.Treinamentos\Treinamentos`.
- Evidências: `C:\Automind.Treinamentos\Evidencias`.
- Logs: `C:\Automind.Treinamentos\Logs`.
- Backup: `C:\Automind.Treinamentos\Backup`.

## Antes de publicar

1. Definir identidade do App Pool.
2. Simular ACLs mínimas.
3. Fazer backup do estado anterior do IIS quando existir algo a alterar.
4. Criar App Pool/site separadamente.
5. Validar HTTP local no servidor.
6. Validar consulta AD.
7. Validar gravação SQLite/PDF.
8. Planejar rollback.

## HTTPS

Não habilitado na v0.0.2. Quando houver certificado, alterar:

- binding IIS para HTTPS;
- `CookieSecurePolicy` para `Always`;
- adicionar `UseHttpsRedirection()`;
- considerar HSTS após validação.

## Estado observado em 05/10/2026

- Site `Automind.Treinamentos` criado em `C:\Automind.Treinamentos\Web`.
- App Pool `Automind.Treinamentos`: `No Managed Code`, `Integrated`, `ApplicationPoolIdentity`, `AlwaysRunning`.
- A identidade do pool recebeu `Modify` somente em `C:\Automind.Treinamentos\Web\App_Data` e descendentes, necessário para SQLite, logs, snapshots e evidências.
- Após a ACL, o site respondeu HTTP 200 e iniciou com sucesso.
- Não ampliar a escrita do App Pool para todo o diretório `Web`.
- Teams v0.0.4 depende de configuração externa do webhook no ambiente do App Pool; o segredo não deve ser colocado no pacote.
