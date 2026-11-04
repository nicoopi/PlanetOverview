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
    const countryInput = byId('country-input');
    const options = byId('country-options');
    let countriesPromise;
    let matches = [];
    let activeOption = -1;
    let suggestionVersion = 0;

    const normalizeCountry = value => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('pt-BR');

    function closeSuggestions() {
        suggestionVersion++;
        options.hidden = true;
        countryInput.setAttribute('aria-expanded', 'false');
        countryInput.removeAttribute('aria-activedescendant');
        activeOption = -1;
        text('country-suggestion-status', '');
    }

    function activateOption(index) {
        activeOption = index;
        for (const [position, option] of Array.from(options.children).entries())
            option.setAttribute('aria-selected', String(position === index));
        if (index >= 0) {
            countryInput.setAttribute('aria-activedescendant', `country-option-${index}`);
            options.children[index]?.scrollIntoView({ block: 'nearest' });
        } else countryInput.removeAttribute('aria-activedescendant');
    }

    function selectCountry(index) {
        const selected = matches[index];
        if (!selected) return;
        countryInput.value = selected.name;
        closeSuggestions();
        form.requestSubmit();
    }

    async function suggestCountries() {
        const version = ++suggestionVersion;
        const query = normalizeCountry(countryInput.value.trim());
        if (!query) { closeSuggestions(); return; }
        try {
            countriesPromise ??= api('/api/country').catch(error => { countriesPromise = undefined; throw error; });
            const countries = await countriesPromise;
            if (version !== suggestionVersion || document.activeElement !== countryInput) return;
            const rank = country => normalizeCountry(country.name).startsWith(query) ? 0
                : normalizeCountry(country.englishName).startsWith(query) ? 1 : 2;
            matches = countries.filter(country => [country.name, country.englishName, country.countryCode]
                .some(value => normalizeCountry(value).includes(query)))
                .sort((a, b) => rank(a) - rank(b) || a.name.localeCompare(b.name, 'pt-BR'))
                .slice(0, 8);
            options.replaceChildren();
            activeOption = -1;
            countryInput.removeAttribute('aria-activedescendant');
            if (!matches.length) {
                const empty = document.createElement('div');
                empty.className = 'country-no-results';
                empty.textContent = 'Nenhum país encontrado.';
                empty.setAttribute('role', 'presentation');
                options.append(empty);
            }
            matches.forEach((country, index) => {
                const option = document.createElement('div');
                option.id = `country-option-${index}`;
                option.className = 'country-option';
                option.setAttribute('role', 'option');
                option.setAttribute('aria-selected', 'false');
                option.dataset.index = String(index);
                const code = document.createElement('span');
                code.className = 'country-code';
                code.textContent = country.countryCode;
                const label = document.createElement('span');
                const name = document.createElement('strong');
                name.textContent = country.name;
                const english = document.createElement('small');
                english.textContent = country.englishName;
                label.append(name, english);
                option.append(code, label);
                options.append(option);
            });
            options.hidden = false;
            countryInput.setAttribute('aria-expanded', 'true');
            text('country-suggestion-status', matches.length ? `${matches.length} sugestões.` : 'Nenhum país encontrado.');
        } catch {
            if (version === suggestionVersion) closeSuggestions();
        }
    }

    countryInput.addEventListener('input', suggestCountries);
    countryInput.addEventListener('focus', suggestCountries);
    countryInput.addEventListener('blur', closeSuggestions);
    options.addEventListener('pointerdown', event => event.preventDefault());
    options.addEventListener('click', event => {
        const option = event.target.closest('[role="option"]');
        if (option) selectCountry(Number(option.dataset.index));
    });
    countryInput.addEventListener('keydown', event => {
        if (event.key === 'Escape' || event.key === 'Tab') { closeSuggestions(); return; }
        if (options.hidden || !matches.length) return;
        if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault();
            const next = event.key === 'ArrowDown' ? (activeOption + 1) % matches.length
                : (activeOption <= 0 ? matches.length - 1 : activeOption - 1);
            activateOption(next);
        } else if (event.key === 'Enter' && activeOption >= 0) {
            event.preventDefault();
            selectCountry(activeOption);
        }
    });

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

    function newsMessage(title, message) {
        const panel = document.createElement('div');
        panel.className = 'news-empty';
        const heading = document.createElement('h3');
        heading.textContent = title;
        const description = document.createElement('p');
        description.textContent = message;
        panel.append(heading, description);
        byId('news-list').replaceChildren(panel);
        byId('country-news').setAttribute('aria-busy', 'false');
        byId('news-count').hidden = true;
    }

    function newsIcon(pathData) {
        const icon = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        icon.setAttribute('viewBox', '0 0 24 24');
        icon.setAttribute('fill', 'none');
        icon.setAttribute('stroke', 'currentColor');
        icon.setAttribute('stroke-width', '1.5');
        icon.setAttribute('aria-hidden', 'true');
        const path = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        path.setAttribute('d', pathData);
        icon.append(path);
        return icon;
    }

    function newsLoading() {
        byId('news-list').replaceChildren();
        byId('country-news').setAttribute('aria-busy', 'true');
        byId('news-count').hidden = true;
        for (let index = 0; index < 3; index++) {
            const skeleton = document.createElement('div');
            skeleton.className = 'news-skeleton';
            skeleton.setAttribute('aria-hidden', 'true');
            byId('news-list').append(skeleton);
        }
    }

    function renderNews(news) {
        const container = byId('news-list');
        container.replaceChildren();
        const countryName = news.countryName || 'País';
        
        text('news-heading', `${countryName} ${window.AppTranslations.emPauta}`);
        
        for (const article of news.articles || []) {
            const url = safeUrl(article.url);
            if (!url || !article.title || article.title === '[Removed]') continue;
            const card = document.createElement('article');
            card.className = 'news-card';
            const link = document.createElement('a');
            link.href = url;
            link.target = '_blank';
            link.rel = 'noopener noreferrer';
            link.className = 'news-card-link';
            const media = document.createElement('div');
            media.className = 'news-media';
            const placeholder = document.createElement('div');
            placeholder.className = 'news-placeholder';
            placeholder.setAttribute('aria-hidden', 'true');
            const icon = newsIcon('M4 4h16v16H4z M8 8h8 M8 12h3 M8 16h3 M14 12h2 M14 16h2');
            placeholder.append(icon);
            media.append(placeholder);
            const imageUrl = safeUrl(article.urlToImage);
            if (imageUrl) {
                const image = document.createElement('img');
                image.alt = '';
                image.loading = 'lazy';
                image.decoding = 'async';
                image.referrerPolicy = 'no-referrer';
                image.addEventListener('load', () => image.classList.add('is-loaded'), { once: true });
                image.addEventListener('error', () => image.remove(), { once: true });
                image.src = imageUrl;
                media.append(image);
            }
            const tag = document.createElement('span');
            tag.className = 'news-tag';
            tag.textContent = countryName;
            media.append(tag);
            const content = document.createElement('div');
            content.className = 'news-content';
            const meta = document.createElement('div');
            meta.className = 'news-meta';
            const source = document.createElement('span');
            source.className = 'news-source';
            source.textContent = article.source?.name || 'Imprensa';
            meta.append(source);
            const published = new Date(article.publishedAt || '');
            if (Number.isFinite(published.getTime())) {
                const separator = document.createElement('span');
                separator.textContent = '·';
                const date = document.createElement('time');
                date.dateTime = published.toISOString();
                date.textContent = published.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' });
                meta.append(separator, date);
            }
            const heading = document.createElement('h3');
            heading.className = 'news-title';
            heading.textContent = article.title;
            const description = document.createElement('p');
            description.className = 'news-description';
            description.textContent = article.description || '';
            const read = document.createElement('div');
            read.className = 'news-read';
            const label = document.createElement('span');
            label.textContent = 'Ler';
            const arrow = newsIcon('M6 18 18 6 M6 6h12v12');
            read.append(label, arrow);
            content.append(meta, heading, description, read);
            link.append(media, content);
            card.append(link);
            container.append(card);
        }
        const count = container.childElementCount;
        byId('country-news').setAttribute('aria-busy', 'false');
        if (count) {
            text('news-count', `${window.AppTranslations.reportagens} ${count}`);
            byId('news-count').hidden = false;
        } else {
            newsMessage('Nenhuma manchete', `Não há notícias disponíveis.`);
        }
    }

    async function search(event) {
        event?.preventDefault();
        const query = byId('country-input').value.trim();
        if (!query) return;
        closeSuggestions();
        currentRequest?.abort();
        currentRequest = new AbortController();
        const signal = currentRequest.signal;
        clearInterval(clock);
        exchangeRate = undefined;
        currency = undefined;
        newsLoading();
        
        text('news-heading', window.AppTranslations.buscando + '...');
        byId('country-flag').hidden = true;
        for (const id of ['capital', 'language', 'population', 'weather', 'local-time', 'currency-rate']) text(id, '—');
        for (const id of ['weather-description', 'time-zone', 'currency-result']) text(id, '');
        text('country-source', '');
        text('country-title', `${window.AppTranslations.buscando} ${query}...`);
        text('updated-at', window.AppTranslations.aguardando);
        text('search-status', window.AppTranslations.statusBuscando);
        byId('overview').setAttribute('aria-busy', 'true');
        byId('search-button').textContent = window.AppTranslations.buscando + '...';
        
        try {
            const country = await api(`/api/country/${encodeURIComponent(query)}`, signal);
            if (signal.aborted) return;
            text('country-title', `${window.AppTranslations.visaoGeral}: ${country.countryName || country.country}`);
            text('news-heading', `${country.countryName || country.country} ${window.AppTranslations.emPauta}`);
            text('country-source', `${country.dataSource}`);
            text('capital', country.capital || '—');
            text('language', country.language || '—');
            text('population', Number(country.population).toLocaleString('pt-BR'));
            const flag = safeUrl(country.flagUrl);
            if (flag) { byId('country-flag').src = flag; byId('country-flag').hidden = false; }
            text('search-status', 'Dados carregados.');

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
                }, () => text('weather', '—')),
                card(async () => {
                    const coordinates = country.capitalCoordinates;
                    if (!coordinates || coordinates.length < 2) throw new Error('No coordinates');
                    const time = await api(`/api/time/coordinates?latitude=${coordinates[0]}&longitude=${coordinates[1]}`, signal);
                    if (signal.aborted) return;
                    const formatter = new Intl.DateTimeFormat('pt-BR', { timeZone: time.timeZone, hour: '2-digit', minute: '2-digit', second: '2-digit' });
                    const initialTime = Date.parse(`${time.localDateTime}Z`);
                    if (!Number.isFinite(initialTime)) throw new Error('Invalid time');
                    const started = performance.now();
                    const wallClock = new Intl.DateTimeFormat('pt-BR', { timeZone: 'UTC', hour: '2-digit', minute: '2-digit', second: '2-digit' });
                    const tick = () => text('local-time', wallClock.format(new Date(initialTime + performance.now() - started)));
                    formatter.format(new Date()); 
                    tick(); clock = setInterval(tick, 1000);
                    text('time-zone', time.timeZone);
                }, () => text('local-time', '—')),
                card(async () => {
                    if (!country.currencyCode) throw new Error('No currency');
                    const rate = await api(`/api/currency/${encodeURIComponent(country.currencyCode)}`, signal);
                    if (signal.aborted) return;
                    if (!Number.isFinite(rate.exchangeRate) || rate.exchangeRate <= 0) throw new Error('Invalid rate');
                    currency = rate.targetCurrency; exchangeRate = rate.exchangeRate;
                    text('currency-rate', `1 ${rate.baseCurrency} = ${exchangeRate.toLocaleString('pt-BR', { maximumFractionDigits: 4 })} ${currency}`);
                    convert();
                }, () => text('currency-rate', '—')),
                card(async () => {
                    const news = await api(`/api/news/${encodeURIComponent(country.countryCode)}`, signal);
                    if (!signal.aborted) renderNews(news);
                }, () => {
                    newsMessage('Erro', 'As notícias estão indisponíveis.');
                })
            ]);
            if (signal.aborted) return;
            text('updated-at', `${window.AppTranslations.consultado} ${new Date().toLocaleTimeString('pt-BR')}`);
            text('search-status', '');
        } catch (error) {
            if (signal.aborted) return;
            text('search-status', 'Erro na consulta.');
            text('country-title', 'Nenhum país carregado');
            newsMessage('Erro', 'Não foi possível carregar as informações.');
            text('updated-at', '—');
        } finally {
            if (!signal.aborted) {
                byId('overview').setAttribute('aria-busy', 'false');
                byId('search-button').textContent = window.AppTranslations.botaoBuscar;
            }
        }
    }
    form.addEventListener('submit', search);
    byId('currency-amount').addEventListener('input', convert);
    search();
})();

document.addEventListener("DOMContentLoaded", () => {
    const themeToggle = document.getElementById("theme-toggle");
    const currentTheme = localStorage.getItem("theme") || "light";
    document.documentElement.setAttribute("data-theme", currentTheme);
    
    if(themeToggle) {
        themeToggle.innerHTML = currentTheme === "dark" ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>';
        
        themeToggle.addEventListener("click", () => {
            const isDark = document.documentElement.getAttribute("data-theme") === "dark";
            const newTheme = isDark ? "light" : "dark";
            document.documentElement.setAttribute("data-theme", newTheme);
            localStorage.setItem("theme", newTheme);
            themeToggle.innerHTML = newTheme === "dark" ? '<i class="fas fa-sun"></i>' : '<i class="fas fa-moon"></i>';
        });
    }
});