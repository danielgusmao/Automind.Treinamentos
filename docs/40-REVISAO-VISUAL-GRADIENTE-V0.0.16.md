# v0.0.16 - Revisao visual global de destaque e gradiente

## Objetivo

Ajustar somente o layout do Automind.Treinamentos para reforcar o destaque visual da marca e corrigir o gradiente institucional, sem qualquer alteracao de comportamento funcional do sistema.

## O que foi solicitado

- usar a composicao equivalente a **opcao 2** para a capa/hero, com o simbolo mais forte e evidente;
- aplicar o gradiente `#fa094e -> #4d1056` nas areas de destaque;
- estender essa linguagem visual para **todo o sistema**;
- nao alterar regra de negocio nem adicionar novas funcionalidades.

## Escopo alterado

Somente arquivos de camada visual:

- `wwwroot/css/site.css`
- `wwwroot/css/security-awareness.css`
- `VERSION.txt`
- `docs/00-CHECKPOINT.md`
- `docs/40-REVISAO-VISUAL-GRADIENTE-V0.0.16.md`

## Ajustes visuais aplicados

- hero principal e hero administrativo com simbolo maior na lateral direita;
- gradiente principal padronizado em `#fa094e -> #4d1056`;
- botoes primarios atualizados para o novo gradiente;
- cards, chips, modais e blocos de destaque com bordas/realces coerentes com a nova paleta;
- login alinhado a mesma identidade visual;
- treinamento dedicado de Seguranca da Informacao harmonizado com a mesma assinatura visual.

## Garantia de nao alteracao da logica

Nenhum arquivo de `Controllers`, `Services`, `Models`, `Data`, `Program.cs`, `appsettings*`, `.csproj` ou scripts de deploy foi modificado.

A revisao e exclusivamente visual.
