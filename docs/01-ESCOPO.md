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

### `_informatica`

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

## Fora do escopo da v0.0.3

- assinatura digital ICP-Brasil;
- workflow de aprovação de conteúdo;
- editor rico/WYSIWYG;
- envio automático de lembretes;
- integração Teams/e-mail;
- HTTPS (aguardando certificado);
- publicação real no IIS do servidor 10.1.2.21.
