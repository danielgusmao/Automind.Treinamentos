# Layout, tempo e evidencias PDF - v0.0.2

## Referencias

A interface usa como referencia visual o pacote fornecido do projeto `Automind.CadastroColaboradores v0.1.37`, principalmente a topbar, tela de login, paleta magenta/purpura e imagens oficiais.

O PDF simples da v0.0.1 foi considerado baseline funcional, mas visualmente insuficiente. A v0.0.2 cria um documento corporativo estruturado.

## Assets incorporados

- `automind-logo-negative.png`: topbar, login e treinamento;
- `automind-logo-positive.png`: versoes claras/mobile;
- `automind-symbol.png`: hero/dashboard e favicon;
- `automind-wallpaper.jpg`: tela de login;
- `automind-logo-positive.jpg`: incorporacao binaria no PDF.

## Medicao de tempo

O sistema diferencia:

1. **Tempo estimado**: definido no cadastro do treinamento e exibido antes/durante a execucao.
2. **Tempo realizado**: intervalo entre a abertura do treinamento e o aceite concluido, calculado pelo servidor.

O cronometro JavaScript e apenas visual. O valor oficial da evidencia e `DurationSeconds`, calculado no controller a partir do timestamp da sessao do servidor.

## Evidencia individual

O PDF individual inclui marca, treinamento, versao, tempo estimado, tempo realizado, identidade AD, resultado, aceite, inicio, conclusao, protocolo, declaracao e hash do snapshot.

## Relatorio consolidado

O PDF consolidado usa pagina paisagem, tabela de colaboradores, nota, tempo, data e protocolo. Tambem exibe a media de duracao registrada.

## Compatibilidade

A v0.0.2 nao apaga o banco v0.0.1. As novas colunas sao adicionadas somente se ainda nao existirem. Registros antigos ficam com duracao `0`/nao registrada.
