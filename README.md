# 🌍 PlanetOverview — Hub Centralizado de Informações Internacionais

> Aplicação web concebida para integrar e exibir um panorama rápido ("raio-X") sobre qualquer país do mundo em tempo real, consolidando notícias, clima, cotações cambiais, fusos horários e dados geopolíticos em uma única interface intuitiva.

![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)
![Back-end](https://img.shields.io/badge/Back--end-C%23-512BD4)
![Front-end](https://img.shields.io/badge/Front--end-HTML5%20%7C%20CSS3%20%7C%20JavaScript-F7DF1E)
![Universidade](https://img.shields.io/badge/Universidade-Positivo-005696)
![NewsAPI](https://img.shields.io/badge/API-NewsAPI-blue)
![TimeAPI](https://img.shields.io/badge/API-TimeApi.io-orange)
![CurrencyFreaks](https://img.shields.io/badge/API-CurrencyFreaks-green)
![RestCountries](https://img.shields.io/badge/API-RestCountries-red)
![OpenWeather](https://img.shields.io/badge/API-OpenWeather-lightblue)

---

## 📌 Sobre o Projeto

O **PlanetOverview** é desenvolvido como projeto interdisciplinar integrando as disciplinas de **Desenvolvimento de Software Visual** e **Elicitação de Requisitos** do curso de Engenharia de Software da **Universidade Positivo**.

O objetivo principal é eliminar a fragmentação de consultas na internet. Em vez de abrir múltiplos sites para checar notícias locais, conversores de moedas, clima e fusos horários de uma nação, o usuário encontra todas essas informações centralizadas em uma tela ágil, responsiva e de fácil leitura.

---

## ✨ Funcionalidades Principais

* 🔍 **Consulta Rápida por País:** Busca direta para obter a ficha completa de qualquer nação.
* 📰 **Manchetes Internacionais:** Principais notícias do momento filtradas por localidade.
* 💱 **Câmbio e Conversão:** Exibição da moeda local com cotação atualizada em tempo real e ferramenta de conversão para moeda de preferência.
* ⏰ **Fuso Horário & Horário Local:** Relógio sincronizado com a região selecionada.
* 🌤️ **Clima Atual:** Condições meteorológicas em tempo real da região buscada.
* 📊 **Dados Gerais:** Ficha síntese com idioma oficial, população e indicadores essenciais.
* 🛡️ **Resiliência e Tratamento de Falhas:** Isolamento no back-end para manter a interface utilizável mesmo em caso de instabilidade em serviços de terceiros.

---

## 🛠️ Tecnologias Utilizadas

### **Back-end**
* **Linguagem:** C#
* **Framework:** ASP.NET Core Web API
* **Função:** Centralização das regras de negócio, consumo/encapsulamento de chaves de APIs externas, padronização de dados e tratamento de erros.

### **Front-end**
* **Linguagens:** HTML5, CSS3 e JavaScript (Vanilla)
* **Design:** Interface responsiva voltada à agilidade de visualização em desktops e dispositivos móveis.

### **Serviços Externos / APIs Integradas**
A orquestração dos dados é feita no back-end consumindo os seguintes serviços em tempo real:
* **[NewsAPI](https://newsapi.org/):** Obtenção de manchetes e notícias recentes filtradas por país.
* **[TimeApi.io](https://timeapi.io/):** Sincronização do relógio e fuso horário (com associação direta ao endpoint `AvailableTimeZones`).
* **[CurrencyFreaks](https://currencyfreaks.com/):** Consulta de taxas de câmbio e cotação de moedas.
* **[RestCountries](https://restcountries.com/):** Consumo de dados geopolíticos, demográficos e informações gerais sobre a nação.
* **[OpenWeatherAPI](https://openweathermap.org/api):** Consulta de dados climáticos e meteorológicos atuais da região.

---

## 📂 Estrutura do Repositório (Sugerida)

```text
PlanetOverview/
│── bin/
│── Controllers/
│── Models/
│── obj
│── Properties
│── Views
├── wwroot
├── appsettings.Development.json
├── appsettings.json
├── PlanetOverview.csproj
├── Program.cs
└── README.md
```

## Executar localmente

Requer o SDK .NET 8. No PowerShell, na raiz do projeto:

```powershell
Copy-Item PlanetOverview/appsettings.example.json PlanetOverview/appsettings.Development.json
# Preencha ApiConfigs no arquivo local com suas chaves.
./start.ps1
```

Se `appsettings.Development.json` já estiver configurado, execute somente
`./start.ps1`. Abra http://localhost:5049. O script usa o SDK local em
`%LOCALAPPDATA%/PlanetOverview/dotnet`, quando disponível, ou o `dotnet` do PATH.

As chaves ficam somente no servidor; `appsettings.Development.json` é ignorado
pelo Git. Em produção, use as variáveis `ApiConfigs__NewsApiKey`,
`ApiConfigs__WeatherApiKey`, `ApiConfigs__CurrencyApiKey` e
`ApiConfigs__CountriesApiKey`. Não coloque chaves
no JavaScript, nas views ou em arquivos publicados em `wwwroot`.

### Integração da tela

A tela busca inicialmente Brasil e permite novas consultas pelo botão ou Enter.
O navegador acessa somente endpoints `/api` da própria aplicação para os dados:

| Endpoint | Uso |
| --- | --- |
| `/api/country` | Lista de 250 países/territórios para sugestões na busca, com nomes em português e inglês e código ISO |
| `/api/country/{pais}` | Nome em português/inglês, capital, idioma, população, moeda e coordenadas |
| `/api/weather/{capital},{codigoPais}` | Clima da capital em °C |
| `/api/time/coordinates?latitude=...&longitude=...` | Horário e fuso IANA da capital |
| `/api/time?timeZone=America/Sao_Paulo` | Consulta direta por fuso IANA |
| `/api/currency/{moeda}` | Taxa de 1 BRL para a moeda local |
| `/api/news/{pais}` | Até seis notícias em português com o nome do país no título; aceita também o código ISO (BR, JP, PT) |

O conversor sempre recebe valores em reais (BRL) e multiplica pela taxa de BRL
para a moeda do país, sem uma nova chamada por digitação. Como o provedor fornece
taxas com uma base comum, o backend calcula `taxaDestino / taxaBRL` usando as duas
cotações da mesma resposta. Para BRL → BRL, retorna 1 sem consultar o provedor.
Países com vários fusos usam o horário da capital. As consultas
complementares são paralelas, têm timeout e falham independentemente; uma nova
busca cancela a anterior no navegador para impedir resultados misturados.

O campo de país mostra até oito sugestões enquanto o usuário digita, ignorando
acentos e aceitando nomes em português, inglês ou códigos ISO. O catálogo é
carregado uma vez por página, sem consultar serviços externos a cada tecla.
Selecione por clique ou pelas setas e Enter para buscar; Escape e Tab fecham a
lista. A busca manual continua disponível se o catálogo não puder ser carregado.

O resumo do país exibe apenas os seis cards de dados gerais, clima, horário e
conversão. As notícias ficam na seção inferior, com uma reportagem principal,
uma segunda manchete e os demais cards em uma lista compacta e responsiva.

As notícias usam os nomes canônicos do país em português e inglês como frases
exatas, com busca restrita ao título, validação adicional de correspondência e
remoção de URLs repetidas. A seção identifica o país selecionado e exibe imagem,
fonte, data e link da reportagem. Imagens indisponíveis recebem uma ilustração
neutra. Sem manchetes, a API retorna uma lista vazia e a tela informa esse estado;
falhas do provedor retornam HTTP 503, mantendo os demais cards disponíveis.

### Dados gerais e disponibilidade

O REST Countries desativou a API `/v3.1` usada na versão original do projeto.
A busca e o autocomplete resolvem os nomes usando uma referência local com 250
países/territórios. Quando `ApiConfigs:CountriesApiKey` está preenchida, o servidor
consulta `https://api.restcountries.com/countries/v5/codes.alpha_2/{ISO}` com
autenticação Bearer e usa os dados retornados pela API v5. As respostas ficam em
cache por quatro horas para poupar a cota, inclusive nas consultas de notícias.
A fonte aparece como `REST Countries API v5` na tela. Sem chave ou durante uma
falha do provedor, a aplicação usa a referência local e identifica essa fonte;
falhas são armazenadas por um minuto antes de permitir nova consulta.
Os dados da base local, especialmente população, não são em tempo real.
Veja a origem, revisão e licença em
[PlanetOverview/Data/README.md](PlanetOverview/Data/README.md).

Clima, câmbio, notícias e horário usam os serviços online. A disponibilidade e a
frequência de atualização dependem do plano e das condições de cada provedor.
O estilo atual ainda carrega Tailwind e Font Awesome por CDN.


### API interna: contas de usuários e login

O CRUD interno agora gerencia contas. Na página principal, clique em **Entrar**.
As telas estão em `/account/register`, `/account/login` e `/account/profile`.
Cada pessoa pode consultar, editar e excluir somente a própria conta.
Após cadastro ou login, o usuário é levado à página principal. A edição fica
em Minha conta, com um botão Ir para o site para retornar à exploração.

| Operação | Método e endpoint | Comportamento |
| --- | --- | --- |
| Create | `POST /api/account/register` | Cadastra nome, e-mail e senha; retorna 201 e inicia sessão |
| Read | `GET /api/account/me` | Retorna o próprio perfil |
| Update | `PUT /api/account/me` | Atualiza nome/e-mail e, opcionalmente, senha |
| Delete | `DELETE /api/account/me` | Exclui a própria conta e encerra a sessão |
| Login | `POST /api/account/login` | Valida e-mail/senha e inicia sessão |
| Logout | `POST /api/account/logout` | Encerra a sessão atual |
| CSRF | `GET /api/account/csrf` | Retorna o token para uma operação de escrita |

Cadastro recebe `name`, `email` e `password`; login recebe `email` e `password`.
Atualização recebe `name`, `email`, `currentPassword` e `newPassword` opcional
(omita ou envie null para manter a senha). Exclusão recebe `currentPassword`.
O perfil retorna apenas `id`, `name`, `email` e `createdAt`, nunca senha ou hash.
O nome aceita de 2 a 100 caracteres. A senha aceita de 8 a 128 caracteres e
exige maiúscula, minúscula e número.

A autenticação usa ASP.NET Core Identity, hash de senha e cookie HttpOnly
(Secure em produção). O cookie não é persistente e o ticket dura 8 horas,
com renovação durante o uso. Escritas exigem cookie e `X-CSRF-TOKEN`: consulte
`/api/account/csrf` antes e preserve os cookies. Obtenha novo token após cadastro,
login ou logout. As telas cuidam disso automaticamente.
Exemplos estão em [PlanetOverview/PlanetOverview.http](PlanetOverview/PlanetOverview.http).

Dados inválidos retornam 400; falta de autenticação ou login recusado, 401;
e-mail duplicado, 409; excesso de tentativas, 429. Cadastro, login, atualização
e exclusão compartilham limite de 10 tentativas por minuto por IP. Cinco senhas
incorretas bloqueiam a autenticação da conta por cinco minutos. Atualizações
invalidam outras sessões; exclusão invalida todas. Não existe listagem pública
de usuários nem acesso ao perfil de outra pessoa.

O SQLite é criado em `PlanetOverview/App_Data/accounts.db`, fora de `wwwroot`
e ignorado pelo Git. Para outro arquivo, configure `ConnectionStrings__Accounts`
com `Data Source=caminho/accounts.db` e crie previamente o diretório.
O esquema inicial usa `EnsureCreated`; futuras alterações em bancos existentes
precisam de migrations.

O CRUD de países salvos foi substituído. Os bancos anteriores são preservados,
mas não são usados pela nova API. A exploração de países continua pública;
o login protege o perfil. A recuperação de senha usa um código de recuperação, sem envio de e-mail.
A confirmação de e-mail não está incluída.

Verificação (SDK .NET 8 e Python 3):

```powershell
dotnet build PlanetOverview/PlanetOverview.csproj
python tests/test_account_api.py
```

Os testes usam bancos temporários e cobrem CRUD, login/logout, persistência,
hash, cookies, CSRF, isolamento, invalidação de sessões, rollback e limites.
Se necessário, substitua `dotnet` por
`& "$env:LOCALAPPDATA/PlanetOverview/dotnet/dotnet.exe"`.

### Esqueci minha senha (sem e-mail)

No login, use **Esqueci minha senha** e informe e-mail, código de recuperação e
nova senha. Gere e guarde o código previamente em **Entrar no meu perfil →
Código de recuperação**, confirmando sua senha atual. O código é mostrado apenas
na resposta de geração, fica armazenado como hash e vale uma única vez. Gerar
outro invalida o anterior. Sem esse código, não é possível redefinir a senha
apenas conhecendo o e-mail da conta.

- `POST /api/account/recovery-code` (autenticado): recebe `currentPassword` e
  devolve `recoveryCode`. Guarde o valor; ele não é retornado por `/me`.
- `POST /api/account/forgot-password` (público, com proteção CSRF e limite de
  tentativas): recebe `email`, `recoveryCode` e `newPassword`. Retorna 204,
  consome o código e invalida as sessões existentes. Entre novamente com a nova
  senha e gere outro código no perfil.

O perfil permite editar os dados cadastrais e não mostra o campo de nova senha.
O banco de contas existente é mantido: os códigos usam a tabela de tokens do Identity.
