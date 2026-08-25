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
├── backend/
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── appsettings.json
│   └── Program.cs
├── frontend/
│   ├── assets/
│   │   ├── css/
│   │   ├── js/
│   │   └── img/
│   └── index.html
├── docs/
│   └── Relatorio_de_Viabilidade_PlanetOverview.pdf
└── README.md
