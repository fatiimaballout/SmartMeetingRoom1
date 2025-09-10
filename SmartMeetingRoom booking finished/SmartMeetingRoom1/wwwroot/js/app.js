const TOKEN_KEY = "smr_token";

export function getToken() {
    return localStorage.getItem(TOKEN_KEY);
}

export async function apiFetch(url, options = {}) {
    const token = getToken();
    const headers = Object.assign({ "Content-Type": "application/json" }, options.headers || {});
    if (token) headers["Authorization"] = `Bearer ${token}`;
    const res = await fetch(url, { ...options, headers });
    if (res.status === 401) {
        // token missing/expired -> go to login
        localStorage.removeItem(TOKEN_KEY);
        window.location.href = "/login.html";
        throw new Error("Unauthorized");
    }
    return res;
}

export function requireAuth() {
    if (!getToken()) window.location.href = "/login.html";
}

export function logout() {
    localStorage.removeItem(TOKEN_KEY);
    window.location.href = "/login.html";
}
