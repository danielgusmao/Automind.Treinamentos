# v0.0.15 - Hardening, consistencia e otimizações

Data: 08/10/2026

## Escopo

A v0.0.15 aplica as correcoes levantadas na auditoria tecnica da v0.0.14, preservando as regras de negocio, o modelo de autorização administrativa, o fluxo de treinamentos, o SQLite, as evidências e a integracao Teams.

HTTPS/TLS NAO foi ativado porque o servidor ainda não possui certificado. Esse e o unico item de segurança explicitamente mantido como pendencia nesta rodada.

## Correcoes aplicadas

### Segurança
- rate limit no POST de login: 10 tentativas/minuto por IP;
- revalidação da conta AD habilitada/elegivel em cache curto de 2 minutos;
- login so aceita conta habilitada com e-mail no sufixo corporativo configurado;
- autorização administrativa continua server-side e fail-closed;
- timeout LDAP de 10 segundos;
- DN do grupo administrativo cacheado por 5 minutos, sem cachear a decisão de membership;
- webhook Teams exige URI HTTPS;
- `AllowedHosts` restrito;
- headers `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` e `Permissions-Policy`;
- `.env`, secrets e certificados adicionados ao `.gitignore`;
- validação de slug/versão e containment de caminho para snapshots;
- limites de tamanho em campos administrativos e login;
- gabarito do treinamento dedicado removido do HTML/JavaScript;
- erros internos deixam de ser exibidos diretamente na UI administrativa.

### Integridade e evidências
- validação completa das respostas do quiz no servidor;
- IDs de questões desconhecidos e indices fora das opções são rejeitados;
- falha de auditoria depois da conclusão persistida não apaga mais o PDF nem transforma sucesso em falha;
- snapshot e comparado com o estado atual e reconstruido quando divergente;
- escrita de snapshot usa arquivos temporarios + troca atomica;
- `Code + Version` passa a ter indice UNIQUE case-insensitive;
- triggers SQLite protegem invariantes basicas de treinamentos, questões e conclusões;
- tabela `SchemaMigrations` registra a aplicacao da migration 0.0.15;
- PDFs usam a versão real do assembly, removendo `v0.0.8` hardcoded.

### Performance
- lista inicial de treinamentos usa duas consultas em lote em vez de uma consulta por treinamento;
- dashboard administrativo calcula total de conclusões com `COUNT(*)` unico;
- lista de usuários elegíveis do AD possui cache de 1 minuto;
- consulta do DN do grupo administrativo deixa de ser repetida em toda requisicao;
- `PRAGMA journal_mode=WAL` foi retirado da abertura de toda conexao e aplicado uma vez no startup;
- indices adicionais para publicacao e lookup de conclusões.

### Observabilidade
- `X-Correlation-ID` em toda resposta;
- correlation id incluido na auditoria quando houver contexto HTTP;
- auditoria passa a ser diaria: `Audit-AAAAMMDD.jsonl`;
- retencao configuravel em `Audit:RetentionDays`, padrao 365 dias;
- endpoint minimo `/health`.

### Deploy / menor privilegio
- script de preparacao cria backup do conjunto SQLite com App Pool parado antes da atualizacao;
- App Pool deixa de receber `Modify` em `Backup`;
- ACL anterior de escrita do App Pool em Backup e removida;
- backups ficam sob identidade administrativa do Release.

### Limpeza de código
- removido atributo `[HttpPost]` duplicado;
- removidos metodos e model antigos de exclusão por treinamento que não tinham mais consumidores;
- tabela legada `TrainingExclusions` foi preservada apenas para histórico/migracao;
- removido acesso redundante a WAL por conexao;
- consolidado PDF persistente passou de GET para POST + antiforgery.

## Pendencia deliberada

### HTTPS / certificado
Continua pendente ate existir certificado confiavel para `treinamentos.automind.com.br`.
Enquanto isso:
- URL continua HTTP;
- `CookieSecurePolicy` continua `None`;
- não ha `UseHttpsRedirection` nem HSTS.

Quando o certificado estiver disponivel, ativar esses tres pontos juntos e validar binding 443 antes de alterar os cookies.
