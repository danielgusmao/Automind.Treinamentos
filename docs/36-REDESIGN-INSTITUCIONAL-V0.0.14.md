# v0.0.14 - Revisão visual institucional

## Objetivo

Aplicar ao Automind.Treinamentos o padrão visual aprovado com base no site institucional da Automind, sem alterar regra de negócio, autenticação, autorização, persistência, publicação, quiz, geração de evidências, Teams ou qualquer comportamento de backend.

## Escopo alterado

Somente camada visual:

- `Views/Shared/_Layout.cshtml`
- `Views/Account/Login.cshtml`
- `Views/Admin/Trainings.cshtml`
- `Views/Training/Index.cshtml`
- `Views/Training/SecurityAwareness.cshtml`
- `wwwroot/css/site.css`
- `wwwroot/css/security-awareness.css`
- referências visuais em `docs/reference/`

## Diretrizes visuais

- geometria reta, sem cantos arredondados;
- cabeçalho escuro com logo negativa;
- paleta escura/plum/purple/magenta coerente com a identidade institucional;
- blocos de texto mais estruturados;
- tabelas e formulários com linhas retas e divisórias claras;
- heros com grafismos diagonais geométricos;
- comportamento responsivo preservado;
- placeholder do login alterado de `daniel.gusmao` para `nome.sobrenome`.

## Garantia de não alteração da lógica

A revisão não altera arquivos em `Controllers`, `Services`, `Models`, `Data`, `Program.cs`, `appsettings*`, `.csproj` ou scripts de deploy.

Nos arquivos Razor modificados, os atributos de integração (`asp-action`, `asp-controller`, `method`, `name` e `id`) foram comparados contra a v0.0.14 anterior e permanecem equivalentes. O bloco JavaScript do treinamento dedicado permaneceu byte a byte idêntico.

## Referências

- `LAYOUT-INSTITUCIONAL-CATALOGO-v0.0.14.png`
- `LAYOUT-INSTITUCIONAL-TREINAMENTO-v0.0.14.png`
