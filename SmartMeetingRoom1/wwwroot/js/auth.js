// ======== /wwwroot/js/auth.js (loop-safe) ========
'use strict';

const API_BASE = "/api";
const TOKEN_KEY = "smr_access_token";
const RTOKEN_KEY = "smr_refresh_token";
const ADMIN_HOME = "/admin.html";
const USER_HOME = "/index.html";

// ---------- storage helpers ----------
function saveTokens(json, remember) {
    const access = json.AccessToken || json.accessToken || json.token || json.jwt;
    const refresh = json.RefreshToken || json.refreshToken || null;
    if (!access) throw new Error("Token not found in response.");
    const S = remember ? localStorage : sessionStorage;
    S.setItem(TOKEN_KEY, access);
    if (refresh) S.setItem(RTOKEN_KEY, refresh);
    return access;
}

function readToken() {
    return (
        localStorage.getItem(TOKEN_KEY) ||
        sessionStorage.getItem(TOKEN_KEY) || ""
    );
}

function clearTokens() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(RTOKEN_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(RTOKEN_KEY);
}

// ---------- API ----------
async function fetchMe(token) {
    const res = await fetch(`${API_BASE}/auth/me`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    if (!res.ok) throw new Error("Unauthorized");
    return res.json(); // { id, userName, email, roles: [...] }
}

// ---------- redirect helpers (loop guard) ----------
function safeRedirect(target) {
    try {
        const guardKey = "smr_redirect_guard";
        const now = Date.now();
        const prev = JSON.parse(sessionStorage.getItem(guardKey) || "{}");
        if (prev.target === target && now - (prev.time || 0) < 2000) {
            // same redirect within 2s -> drop it (prevents loops)
            return;
        }
        sessionStorage.setItem(guardKey, JSON.stringify({ target, time: now }));
    } catch { }
    window.location.href = target;
}

function redirectByRoles(roles) {
    const isAdmin = Array.isArray(roles) && roles.some(r => String(r).toLowerCase() === "admin");
    safeRedirect(isAdmin ? ADMIN_HOME : USER_HOME);
}

// ---------- page helpers ----------
function path() { return location.pathname.toLowerCase(); }
function isLoginPage() { return path().endsWith("/login.html"); }
function isAdminPage() { return path().endsWith("/admin.html"); }
function isIndexPage() { return path() === "/" || path().endsWith("/index.html"); }

// ---------- wire up login form & session checks ----------
document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("loginForm");
    const emailEl = document.getElementById("email");
    const passEl = document.getElementById("password");
    const rememberEl = document.getElementById("rememberMe");
    const errEl = document.getElementById("error");

    // Handle manual sign-in
    if (form) {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errEl) errEl.textContent = "";
            const email = (emailEl?.value || "").trim();
            const password = passEl?.value || "";
            const remember = !!(rememberEl && rememberEl.checked);

            try {
                const res = await fetch(`${API_BASE}/auth/login`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ userNameOrEmail: email, password })
                });
                if (!res.ok) throw new Error(`Login failed (${res.status})`);

                const data = await res.json();
                const token = saveTokens(data, remember);

                // ✅ Only redirect if /me verifies the token
                try {
                    const me = await fetchMe(token);
                    redirectByRoles(me.roles || []);
                } catch {
                    clearTokens();
                    if (errEl) errEl.textContent = "Login succeeded but the session was not accepted by the server.";
                }
            } catch (err) {
                if (errEl) errEl.textContent = err.message || "Login failed.";
                else alert(err.message || "Login failed.");
            }
        });
    }

    // Auto-check session:
    // - On login page: if token is valid, redirect by role; if invalid, clear & stay (NO redirect).
    // - On other pages: do nothing here (page-specific scripts can guard).
    (async () => {
        const token = readToken();
        if (!token) return;

        if (isLoginPage()) {
            try {
                const me = await fetchMe(token);
                redirectByRoles(me.roles || []);
            } catch {
                clearTokens(); // stay on login without redirect -> no loops
            }
        }
    })();
});
// ======== end auth.js ========
