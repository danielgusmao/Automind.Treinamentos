# Contexto Atual - Automind.Treinamentos

## Versao

`v0.0.4`

## Estado atual

MVP local ASP.NET Core MVC com autenticacao AD, perfil administrativo `_informatica`, SQLite, treinamentos, quiz, aceite, evidencias PDF, comparacao de concluidos/pendentes no AD e identidade visual alinhada ao CadColab.

## Novidade principal v0.0.4

A administracao agora permite editar questoes e enviar lembretes Teams individuais ou em lote somente para colaboradores pendentes, incluindo o link direto do treinamento. O cadastro de novo treinamento passou a abrir com um modelo de conteudo e um exemplo de questao. A interface removeu a exibicao de `_informatica` e os avisos visuais de HTTP, mantendo a regra de autorizacao no backend.

## Recursos atuais

- edicao de questoes existentes;
- selecao individual e `Marcar todos os pendentes`;
- lembrete Teams individual/em lote com link direto;
- webhook Teams externo ao Git (`Automind__Teams__WebhookUrl`);
- modelo inicial para novo treinamento e exemplo de questao;
- identidade visual do Cadastro de Colaboradores reutilizada;
- logos, simbolo e wallpaper corporativos;
- login AD;
- navegacao e cards corporativos;
- treinamento de SI integrado ao shell principal;
- `EstimatedMinutes` por treinamento;
- cronometro visual durante o treinamento;
- tempo real calculado pelo servidor;
- `StartedAtUtc` e `DurationSeconds` persistidos;
- PDF individual institucional com logo, nota, tempos, protocolo e hash;
- PDF consolidado em paisagem com duracao por colaborador;
- migracao incremental do SQLite.

## Decisoes permanentes

- servidor pretendido: `10.1.2.21`;
- nenhuma alteracao no servidor sem aviso, backup e rollback;
- HTTP apenas temporario enquanto nao houver certificado;
- senha AD nunca e persistida;
- `_informatica` e o grupo administrativo;
- usuarios elegiveis: AD habilitado com `mail @automind.com.br`;
- banco e a fonte oficial; PDFs sao evidencias derivadas;
- pasta geral de treinamentos e pasta individual por colaborador;
- treinamento deve permanecer dentro do layout geral do portal;
- documentacao cumulativa com checkpoint atualizado em cada mudanca relevante.

## Primeiro treinamento

Seguranca da Informacao, versao `1.0.0`, nota minima 5/5 e tempo estimado de 8 minutos.

## Proxima acao

1. Build da `v0.0.4` no Visual Studio.
2. Validar edicao de questao e tela de pendencias.
3. GitHub confirmado em `https://github.com/danielgusmao/Automind.Treinamentos.git`; publicar a branch `release` após build aprovado.
4. Confirmar a URL específica do repositório Azure DevOps antes de configurar o remoto `azure`.
5. Configurar o segredo Teams no ambiente do App Pool antes do primeiro envio real.
6. Publicar no IIS somente apos o build e revisao das alteracoes.
