# v0.0.17 - Refinamento visual do login e banners

## Objetivo

Refinar a identidade visual aprovada na v0.0.16 sem modificar nenhuma regra funcional do Automind.Treinamentos.

## Origem da alteração

A revisão parte do feedback visual mais recente:

- textos que estavam em rosa claro devem usar o magenta institucional;
- a primeira tela de login está aprovada, mas deve perder as linhas/faixas neon inclinadas;
- o banner principal deve perder as barras degradê de fundo;
- o símbolo Automind deve permanecer sozinho como elemento gráfico do banner, em escala maior, preenchendo melhor a composição.

A cor informada como `#fa094` foi aplicada como `#fa094e`: hexadecimal CSS de cinco dígitos não é válido e `#fa094e` é a cor já definida na identidade visual do projeto desde a v0.0.16.

## Alterações aplicadas

### Textos de destaque

- `.eyebrow-light`: de `#ff6c96` para `#fa094e` via `var(--am-accent-a)`;
- `.training-eyebrow`: de `#ff6c96` para `#fa094e` via `var(--tr-magenta-2)`.

### Tela de login

- removidos os pseudo-elementos `:before` e `:after` que desenhavam as faixas inclinadas/neon;
- mantido o fundo `linear-gradient(110deg,#111018 0%,#1a1422 44%,#35113e 69%,#4d1056 100%)`;
- formulário, autenticação AD, mensagens, logo e responsividade permanecem sem alteração.

### Hero do portal e administração

- removidas as barras degradê desenhadas por `:after`;
- mantido o fundo escuro/gradiente do banner;
- ampliado o `automind-symbol.png` e reposicionado para ocupar mais a área direita;
- nenhum novo asset foi introduzido.

### Hero do treinamento dedicado

- removidas as barras degradê desenhadas por `:before`;
- símbolo Automind ampliado seguindo o mesmo padrão do portal;
- conteúdo, quiz, aceite, cronômetro e scripts permanecem intactos.

## Arquivos de código alterados

- `wwwroot/css/site.css`
- `wwwroot/css/security-awareness.css`
- `Automind.Treinamentos.csproj`
- `VERSION.txt`

Nenhuma View, Controller, Service, Model, arquivo de banco, configuração de AD/Teams ou script de deploy precisou ser modificado.

## Garantias preservadas

- grupo administrativo: `_treinamentos`;
- autenticação e autorização AD sem mudança;
- evidências em `Evidencias\Colaboradores\<login>`;
- PDFs e snapshots sem mudança;
- webhook Teams fora do Git;
- estrutura `Web`, `Data`, `Treinamentos`, `Evidencias`, `Logs`, `Backup` preservada;
- branch de deploy: `release`;
- pipeline/Release automática após push em `release`.
