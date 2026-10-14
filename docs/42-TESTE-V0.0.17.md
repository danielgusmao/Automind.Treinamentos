# Teste - v0.0.17

## Escopo

Validação da revisão visual sobre a v0.0.16.

## Verificações estáticas realizadas no pacote

- `VERSION.txt` ajustado para `v0.0.17`;
- `Automind.Treinamentos.csproj` ajustado para `0.0.17` em Version, FileVersion e InformationalVersion;
- não existem mais ocorrências de `#ff6c96` nos CSS de produção;
- `.eyebrow-light` usa `var(--am-accent-a)` = `#fa094e`;
- `.training-eyebrow` usa `var(--tr-magenta-2)` = `#fa094e`;
- pseudo-elementos de faixas neon do login foram desativados com `content:none`;
- pseudo-elementos de barras degradê do hero principal/administrativo e do treinamento foram desativados com `content:none`;
- símbolo Automind foi ampliado nos dois tipos de hero;
- nenhum arquivo em `Controllers`, `Services`, `Models`, `Data`, `Program.cs`, `Views` ou `appsettings*` foi alterado pela revisão visual.

## Build

O ambiente de empacotamento utilizado nesta revisão não possui SDK .NET instalado. Por isso o `dotnet build` não foi executado neste ambiente.

Antes do push para `release`, executar no ambiente de desenvolvimento com .NET 10:

```powershell
dotnet restore .\Automind.Treinamentos.sln; dotnet build .\Automind.Treinamentos.sln -c Release --no-restore
```

## Validação visual recomendada antes do push

1. Login desktop: fundo preservado e sem linhas neon.
2. Login mobile: formulário e logo mantidos.
3. Meus Treinamentos: hero sem barras decorativas, símbolo Automind grande e texto de destaque em `#fa094e`.
4. Administração: mesmo padrão visual do hero.
5. Segurança da Informação: hero sem barras, símbolo ampliado e conteúdo/quiz sem alteração.
6. Conferir responsividade em largura abaixo de 820px, onde o símbolo continua oculto conforme regra existente.

## Resultado esperado

Somente a apresentação visual muda. Fluxos de login, treinamento, quiz, aceite, evidência, administração, AD, exclusões e Teams devem permanecer idênticos à v0.0.16.
