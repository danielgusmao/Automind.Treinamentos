# v0.0.18 - Revisão ortográfica geral e ajuste final de gradiente

Data: 2026-10-15

## Objetivo

Aplicar a revisão ortográfica do portal aos padrões brasileiros de português, corrigindo acentuação e grafias solicitadas pelo usuário, além de alinhar o gradiente do bloco de login “Treinar. Comprovar. Evoluir.” ao padrão institucional.

## Escopo aplicado

- correção de textos visíveis na tela de login, incluindo aviso de credenciais;
- correção de textos nas telas administrativas;
- correção de mensagens auxiliares e validações exibidas ao usuário;
- correção de trechos do treinamento dedicado de Segurança da Informação;
- ajuste do gradiente do bloco lateral da página de login;
- reforço visual do símbolo Automind nos banners principais.

## Exemplos de correções

- “Credenciais nao sao armazenadas” → “Credenciais não são armazenadas”;
- “A senha e utilizada” → “A senha é utilizada”;
- “Gestao” → “Gestão”;
- “Catalogo” → “Catálogo”;
- “Codigo” → “Código”;
- “tecnico” → “técnico”;
- “conclusao/conclusoes” → “conclusão/conclusões”;
- “capacitacao” → “capacitação”;
- “evidencia/evidencias” → “evidência/evidências”.

## Arquivos principais alterados

- `Views/Account/Login.cshtml`
- `Views/Admin/Index.cshtml`
- `Views/Admin/EditTraining.cshtml`
- `Views/Admin/Questions.cshtml`
- `Views/Training/SecurityAwareness.cshtml`
- `Controllers/AdminController.cs`
- `Controllers/TrainingController.cs`
- `Models/ViewModels.cs`
- `wwwroot/css/site.css`
- `wwwroot/css/security-awareness.css`
- `Automind.Treinamentos.csproj`
- `VERSION.txt`

## Observações

- Não houve mudança em regras de negócio.
- O ambiente local desta execução não possui `dotnet` instalado, portanto a validação nesta rodada ficou restrita à revisão estática dos arquivos.
