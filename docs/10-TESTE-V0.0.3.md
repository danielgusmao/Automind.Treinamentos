# Roteiro de teste local - v0.0.3

## Objetivo

Validar especificamente que o treinamento e a conclusão agora permanecem dentro do mesmo layout do portal.

## Teste

1. Abrir a solucao no Visual Studio.
2. Restore e Build.
3. Executar o perfil HTTP local.
4. Autenticar com uma conta AD de teste.
5. Em `Meus Treinamentos`, iniciar `Treinamento de Conscientizacao em Segurança da Informação`.
6. Confirmar que continuam visiveis:
   - logo/topbar do portal;
   - `Meus treinamentos`;
   - `Administração`, quando o usuário for `_informatica`;
   - usuário autenticado;
   - versão v0.0.3;
   - botao `Sair`.
7. Confirmar que o conteúdo do treinamento aparece dentro da area central do portal, sem documento HTML independente.
8. Confirmar tempo estimado e cronometro.
9. Responder o quiz e validar que o termo so aparece com 5/5.
10. Registrar o aceite.
11. Confirmar que a tela de conclusão continua usando a mesma topbar/layout do portal.
12. Baixar e conferir o PDF individual.

## Criterio de aprovacao

O fluxo inteiro, de `Meus Treinamentos` ate a conclusão, deve manter a mesma identidade e estrutura de navegacao do portal.
