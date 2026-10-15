# v0.0.11 - Edição e versionamento seguro de treinamentos

## Objetivo

Permitir editar um treinamento pelo catálogo administrativo sem alterar retroativamente evidências já emitidas.

## Regras

1. Todo treinamento passa a ter acao `Editar` no catálogo.
2. `Code` e `Slug` são identificadores técnicos e ficam somente leitura depois da criação.
3. Sem conclusões/evidências:
   - título, descrição, resumo do PDF, versão, tempo, nota, conteúdo e obrigatoriedade podem ser atualizados na mesma versão logica;
   - se estiver publicado, o snapshot e regravado depois da validação.
4. Com pelo menos uma conclusão/evidência:
   - a versão existente fica imutável;
   - salvar exige uma nova versão;
   - o sistema cria uma nova linha em `Trainings` como rascunho;
   - todas as questões são copiadas para a nova versão;
   - conclusões, PDFs, hashes, protocolos e snapshot da versão anterior permanecem vinculados ao registro antigo.
5. Questões de uma versão com conclusões ficam somente leitura no frontend e tambem são bloqueadas no backend.
6. Ao publicar uma nova versão do mesmo `Code`, a versão anteriormente publicada e marcada como histórica e deixa de receber novos colaboradores.
7. Versoes históricas continuam disponíveis na Administração para consulta de concluídos/evidências, mas não podem enviar lembretes Teams nem voltar a ser publicadas diretamente.

## Compatibilidade com banco existente

A migracao adiciona:

- `Trainings.FamilySlug` - identifica a família logica entre revisoes;
- `Trainings.IsArchived` - identifica versoes históricas.

Registros existentes recebem `FamilySlug = Slug` automaticamente. Nenhum registro de conclusão ou evidência e apagado.

## Layout dedicado SI-001

O treinamento `security-awareness-v1` continua usando o layout visual dedicado. Nesta versão:

- título e descrição do cabecalho passam a vir do banco;
- tempo e nota continuam dinamicos;
- os módulos visuais permanecem definidos pelo template dedicado;
- o campo de conteúdo não e exibido para edição nesse layout para evitar uma falsa expectativa de alteração.

## Proxima alteração planejada

Depois de validar a v0.0.11, reorganizar as evidências para uma estrutura por treinamento/versão, preservando os PDFs existentes.
