# Escopo - Automind.Treinamentos

## Objetivo

Criar uma aplicação interna para disponibilizar treinamentos corporativos, validar conhecimento, registrar ciência/aceite e manter evidências individuais e consolidadas.

## Perfis

### Colaborador

- autentica no Active Directory;
- visualiza treinamentos publicados;
- realiza conteúdo e quiz;
- registra aceite somente quando atinge a nota mínima;
- consulta/baixa seu comprovante em PDF.

### Administrador (`_treinamentos`)

Além das funções do colaborador:

- administra treinamentos;
- adiciona questões;
- publica/despublica;
- acompanha conclusões;
- consulta pendentes no AD;
- acessa PDFs individuais;
- gera relatório consolidado.

## Critério AD para população elegível

Usuários habilitados cujo atributo `mail` termina em `@automind.com.br`.

## Itens ainda pendentes / evolucoes futuras

- assinatura digital ICP-Brasil;
- workflow formal de aprovação de conteúdo;
- editor rico/WYSIWYG;
- HTTPS/TLS interno (fase seguinte).
