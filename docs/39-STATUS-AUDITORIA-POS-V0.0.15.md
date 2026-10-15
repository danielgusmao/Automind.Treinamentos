# Status da auditoria apos v0.0.15

## Resolvidos nesta versão

- SEG-002 - rate limit de login;
- SEG-003 - revalidação de conta AD durante sessão;
- INT-001 - consistencia entre conclusão, PDF e auditoria;
- INT-002 - validação/reconstrucao de snapshot;
- AUTH-001 - elegibilidade corporativa no backend;
- VAL-001 - validação completa das respostas do quiz;
- PATH-001 - proteção de caminho para snapshots;
- DB-001 - unicidade de Code + Version;
- DEV-001 - App Pool sem Modify em Backup;
- OBS-001 - rotacao e retencao de auditoria + correlation id;
- PERF-001 - timeout/cache LDAP;
- TEST-001 - SelfTests e script de validação pre-push sem framework externo;
- OPS-001 - registro de migration e backup pre-deploy;
- REL-001 - versão dinamica nos PDFs;
- SEC-004 - webhook Teams somente HTTPS;
- SEC-005 - headers de hardening e AllowedHosts restrito, exceto HSTS que depende de HTTPS;
- PERF-002 - N+1 removido da tela inicial e do total do dashboard;
- INFO-001 - detalhes de exception removidos da UI;
- QUIZ-001 - gabarito removido do HTML;
- DB-002 - invariantes basicas protegidas no SQLite;
- DEV-002 - PDF consolidado persistente alterado para POST + antiforgery;
- ARCH-001 - limpeza inicial: validação do quiz extraida, código legado sem consumidores removido e responsabilidades repetidas reduzidas.

## Pendente por decisão operacional

- SEG-001 - HTTPS/TLS, cookie Secure, HSTS e redirecionamento HTTP->HTTPS.
  Motivo: servidor ainda não possui certificado confiavel.

## Validações que dependem da maquina de desenvolvimento/HML

O pacote inclui `tests/Validate-v0.0.15.ps1`. Quando as ferramentas estiverem instaladas, executar:
- build Release;
- SelfTests;
- `dotnet list package --vulnerable --include-transitive`;
- Gitleaks.

OWASP ZAP continua sendo teste dinamico para HML autorizado e não deve ser executado automaticamente contra producao.
