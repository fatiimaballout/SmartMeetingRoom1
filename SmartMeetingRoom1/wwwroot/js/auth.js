// ======== /wwwroot/js/auth.js (safe guards + register support) ========
'use strict';

const API_BASE = '/api';
const TOKEN_KEY = 'smr_access_token';
const RTOKEN_KEY = 'smr_refresh_token';

const LOGIN_PAGE = '/login.html';
const ADMIN_HOME = '/admin.html';
const EMPLOYEE_HOME = '/index.html';
const GUEST_HOME = '/guest.html';

// replace redirectByRoles with:
function redirectByRoles(roles) {
    const r = (roles || []).map(x => String(x).toLowerCase());
    if (r.includes('admin')) return safeRedirect(ADMIN_HOME);
    if (r.includes('user')) return safeRedirect(GUEST_HOME);   // <- "User" (guest) wins
    return safeRedirect(EMPLOYEE_HOME);                          // employee/others
}


// ---------- storage ----------
function saveTokens(json, remember) {
    const access = json.AccessToken || json.accessToken || json.token || json.jwt;
    const refresh = json.RefreshToken || json.refreshToken || null;
    if (!access) throw new Error('Token not found in response.');
    const S = remember ? localStorage : sessionStorage;
    S.setItem(TOKEN_KEY, access);
    if (refresh) S.setItem(RTOKEN_KEY, refresh);
    return access;
}

function readToken() {
    return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY) || '';
}

function clearTokens() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(RTOKEN_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(RTOKEN_KEY);
}

// ---------- api ----------
async function fetchMe(token) {
    const res = await fetch(`${API_BASE}/auth/me`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    if (!res.ok) throw new Error('Unauthorized');
    return res.json(); // -> { id, userName, email, roles: [...] }
}
(async function guardDashboardRole() {
    const t = localStorage.getItem('smr_access_token') || sessionStorage.getItem('smr_access_token') || '';
    if (!t) return; // your signed-in guard handles redirect to login
    try {
        const me = await fetch('/api/auth/me', { headers: { Authorization: 'Bearer ' + t } }).then(r => r.json());
        const r = (me.roles || me.Roles || []).map(x => String(x).toLowerCase());
        if (r.includes('user') && !r.includes('employee') && !r.includes('admin')) {
            location.href = '/guest.html';
        }
    } catch { }
})();

// ---------- small utils ----------
function pth() { return location.pathname.toLowerCase(); }
function isLogin() { return pth().endsWith('/login.html'); }
function isRegister() { return pth().endsWith('/register.html'); }
function isAdmin() { return pth().endsWith('/admin.html'); }
function isIndex() { return pth() === '/' || pth().endsWith('/index.html'); }

function safeRedirect(target) {
    try {
        const guardKey = 'smr_redirect_guard';
        const now = Date.now();
        const prev = JSON.parse(sessionStorage.getItem(guardKey) || '{}');
        if (prev.target === target && now - (prev.time || 0) < 2000) return; // avoid loops
        sessionStorage.setItem(guardKey, JSON.stringify({ target, time: now }));
    } catch { /* ignore */ }
    location.href = target;
}

function toFriendlyRole(r) { return String(r) === 'User' ? 'Guest' : String(r); }


function putUserName(me) {
    const el = document.getElementById('navUserName') || document.getElementById('meBadge');
    if (el) el.textContent = me?.userName ?? me?.email ?? `User #${me?.id ?? ''}`;
}

// ---------- page boot ----------
document.addEventListener('DOMContentLoaded', () => {
    const token = readToken();

    // ----- Login page -----
    if (isLogin()) {
        const form = document.getElementById('loginForm');
        const email = document.getElementById('email');
        const pass = document.getElementById('password');
        const remember = document.getElementById('rememberMe');
        const errEl = document.getElementById('error');

        // If already authenticated, go home (by role)
        (async () => {
            if (!token) return;
            try {
                const me = await fetchMe(token);
                redirectByRoles(me.roles || []);
            } catch {
                clearTokens(); // stay on login
            }
        })();

        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                if (errEl) errEl.textContent = '';
                try {
                    const res = await fetch(`${API_BASE}/auth/login`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            userNameOrEmail: (email?.value || '').trim(),
                            password: pass?.value || ''
                        })
                    });
                    if (!res.ok) throw new Error(`Login failed (${res.status})`);
                    const data = await res.json();
                    const saved = saveTokens(data, !!(remember && remember.checked));

                    // verify before redirect
                    const me = await fetchMe(saved);
                    redirectByRoles(me.roles || []);
                } catch (err) {
                    if (errEl) errEl.textContent = err.message || 'Login failed.';
                    else alert(err.message || 'Login failed.');
                }
            });
        }
        return;
    }

    // ----- Register page (no auto-redirects) -----
    if (isRegister()) {
        const form = document.getElementById('registerForm');
        const errEl = document.getElementById('error');

        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                if (errEl) errEl.textContent = '';

                const fullName = (document.getElementById('fullName')?.value || '').trim();
                const email = (document.getElementById('email')?.value || '').trim();
                const password = document.getElementById('password')?.value || '';
                const confirm = document.getElementById('confirm')?.value || '';
                if (password !== confirm) {
                    errEl && (errEl.textContent = 'Passwords do not match.');
                    return;
                }

                try {
                    const res = await fetch(`${API_BASE}/auth/register`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            fullName, email, password, confirmPassword: confirm, userName: email
                            // server should default role to "User" (Guest)
                        })
                    });

                    if (!res.ok) {
                        const ct = res.headers.get('content-type') || '';
                        let msg = '';
                        if (ct.includes('application/json')) {
                            const j = await res.json().catch(() => ({}));
                            if (j?.errors) {
                                msg = Object.keys(j.errors).map(k => `${k}: ${[].concat(j.errors[k]).join(' ')}`).join(' | ');
                            } else if (j.title || j.message) msg = j.title || j.message;
                        } else {
                            msg = await res.text().catch(() => '');
                        }
                        throw new Error(msg || `Register failed (${res.status})`);
                    }

                    // If API returns tokens, use them; else go to login.
                    const data = await res.json().catch(() => ({}));
                    if (data && (data.token || data.accessToken || data.jwt)) {
                        saveTokens(data, true);
                        safeRedirect(USER_HOME);
                    } else {
                        safeRedirect(LOGIN_PAGE);
                    }
                } catch (ex) {
                    errEl && (errEl.textContent = ex.message || 'Registration failed.');
                }
            });
        }
        return;
    }

    // ----- Admin page guard -----
    if (isAdmin()) {
        (async () => {
            if (!token) { safeRedirect(LOGIN_PAGE); return; }
            try {
                const me = await fetchMe(token);
                putUserName(me);
                const roles = me.roles || [];
                const ok = roles.some(r => String(r).toLowerCase() === 'admin');
                if (!ok) { safeRedirect(USER_HOME); return; }

                // wire logout if present
                const signOut = document.getElementById('btnSignOut');
                if (signOut) {
                    signOut.addEventListener('click', (e) => {
                        e.preventDefault();
                        clearTokens();
                        safeRedirect(LOGIN_PAGE);
                    });
                }
            } catch {
                clearTokens();
                safeRedirect(LOGIN_PAGE);
            }
        })();
        return;
    }

    // ----- Index (and other pages): set badge if possible, no redirects -----
    if (token) {
        (async () => {
            try { putUserName(await fetchMe(token)); } catch { /* ignore */ }
        })();
    }

    // Optional global logout button
    const signOut = document.getElementById('btnSignOut');
    if (signOut) {
        signOut.addEventListener('click', (e) => {
            e.preventDefault();
            clearTokens();
            safeRedirect(LOGIN_PAGE);
        });
    }
});
// ======== end auth.js ========
