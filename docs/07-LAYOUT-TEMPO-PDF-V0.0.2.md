# Layout, tempo e evidências PDF - v0.0.2

## Referencias

A interface usa como referencia visual o pacote fornecido do projeto `Automind.CadastroColaboradores v0.1.37`, principalmente a topbar, tela de login, paleta magenta/purpura e imagens oficiais.

O PDF simples da v0.0.1 foi considerado baseline funcional, mas visualmente insuficiente. A v0.0.2 cria um documento corporativo estruturado.

## Assets incorporados

- `automind-logo-negative.png`: topbar, login e treinamento;
- `automind-logo-positive.png`: versoes claras/mobile;
- `automind-symbol.png`: hero/dashboard e favicon;
- `automind-wallpaper.jpg`: tela de login;
- `automind-logo-positive.jpg`: incorporacao binaria no PDF.

## Medição de tempo

O sistema diferencia:

1. **Tempo estimado**: definido no cadastro do treinamento e exibido antes/durante a execucao.
2. **Tempo realizado**: intervalo entre a abertura do treinamento e o aceite concluído, calculado pelo servidor.

O cronometro JavaScript e apenas visual. O valor oficial da evidência e `DurationSeconds`, calculado no controller a partir do timestamp da sessão do servidor.

## Evidência individual

O PDF individual inclui marca, treinamento, versão, tempo estimado, tempo realizado, identidade AD, resultado, aceite, inicio, conclusão, protocolo, declaracao e hash do snapshot.

## Relatorio consolidado

O PDF consolidado usa página paisagem, tabela de colaboradores, nota, tempo, data e protocolo. Tambem exibe a media de duracao registrada.

## Compatibilidade

A v0.0.2 não apaga o banco v0.0.1. As novas colunas são adicionadas somente se ainda não existirem. Registros antigos ficam com duracao `0`/nao registrada.
