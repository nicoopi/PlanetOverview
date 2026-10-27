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
`ApiConfigs__WeatherApiKey` e `ApiConfigs__CurrencyApiKey`. Não coloque chaves
no JavaScript, nas views ou em arquivos publicados em `wwwroot`.

### Integração da tela

A tela busca inicialmente Brasil e permite novas consultas pelo botão ou Enter.
O navegador acessa somente endpoints `/api` da própria aplicação para os dados:

| Endpoint | Uso |
| --- | --- |
| `/api/country/{pais}` | Nome em português/inglês, capital, idioma, população, moeda e coordenadas |
| `/api/weather/{capital},{codigoPais}` | Clima da capital em °C |
| `/api/time/coordinates?latitude=...&longitude=...` | Horário e fuso IANA da capital |
| `/api/time?timeZone=America/Sao_Paulo` | Consulta direta por fuso IANA |
| `/api/currency/{moeda}` | Taxa de 1 USD para a moeda local |
| `/api/news/{pais}` | Até seis notícias relacionadas ao país |

O conversor multiplica o valor em USD pela taxa carregada, sem uma nova chamada
por digitação. Países com vários fusos usam o horário da capital. As consultas
complementares são paralelas, têm timeout e falham independentemente; uma nova
busca cancela a anterior no navegador para impedir resultados misturados.

### Dados gerais e disponibilidade

O REST Countries desativou a API `/v3.1` usada na versão original do projeto.
A busca agora usa uma referência local com 250 países/territórios, incorporada
na compilação e identificada na tela. Esses dados gerais, especialmente população,
não são em tempo real. Veja a origem, revisão e licença em
[PlanetOverview/Data/README.md](PlanetOverview/Data/README.md).

Clima, câmbio, notícias e horário usam os serviços online. A disponibilidade e a
frequência de atualização dependem do plano e das condições de cada provedor.
O estilo atual ainda carrega Tailwind e Font Awesome por CDN.
