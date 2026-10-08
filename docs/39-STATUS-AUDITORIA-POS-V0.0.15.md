# Status da auditoria apos v0.0.15

## Resolvidos nesta versao

- SEG-002 - rate limit de login;
- SEG-003 - revalidacao de conta AD durante sessao;
- INT-001 - consistencia entre conclusao, PDF e auditoria;
- INT-002 - validacao/reconstrucao de snapshot;
- AUTH-001 - elegibilidade corporativa no backend;
- VAL-001 - validacao completa das respostas do quiz;
- PATH-001 - protecao de caminho para snapshots;
- DB-001 - unicidade de Code + Version;
- DEV-001 - App Pool sem Modify em Backup;
- OBS-001 - rotacao e retencao de auditoria + correlation id;
- PERF-001 - timeout/cache LDAP;
- TEST-001 - SelfTests e script de validacao pre-push sem framework externo;
- OPS-001 - registro de migration e backup pre-deploy;
- REL-001 - versao dinamica nos PDFs;
- SEC-004 - webhook Teams somente HTTPS;
- SEC-005 - headers de hardening e AllowedHosts restrito, exceto HSTS que depende de HTTPS;
- PERF-002 - N+1 removido da tela inicial e do total do dashboard;
- INFO-001 - detalhes de exception removidos da UI;
- QUIZ-001 - gabarito removido do HTML;
- DB-002 - invariantes basicas protegidas no SQLite;
- DEV-002 - PDF consolidado persistente alterado para POST + antiforgery;
- ARCH-001 - limpeza inicial: validacao do quiz extraida, codigo legado sem consumidores removido e responsabilidades repetidas reduzidas.

## Pendente por decisao operacional

- SEG-001 - HTTPS/TLS, cookie Secure, HSTS e redirecionamento HTTP->HTTPS.
  Motivo: servidor ainda nao possui certificado confiavel.

## Validacoes que dependem da maquina de desenvolvimento/HML

O pacote inclui `tests/Validate-v0.0.15.ps1`. Quando as ferramentas estiverem instaladas, executar:
- build Release;
- SelfTests;
- `dotnet list package --vulnerable --include-transitive`;
- Gitleaks.

OWASP ZAP continua sendo teste dinamico para HML autorizado e nao deve ser executado automaticamente contra producao.
