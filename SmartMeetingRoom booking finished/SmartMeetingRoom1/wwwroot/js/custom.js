/* SmartMeet – site-wide JS (works with ASP.NET Core static files) */

/* ========= CONFIG ========= */
const API_BASE =
    window.location.origin.startsWith("http") ? window.location.origin : "https://localhost:7088";

// Where we store the access token after /api/auth/login
const TOKEN_KEY = "smr_access_token";

/* ========= TOKEN HELPERS ========= */
function getToken() {
    return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY) || "";
}

function setToken(token, rememberMe = true) {
    // rememberMe true -> localStorage, false -> sessionStorage
    if (rememberMe) localStorage.setItem(TOKEN_KEY, token || "");
    else sessionStorage.setItem(TOKEN_KEY, token || "");
}

function clearToken() {
    localStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
}

/* ========= FETCH WITH AUTH ========= */
async function authedFetch(path, options = {}) {
    const url = path.startsWith("http") ? path : `${API_BASE}${path}`;
    const headers = new Headers(options.headers || {});
    if (!headers.has("Content-Type") && options.body) headers.set("Content-Type", "application/json");

    const token = getToken();
    if (token) headers.set("Authorization", "Bearer " + token);

    const res = await fetch(url, { ...options, headers });
    // If unauthorized, bounce to login
    if (res.status === 401) {
        clearToken();
        // Only redirect on pages that need auth (avoid loops on login)
        if (!location.pathname.includes("login")) location.href = "/login.html";
    }
    return res;
}

/* ========= BASIC GUARD / NAV ========= */
async function guardLoggedIn() {
    const token = getToken();
    if (!token) {
        location.href = "/login.html";
        return null;
    }
    const meRes = await authedFetch("/api/auth/me");
    if (!meRes.ok) {
        location.href = "/login.html";
        return null;
    }
    const me = await meRes.json();
    const name = me.userName || me.UserName || me.email || "User";
    const navUserName = document.getElementById("navUserName");
    if (navUserName) navUserName.textContent = name;
    return me;
}

/* ========= LOGIN / LOGOUT HOOKS (optional forms) ========= */
async function handleLoginSubmit(e) {
    e.preventDefault();
    const form = e.target;
    const dto = {
        userNameOrEmail: form.email.value.trim(),
        password: form.password.value
    };
    const remember = !!form.querySelector("#rememberMe")?.checked;

    const res = await fetch(`${API_BASE}/api/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dto)
    });

    if (!res.ok) {
        alert("Login failed: " + (await res.text()));
        return;
    }

    const data = await res.json(); // expect { accessToken, ... }
    setToken(data.accessToken, remember);
    location.href = "/index.html";
}

async function handleLogoutClick(e) {
    e?.preventDefault?.();
    try { await authedFetch("/api/auth/revoke", { method: "POST" }); } catch { }
    clearToken();
    location.href = "/login.html";
}

/* ========= BOOKING PAGE ========= */
function collectAttendees() {
    // Attendees are typed into the input and turned into "pills".
    // If you haven't built pills yet, we also accept comma-separated emails in the input.
    const pills = Array.from(document.querySelectorAll("#attendeePills .attendee-pill"))
        .map(p => (p.firstChild?.textContent || "").trim())
        .filter(Boolean);

    if (pills.length) return pills;

    const raw = document.getElementById("meetingAttendees")?.value || "";
    return raw.split(",").map(s => s.trim()).filter(s => s.includes("@"));
}

function toIso(dtLocalValue) {
    // <input type="datetime-local"> gives local time string; convert to ISO (local -> UTC)
    if (!dtLocalValue) return null;
    const dt = new Date(dtLocalValue);
    return dt.toISOString();
}

async function loadRoomsIntoSelect() {
    const sel = document.getElementById("meetingRoom");
    if (!sel) return;

    sel.innerHTML = `<option value="">Loading rooms…</option>`;
    const res = await authedFetch("/api/rooms");
    if (!res.ok) {
        sel.innerHTML = `<option value="">(failed to load)</option>`;
        return;
    }

    const rooms = await res.json();
    sel.innerHTML = `<option value="">Select a room</option>`;
    rooms.forEach(r => {
        const opt = document.createElement("option");
        opt.value = r.id; // IMPORTANT: send numeric RoomId back to API
        opt.textContent = `${r.name} (${r.capacity ?? 0} people)`;
        sel.appendChild(opt);
    });
}

async function loadAvailability(forDateStr) {
    const grid = document.querySelector("#gridRoomAvailability tbody");
    if (!grid) return;

    grid.innerHTML = `<tr><td colspan="4">Loading…</td></tr>`;

    // If a date is picked, use that date; else today.
    // Send ISO so backend can parse.
    const dateParam = encodeURIComponent(
        forDateStr ? new Date(forDateStr).toISOString() : new Date().toISOString()
    );

    const res = await authedFetch(`/api/meetings/availability?date=${dateParam}`);
    if (!res.ok) {
        grid.innerHTML = `<tr><td colspan="4">No data (create /api/meetings/availability)</td></tr>`;
        return;
    }

    const data = await res.json(); // expect [{ time:"09:00", room:"Room A", status:"Free"|"Busy" }, ...]
    // group rows by time
    const byTime = data.reduce((acc, x) => {
        (acc[x.time] ||= []).push(x);
        return acc;
    }, {});

    // determine columns from the first row’s rooms (optional)
    // Your table header already has Room A/B/C static; this simply fills cells in order.
    grid.innerHTML = "";
    Object.keys(byTime).sort().forEach(time => {
        const row = document.createElement("tr");
        const cells = byTime[time]
            .map(x => `<td><span class="badge ${x.status === "Busy" ? "bg-danger" : "bg-success"}">${x.status}</span></td>`)
            .join("");
        row.innerHTML = `<td>${time}</td>${cells}`;
        grid.appendChild(row);
    });
}

async function handleBookingSubmit(e) {
    e.preventDefault();

    const title = document.getElementById("meetingTitle").value.trim();
    const when = document.getElementById("meetingDate").value;
    const durationMin = parseInt(document.getElementById("meetingDuration").value, 10) || 60;
    const roomId = parseInt(document.getElementById("meetingRoom").value, 10);
    const recurring = !!document.getElementById("chkRecurring")?.checked;
    const videoConf = !!document.getElementById("chkVideoConf")?.checked;

    if (!title || !when || !roomId) {
        alert("Please fill the title, date/time, and room.");
        return;
    }

    const start = new Date(when);
    const end = new Date(start.getTime() + durationMin * 60000);

    const dto = {
        title,
        roomId,
        startTime: start.toISOString(),
        endTime: end.toISOString(),
        attendees: collectAttendees(),   // array of emails (optional)
        recurring,
        videoConf
    };

    const res = await authedFetch("/api/meetings", {
        method: "POST",
        body: JSON.stringify(dto)
    });

    if (res.status === 201 || res.ok) {
        alert("Meeting booked successfully!");
        location.href = "/index.html";
    } else {
        const msg = await res.text();
        alert("Booking failed: " + msg);
    }
}

function wireBookingPage() {
    const form = document.getElementById("bookingForm");
    const cancelBtn = document.getElementById("btnCancel");
    const dt = document.getElementById("meetingDate");

    if (form) form.addEventListener("submit", handleBookingSubmit);
    if (cancelBtn) cancelBtn.addEventListener("click", () => (location.href = "/index.html"));

    // When date changes, reload availability panel to that day
    if (dt) dt.addEventListener("change", () => loadAvailability(dt.value));

    // attendee pill creation (Enter key)
    const attendeeInput = document.getElementById("meetingAttendees");
    if (attendeeInput) {
        attendeeInput.addEventListener("keypress", e => {
            if (e.key === "Enter") {
                e.preventDefault();
                const email = attendeeInput.value.trim();
                if (!email || !email.includes("@")) return;
                const pill = document.createElement("span");
                pill.className = "badge bg-primary attendee-pill me-1";
                pill.innerHTML = `${email}<button class="btn-close btn-close-white ms-2" aria-label="Remove"></button>`;
                pill.querySelector("button").addEventListener("click", () => pill.remove());
                document.getElementById("attendeePills").appendChild(pill);
                attendeeInput.value = "";
            }
        });
    }
}

/* ========= ADMIN ROOMS (table only) ========= */
async function loadRoomsTable() {
    const tbody = document.getElementById("roomsTbody");
    const tpl = document.getElementById("roomRowTemplate");
    if (!tbody || !tpl) return;

    const res = await authedFetch("/api/rooms");
    if (!res.ok) return;

    const rooms = await res.json();
    tbody.innerHTML = "";
    rooms.forEach(r => {
        const row = tpl.content.firstElementChild.cloneNode(true);
        row.dataset.id = r.id;
        row.querySelector('[data-field="name"]').textContent = r.name;
        row.querySelector('[data-field="capacity"]').textContent = `${r.capacity ?? 0} people`;
        const feats = (r.features || "").split(",").map(s => s.trim()).filter(Boolean)
            .map(f => `<span class="badge bg-info me-1">${f}</span>`).join("");
        row.querySelector('[data-field="features"]').innerHTML = feats;
        tbody.appendChild(row);
    });
}

/* ========= PAGE ROUTER ========= */
function currentPage() {
    const p = location.pathname.toLowerCase();
    if (p.includes("pages-login")) return "login";
    if (p.includes("booking")) return "booking";
    if (p.includes("admin")) return "admin";
    return "other";
}

document.addEventListener("DOMContentLoaded", async () => {
    // Wire login page if present
    const loginForm = document.getElementById("loginForm");
    if (loginForm) loginForm.addEventListener("submit", handleLoginSubmit);

    // Wire sign out buttons (header dropdown)
    document.querySelectorAll("#btnSignOut,.btn-signout").forEach(btn => {
        btn.addEventListener("click", handleLogoutClick);
    });

    // Route
    const page = currentPage();
    if (page === "login") return; // no guard on login page

    // Guard + set user name
    await guardLoggedIn();

    if (page === "booking") {
        wireBookingPage();
        await loadRoomsIntoSelect();
        const dt = document.getElementById("meetingDate");
        await loadAvailability(dt?.value);
    } else if (page === "admin") {
        await loadRoomsTable();
    }
});
