(() => {
    'use strict';
    const t = JSON.parse(document.getElementById('account-translations').textContent);
    const mode = document.body.dataset.accountMode;
    const status = document.getElementById('account-status');
    const form = document.getElementById('account-form');
    document.getElementById('account-language').addEventListener('change', event => event.target.form.requestSubmit());
    const themeBtn = document.getElementById('account-theme');
    const themeIcon = themeBtn.querySelector('i');

    const updateThemeIcon = () => {
        if (!themeIcon) return;
        if (document.documentElement.dataset.theme === 'dark') {
            themeIcon.classList.remove('fa-moon');
            themeIcon.classList.add('fa-sun');
        } else {
            themeIcon.classList.remove('fa-sun');
            themeIcon.classList.add('fa-moon');
        }
    };

    updateThemeIcon(); // Ajusta o ícone ao carregar a página

    themeBtn.addEventListener('click', () => {
        const theme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
        document.documentElement.dataset.theme = theme;
        try { localStorage.setItem('theme', theme); } catch { }
        updateThemeIcon(); // Troca o ícone após o clique
    });
    const fields = document.getElementById('account-fields');
    const value = id => document.getElementById(id).value;
    const message = (text, success = false) => {
        status.textContent = text;
        status.dataset.success = String(success);
        if (text) status.focus();
    };

    async function request(path, method = 'GET', body) {
        const headers = {};
        if (method !== 'GET') {
            const tokenResponse = await fetch('/api/account/csrf', { credentials: 'same-origin', cache: 'no-store' });
            if (!tokenResponse.ok) throw new Error(t.error_request);
            headers['X-CSRF-TOKEN'] = (await tokenResponse.json()).token;
            headers['Content-Type'] = 'application/json';
        }
        const response = await fetch(`/api/account/${path}`, {
            method, headers, credentials: 'same-origin', cache: 'no-store',
            body: body === undefined ? undefined : JSON.stringify(body)
        });
        const data = await response.json().catch(() => null);
        if (!response.ok) {
            if (response.status === 429) throw new Error(t.error_rate);
            if (response.status === 401 && mode === 'profile') {
                location.assign('/account/login');
                throw new Error(t.error_session);
            }
            throw new Error(data?.detail || data?.title || t.error_generic);
        }
        return data;
    }

    async function submit(action) {
        const buttons = [...document.querySelectorAll('button')];
        buttons.forEach(button => { button.disabled = true; });
        message('');
        try { await action(); }
        catch (error) { message(error instanceof TypeError ? t.error_connection : error.message); }
        finally { buttons.forEach(button => { button.disabled = false; }); }
    }

    form.addEventListener('submit', event => {
        event.preventDefault();
        submit(async () => {
            const email = value('account-email').trim();
            const password = value('account-password');
            if (mode === 'forgot') {
                if (password !== value('confirm-password')) throw new Error(t.error_match);
                await request('forgot-password', 'POST', {
                    email, recoveryCode: value('recovery-code').trim(), newPassword: password
                });
                form.reset();
                form.hidden = true;
                message(t.reset_done, true);
                return;
            }
            if (mode === 'register') {
                if (password !== value('confirm-password')) throw new Error(t.error_match);
                await request('register', 'POST', { name: value('account-name').trim(), email, password });
            } else if (mode === 'login') {
                await request('login', 'POST', { email, password });
            } else {
                await request('me', 'PUT', {
                    name: value('account-name').trim(), email, currentPassword: password
                });
                document.getElementById('account-password').value = '';
                message(t.saved, true);
                return;
            }
            location.replace('/');
        });
    });

    if (mode === 'profile') {
        document.getElementById('recovery-form').addEventListener('submit', event => {
            event.preventDefault();
            submit(async () => {
                const result = await request('recovery-code', 'POST', { currentPassword: value('recovery-password') });
                document.getElementById('generated-code').textContent = result.recoveryCode;
                document.getElementById('recovery-result').hidden = false;
                document.getElementById('recovery-password').value = '';
                message(t.code_generated, true);
            });
        });
        request('me').then(user => {
            document.getElementById('account-name').value = user.name;
            document.getElementById('account-email').value = user.email;
            fields.disabled = false;
        }).catch(() => message(t.error_load));
        document.getElementById('logout-button').addEventListener('click', () => submit(async () => {
            await request('logout', 'POST');
            location.assign('/account/login');
        }));
        document.getElementById('delete-form').addEventListener('submit', event => {
            event.preventDefault();
            if (!confirm(t.confirm_delete)) return;
            submit(async () => {
                await request('me', 'DELETE', { currentPassword: value('delete-password') });
                location.assign('/account/register');
            });
        });
    }
})();
