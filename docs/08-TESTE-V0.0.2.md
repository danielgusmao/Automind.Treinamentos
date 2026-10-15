# Roteiro de teste local - v0.0.2

## Objetivo

Validar a revisão visual, medição de tempo e novas evidências antes de qualquer alteração no IIS do servidor `10.1.2.21`.

## Pre-condicoes

- abrir `Automind.Treinamentos.sln` no Visual Studio;
- restaurar NuGet;
- compilar em Debug;
- executar somente por HTTP local/controlado nesta fase;
- usar conta de teste quando possível, pois ainda não existe HTTPS.

## Validações

1. Login exibe wallpaper e logos Automind e autentica no AD.
2. Usuário comum visualiza somente `Meus Treinamentos`.
3. Usuário de `_informatica` visualiza `Administração`.
4. Card do treinamento mostra versão, nota mínima e tempo estimado.
5. Ao abrir o treinamento, o cronometro visual inicia e continua a partir do timestamp da sessão do servidor.
6. O aceite continua bloqueado se a pontuacao não atingir a nota mínima.
7. Ao concluir, o banco grava `StartedAtUtc`, `AcceptedAtUtc` e `DurationSeconds`.
8. A tela de conclusão mostra tempo estimado e realizado.
9. O PDF individual contem logo, resultado, tempos, identidade AD, aceite, protocolo e hash.
10. O PDF fica na pasta individual do colaborador.
11. O painel administrativo mostra o tempo real das conclusões.
12. O PDF consolidado mostra tempo por colaborador e media.
13. Um banco criado na v0.0.1 recebe as novas colunas sem exclusão/recriação.

## Criterio de parada

Qualquer erro de build, autenticacao, migracao do banco, gravacao da evidência ou inconsistencia de tempo deve ser tratado antes de discutir publicacao no IIS.
