# Planet Overview — entrega para o grupo

Este pacote contém o código atualizado, com API interna, cadastro, login/logout,
consulta/edição/exclusão do próprio perfil e recuperação por código, sem e-mail.
Mantém a versão recente do site, os idiomas português/inglês e o tema escuro.

## Executar no Windows

1. Extraia o ZIP inteiro para uma pasta. Não execute de dentro do ZIP.
2. Instale o **SDK .NET 8** (não apenas o Runtime), se necessário.
3. Para ativar as consultas externas, copie
   `PlanetOverview/appsettings.example.json` para
   `PlanetOverview/appsettings.Development.json` e preencha as quatro chaves em
   `ApiConfigs`. Peça as chaves ao responsável pelo projeto por um canal privado,
   ou configure chaves próprias. Não publique esse arquivo no GitHub.
4. Dê dois cliques em **INICIAR.cmd**. Também é possível executar `./start.ps1`
   no PowerShell. Se a configuração local não existir, o iniciador cria uma cópia
   do exemplo, com as chaves vazias.
5. Aguarde a restauração dos pacotes e a mensagem `Now listening on`.
6. Abra **http://localhost:5049** no navegador. Mantenha o terminal aberto.

É necessária internet na primeira execução para restaurar os pacotes NuGet.
As consultas externas, o Tailwind e os ícones da página principal também usam
internet. Se a porta 5049 estiver ocupada, encerre a outra cópia do projeto.

## Conferir as funções

1. Clique em **Entrar → Criar conta**. Use uma senha com pelo menos oito caracteres,
   incluindo maiúscula, minúscula e número.
2. O cadastro inicia a sessão e abre a página principal.
3. Clique em **Entrar no meu perfil** para consultar ou editar os dados. A edição
   exige a senha atual. Use **Ir para o site** para voltar.
4. No perfil, abra **Código de recuperação**, confirme a senha e guarde o código
   gerado. Ele permite recuperar acesso sem envio de e-mail.
5. Saia e teste **Esqueci minha senha**, informando e-mail, código e nova senha.
   O código só vale uma vez; depois de entrar, gere outro para guardar.
6. Para excluir sua conta, abra a opção correspondente no perfil e confirme a senha.
7. Com as chaves configuradas, pesquise países e confira clima, horário, câmbio e
   notícias. Cada serviço depende da disponibilidade e da cota do provedor.

## Banco de dados e configurações

O SQLite será criado automaticamente em `PlanetOverview/App_Data/accounts.db`.
Cada computador terá suas próprias contas; não há uma conta pronta no pacote.
Os dados persistem ao reiniciar. As senhas são guardadas como hash.

O pacote não contém chaves de API, banco de usuários, histórico Git nem arquivos
de compilação. O código restaura as dependências ao iniciar.
Cadastro, login e CRUD não dependem das chaves externas; as consultas de países,
clima, notícias e câmbio precisam delas.

## Testes e estrutura da API

Com SDK .NET 8 e Python 3:

```powershell
dotnet build PlanetOverview/PlanetOverview.csproj
python tests/test_account_api.py
```

Os sete testes verificam CRUD, autenticação, persistência, validações, sessões,
proteção CSRF, limites de tentativas e recuperação de senha com código de uso único.
O arquivo `PlanetOverview/PlanetOverview.http` contém exemplos das requisições.
O `README.md` descreve os endpoints e a arquitetura.

## Mensagem sugerida para o grupo

Segue o Planet Overview atualizado, com cadastro/login e CRUD de contas integrados
ao novo visual. Extraiam o ZIP e leiam LEIA-ME-GRUPO.md. No Windows, iniciem com
INICIAR.cmd (requer SDK .NET 8). O banco SQLite é criado automaticamente.
Para as consultas externas, precisamos configurar as chaves das APIs no arquivo
local indicado no guia; elas não estão no ZIP.
