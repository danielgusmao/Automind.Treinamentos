# v0.0.11 - Edicao e versionamento seguro de treinamentos

## Objetivo

Permitir editar um treinamento pelo catalogo administrativo sem alterar retroativamente evidencias ja emitidas.

## Regras

1. Todo treinamento passa a ter acao `Editar` no catalogo.
2. `Code` e `Slug` sao identificadores tecnicos e ficam somente leitura depois da criacao.
3. Sem conclusoes/evidencias:
   - titulo, descricao, resumo do PDF, versao, tempo, nota, conteudo e obrigatoriedade podem ser atualizados na mesma versao logica;
   - se estiver publicado, o snapshot e regravado depois da validacao.
4. Com pelo menos uma conclusao/evidencia:
   - a versao existente fica imutavel;
   - salvar exige uma nova versao;
   - o sistema cria uma nova linha em `Trainings` como rascunho;
   - todas as questoes sao copiadas para a nova versao;
   - conclusoes, PDFs, hashes, protocolos e snapshot da versao anterior permanecem vinculados ao registro antigo.
5. Questoes de uma versao com conclusoes ficam somente leitura no frontend e tambem sao bloqueadas no backend.
6. Ao publicar uma nova versao do mesmo `Code`, a versao anteriormente publicada e marcada como historica e deixa de receber novos colaboradores.
7. Versoes historicas continuam disponiveis na Administracao para consulta de concluidos/evidencias, mas nao podem enviar lembretes Teams nem voltar a ser publicadas diretamente.

## Compatibilidade com banco existente

A migracao adiciona:

- `Trainings.FamilySlug` - identifica a familia logica entre revisoes;
- `Trainings.IsArchived` - identifica versoes historicas.

Registros existentes recebem `FamilySlug = Slug` automaticamente. Nenhum registro de conclusao ou evidencia e apagado.

## Layout dedicado SI-001

O treinamento `security-awareness-v1` continua usando o layout visual dedicado. Nesta versao:

- titulo e descricao do cabecalho passam a vir do banco;
- tempo e nota continuam dinamicos;
- os modulos visuais permanecem definidos pelo template dedicado;
- o campo de conteudo nao e exibido para edicao nesse layout para evitar uma falsa expectativa de alteracao.

## Proxima alteracao planejada

Depois de validar a v0.0.11, reorganizar as evidencias para uma estrutura por treinamento/versao, preservando os PDFs existentes.
