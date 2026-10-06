# Layout integrado do treinamento - v0.0.3

## Motivo da alteracao

Na v0.0.2, o treinamento de Seguranca da Informacao usava `Layout = null` e montava um documento HTML completo proprio. Isso fazia o colaborador sair visualmente do portal ao iniciar o treinamento, perdendo a mesma topbar, navegacao, usuario, versao e rodape usados em `Meus Treinamentos`.

O requisito confirmado para a v0.0.3 e:

> O treinamento deve abrir dentro do mesmo layout do portal Automind.Treinamentos. O conteudo do HTML original continua sendo a base do treinamento, mas nao deve parecer outro site/pagina independente.

## Implementacao

- `Views/Training/SecurityAwareness.cshtml` voltou a usar o `_Layout.cshtml` padrao do portal.
- Removidos `<!doctype>`, `<html>`, `<head>`, `<body>` e rodape proprios do treinamento.
- A topbar corporativa, menu `Meus treinamentos`, menu administrativo para `_informatica`, identificacao do usuario, versao e botao `Sair` permanecem visiveis durante todo o treinamento.
- `Views/Training/Completed.cshtml` tambem foi integrado ao mesmo layout para que a conclusao nao mude de interface.
- `_Layout.cshtml` ganhou uma secao opcional `Styles`, permitindo CSS especifico de treinamento sem criar outro documento HTML.
- `security-awareness.css` foi refeito com seletores escopados em `.training-experience`, evitando interferencia no layout global/topbar.

## Conteudo preservado

O primeiro treinamento continua baseado no arquivo `docs/reference/treinamento_si_original.html`, mantendo:

- Senhas e Autenticacao (MFA);
- Phishing e Engenharia Social;
- Dados Pessoais e LGPD;
- quiz com 5 questoes;
- exigencia de 5/5;
- liberacao do Termo de Ciencia somente apos aprovacao;
- identificacao do colaborador vinda do AD;
- tempo estimado e cronometro da sessao;
- validacao final no servidor;
- evidencia PDF individual.

## Comportamento visual esperado

Fluxo:

```text
Login Automind
    -> Meus Treinamentos
        -> Iniciar treinamento
            -> MESMA topbar / navegacao / usuario / rodape
            -> hero do treinamento dentro da area central
            -> modulos
            -> quiz
            -> aceite
            -> conclusao no MESMO layout do portal
```

Nao deve existir abertura em nova aba nem pagina com identidade visual independente.

## Seguranca

A mudanca e apenas de composicao de interface. As regras de autenticacao AD, autorizacao `_informatica`, calculo de nota no servidor, registro de tempo, protocolo, SQLite, auditoria e PDF nao foram relaxadas.
