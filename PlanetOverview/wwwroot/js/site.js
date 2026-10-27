(() => {
    'use strict';
    const form = document.getElementById('country-search');
    if (!form) return;
    const byId = id => document.getElementById(id);
    const text = (id, value) => { byId(id).textContent = value; };
    let currentRequest;
    let exchangeRate;
    let currency;
    let clock;

    async function api(path, signal) {
        const response = await fetch(path, { signal, headers: { Accept: 'application/json' } });
        if (!response.ok) throw new Error(String(response.status));
        return response.json();
    }

    function convert() {
        const amount = byId('currency-amount').valueAsNumber;
        text('currency-result', exchangeRate && Number.isFinite(amount) && amount >= 0
            ? `${(amount * exchangeRate).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} ${currency}` : '');
    }

    function safeUrl(value) {
        try {
            const url = new URL(value);
            return ['http:', 'https:'].includes(url.protocol) ? url.href : null;
        } catch { return null; }
    }

    function renderNews(news) {
        const container = byId('news-list');
        container.replaceChildren();
        for (const article of news.articles || []) {
            const url = safeUrl(article.url);
            if (!url || !article.title || article.title === '[Removed]') continue;
            const card = document.createElement('article');
            card.className = 'bg-white border border-slate-200 rounded-xl p-5';
            const link = document.createElement('a');
            link.href = url;
            link.target = '_blank';
            link.rel = 'noopener noreferrer';
            link.className = 'font-bold text-blue-600 hover:underline';
            link.textContent = article.title;
            const description = document.createElement('p');
            description.className = 'text-sm text-slate-500 mt-2';
            description.textContent = article.description || article.source?.name || '';
            card.append(link, description);
            container.append(card);
        }
        text('news-status', container.childElementCount ? `${container.childElementCount} notícias abaixo` : 'Nenhuma notícia encontrada.');
    }

    async function search(event) {
        event?.preventDefault();
        const query = byId('country-input').value.trim();
        if (!query) return;
        currentRequest?.abort();
        currentRequest = new AbortController();
        const signal = currentRequest.signal;
        clearInterval(clock);
        exchangeRate = undefined;
        currency = undefined;
        byId('news-list').replaceChildren();
        byId('country-flag').hidden = true;
        for (const id of ['capital', 'language', 'population', 'weather', 'local-time', 'currency-rate', 'news-status']) text(id, 'Carregando…');
        for (const id of ['weather-description', 'time-zone', 'currency-result']) text(id, '');
        text('country-source', '');
        text('country-title', `Buscando ${query}`);
        text('updated-at', 'Consultando');
        text('search-status', 'Buscando informações do país…');
        byId('overview').setAttribute('aria-busy', 'true');
        byId('search-button').textContent = 'Buscando…';
        try {
            const country = await api(`/api/country/${encodeURIComponent(query)}`, signal);
            if (signal.aborted) return;
            text('country-title', `Visão geral: ${country.countryName || country.country}`);
            text('country-source', `${country.dataSource} · dados gerais de referência`);
            text('capital', country.capital || 'Sem capital informada');
            text('language', country.language || 'Não informado');
            text('population', Number(country.population).toLocaleString('pt-BR'));
            const flag = safeUrl(country.flagUrl);
            if (flag) { byId('country-flag').src = flag; byId('country-flag').hidden = false; }
            text('search-status', 'Dados gerais carregados. Consultando os demais serviços…');

            let unavailable = 0;
            async function card(action, fallback) {
                try { await action(); }
                catch { if (!signal.aborted) { unavailable++; fallback(); } }
            }
            await Promise.all([
                card(async () => {
                    if (!country.capital) throw new Error('No capital');
                    const weather = await api(`/api/weather/${encodeURIComponent(`${country.capital},${country.countryCode}`)}`, signal);
                    if (signal.aborted) return;
                    text('weather', `${Number(weather.temp).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} °C`);
                    text('weather-description', weather.description);
                }, () => text('weather', 'Clima indisponível')),
                card(async () => {
                    const coordinates = country.capitalCoordinates;
                    if (!coordinates || coordinates.length < 2) throw new Error('No coordinates');
                    const time = await api(`/api/time/coordinates?latitude=${coordinates[0]}&longitude=${coordinates[1]}`, signal);
                    if (signal.aborted) return;
                    const formatter = new Intl.DateTimeFormat('pt-BR', { timeZone: time.timeZone, hour: '2-digit', minute: '2-digit', second: '2-digit' });
                    const initialTime = Date.parse(`${time.localDateTime}Z`);
                    if (!Number.isFinite(initialTime)) throw new Error('Invalid time');
                    // TimeAPI returns a wall-clock value without an offset. Advance it in UTC
                    // to avoid interpreting that value in the browser's own timezone.
                    const started = performance.now();
                    const wallClock = new Intl.DateTimeFormat('pt-BR', { timeZone: 'UTC', hour: '2-digit', minute: '2-digit', second: '2-digit' });
                    const tick = () => text('local-time', wallClock.format(new Date(initialTime + performance.now() - started)));
                    formatter.format(new Date()); // Validate the provider's IANA timezone.
                    tick(); clock = setInterval(tick, 1000);
                    text('time-zone', time.timeZone);
                }, () => text('local-time', 'Horário indisponível')),
                card(async () => {
                    if (!country.currencyCode) throw new Error('No currency');
                    const rate = await api(`/api/currency/${encodeURIComponent(country.currencyCode)}`, signal);
                    if (signal.aborted) return;
                    if (!Number.isFinite(rate.exchangeRate) || rate.exchangeRate <= 0) throw new Error('Invalid rate');
                    currency = rate.targetCurrency; exchangeRate = rate.exchangeRate;
                    text('currency-rate', `1 ${rate.baseCurrency} = ${exchangeRate.toLocaleString('pt-BR', { maximumFractionDigits: 4 })} ${currency}`);
                    convert();
                }, () => text('currency-rate', 'Câmbio indisponível')),
                card(async () => {
                    const news = await api(`/api/news/${encodeURIComponent(country.country)}`, signal);
                    if (!signal.aborted) renderNews(news);
                }, () => text('news-status', 'Notícias indisponíveis'))
            ]);
            if (signal.aborted) return;
            text('updated-at', `Consultado às ${new Date().toLocaleTimeString('pt-BR')}`);
            text('search-status', unavailable ? `Consulta concluída. ${unavailable} serviço(s) indisponível(is); tente novamente mais tarde.` : 'Consulta concluída.');
        } catch (error) {
            if (signal.aborted) return;
            text('search-status', error.message === '404' ? 'País não encontrado ou serviço indisponível. Confira o nome e tente novamente.' : 'Não foi possível consultar o país. Tente novamente.');
            text('country-title', 'Nenhum país carregado');
            text('updated-at', 'Consulta não concluída');
            for (const id of ['capital', 'language', 'population', 'weather', 'local-time', 'currency-rate', 'news-status']) text(id, '—');
        } finally {
            if (!signal.aborted) {
                byId('overview').setAttribute('aria-busy', 'false');
                byId('search-button').textContent = 'Buscar país';
            }
        }
    }
    form.addEventListener('submit', search);
    byId('currency-amount').addEventListener('input', convert);
    search();
})();
