const API_BASE = "/api";          // your controllers are already under /api/... I assume
const TOKEN_KEY = "smr_token";    // localStorage key

// utility: store token (adjust property name if your API returns a different field)
function saveTokenFromResponse(json) {
    // Try common property names; adapt to your AuthController response
    const token = json.token || json.accessToken || json.jwt;
    if (!token) throw new Error("Token not found in response. Adjust auth.js to your API response shape.");
    localStorage.setItem(TOKEN_KEY, token);
}

// LOGIN
const loginForm = document.getElementById("loginForm");
if (loginForm) {
    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const email = document.getElementById("email").value.trim();
        const password = document.getElementById("password").value;

        try {
            const res = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ userNameOrEmail: email, password })
            });

            if (!res.ok) throw new Error(`Login failed (${res.status})`);
            const data = await res.json();
            saveTokenFromResponse(data);
            window.location.href = "/index.html";
        } catch (err) {
            document.getElementById("error").textContent = err.message;
        }
    });
}
