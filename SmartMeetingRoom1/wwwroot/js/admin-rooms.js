// --- admin-rooms.js ---
const API_BASE = "/api";
const TOKEN_KEY = "smr_access_token"; // MUST match auth.js

function getToken() {
    return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY) || "";
}

async function api(url, options = {}) {
    const res = await fetch(url, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {}),
            "Authorization": "Bearer " + getToken()
        }
    });
    if (res.status === 401) {
        // not logged in
        window.location.href = "/login.html";
        throw new Error("Unauthorized");
    }
    return res;
}

// ---- UI helpers ----
function equipmentBadgesFromFeatures(features) {
    if (!features) return "";
    const items = String(features).split(",").map(s => s.trim()).filter(Boolean);
    return items.map(x => `<span class="badge bg-info me-1">${x}</span>`).join("");
}

function readEquipFromForm() {
    const items = [];
    if (document.getElementById("equipMic").checked) items.push("Mic");
    if (document.getElementById("equipProjector").checked) items.push("Projector");
    if (document.getElementById("equipScreen").checked) items.push("Screen");
    return items.join(","); // your DTO uses Features (string)
}

function fillEquipCheckboxes(features) {
    const set = new Set(String(features || "").split(",").map(s => s.trim()));
    document.getElementById("equipMic").checked = set.has("Mic");
    document.getElementById("equipProjector").checked = set.has("Projector");
    document.getElementById("equipScreen").checked = set.has("Screen");
}

// ---- Auth guard (Admin only) ----
async function guardAdmin() {
    const meRes = await api(`${API_BASE}/auth/me`);
    if (!meRes.ok) { window.location.href = "/login.html"; return; }
    const me = await meRes.json();
    document.getElementById("navUserName").textContent = me.userName || me.UserName || "User";
    const roles = me.roles || [];
    if (!roles.some(r => r.toLowerCase() === "admin")) {
        window.location.href = "/index.html";
    }
}

// ---- Rooms ----
async function loadRooms() {
    const res = await api(`${API_BASE}/rooms`);
    if (!res.ok) return;
    const rooms = await res.json();

    // simple stats
    document.getElementById("statTotalRooms").textContent = rooms.length;
    document.getElementById("statNewThisMonth").textContent = 0;

    const tbody = document.getElementById("roomsTbody");
    const tpl = document.getElementById("roomRowTemplate");
    tbody.innerHTML = "";

    rooms.forEach(r => {
        const tr = tpl.content.firstElementChild.cloneNode(true);
        tr.dataset.id = r.id;
        tr.querySelector('[data-field="name"]').textContent = r.name;
        tr.querySelector('[data-field="capacity"]').textContent = (r.capacity ?? 0) + " people";
        tr.querySelector('[data-field="equipment"]').innerHTML = equipmentBadgesFromFeatures(r.features);

        tr.querySelector(".btn-edit").addEventListener("click", () => openEdit(r));
        tr.querySelector(".btn-delete").addEventListener("click", () => onDelete(r.id, r.name));
        tbody.appendChild(tr);
    });
}

function getDtoFromForm() {
    return {
        name: document.getElementById("roomName").value.trim(),
        capacity: parseInt(document.getElementById("roomCapacity").value, 10) || 0,
        location: document.getElementById("roomLocation").value.trim() || "(unspecified)",
        features: readEquipFromForm() // <-- comma-separated string
    };
}

async function onSaveRoom() {
    const id = document.getElementById("roomId").value;
    const dto = getDtoFromForm();
    const url = id ? `${API_BASE}/rooms/${id}` : `${API_BASE}/rooms`;
    const method = id ? "PUT" : "POST";

    const res = await api(url, { method, body: JSON.stringify(dto) });
    if (res.ok || res.status === 201 || res.status === 204) {
        bootstrap.Modal.getInstance(document.getElementById("modalRoom"))?.hide();
        await loadRooms();
    } else {
        const msg = await res.text();
        alert("Save failed: " + msg);
    }
}

function openEdit(r) {
    document.getElementById("modalRoomLabel").textContent = "Edit Room";
    document.getElementById("roomId").value = r.id;
    document.getElementById("roomName").value = r.name ?? "";
    document.getElementById("roomCapacity").value = r.capacity ?? 0;
    document.getElementById("roomLocation").value = r.location ?? "";
    fillEquipCheckboxes(r.features);
    new bootstrap.Modal("#modalRoom").show();
}

async function onDelete(id, name) {
    if (!confirm(`Delete room "${name}"?`)) return;
    const res = await api(`${API_BASE}/rooms/${id}`, { method: "DELETE" });
    if (res.status === 204) await loadRooms();
    else alert("Delete failed");
}

// Sign out
document.getElementById("btnSignOut")?.addEventListener("click", (e) => {
    e.preventDefault();
    localStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    window.location.href = "/login.html";
});

// Add new
document.getElementById("btnAddRoom")?.addEventListener("click", () => {
    document.getElementById("modalRoomLabel").textContent = "Add New Room";
    document.getElementById("roomForm").reset();
    document.getElementById("roomId").value = "";
});

document.getElementById("btnSaveRoom")?.addEventListener("click", onSaveRoom);

// Boot
(async function init() {
    await guardAdmin();
    await loadRooms();
})();
