# Dados gerais dos países

`countries.json` contém os campos usados pelo painel, extraídos da base pública
REST Countries v3.1, com 250 países/territórios. Não contém informações em tempo
real. População e informações geopolíticas podem estar desatualizadas.

- Origem: https://github.com/restcountries/restcountries/blob/bfadee4f951682c29970e53677707bc558e80b74/src/main/resources/countriesV3.1.json
- Revisão da origem: `bfadee4f951682c29970e53677707bc558e80b74`
- Importação: 27/10/2026.
- Licença da origem: Mozilla Public License 2.0, preservada em `LICENSE.restcountries`.
- Alteração: projeção apenas dos campos consumidos pelo painel; valores preservados.

O endpoint público `/v3.1` foi desativado. A base é incorporada ao assembly para
permitir buscas sem exigir uma quarta chave e sem chamadas a essa versão inativa.
Clima, notícias, câmbio e horário continuam vindo dos respectivos serviços online.
Para atualizar a referência, substitua o JSON por dados compatíveis, atualize esta
proveniência e recompile a aplicação.
