requireAuth()

const user = decodeJwt(getToken())

const role = user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]

if (role !== "Admin") {
    location.href = "index.html"
}

const firstName = user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"]
const lastName = user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"]

document.getElementById("user-name").textContent = firstName + " " + lastName
document.getElementById("logout-btn").addEventListener("click", logout)

const statusText = {
    Queued: "У черзі",
    InProgress: "Рахується"
}

function showStats(stats) {
    document.getElementById("stat-queued").textContent = stats.queued
    document.getElementById("stat-in-progress").textContent = stats.inProgress
    document.getElementById("stat-done-hour").textContent = stats.doneLastHour
    document.getElementById("stat-failed-hour").textContent = stats.failedLastHour
    document.getElementById("stat-done-total").textContent = stats.doneTotal
    document.getElementById("stat-failed-total").textContent = stats.failedTotal
    document.getElementById("stat-canceled-total").textContent = stats.canceledTotal
}

function showServers(servers) {
    let html = ""

    for (let i = 0; i < servers.length; i++) {
        const s = servers[i]

        if (s.isAlive) {
            const percent = Math.round(s.activeTasks / s.maxTasks * 100)

            html += `
                <div class="card server">
                    <h2>${s.serverId}</h2>
                    <p class="alive-text">● працює</p>
                    <p>Задач: ${s.activeTasks} з ${s.maxTasks}</p>
                    <div class="progress"><div class="progress-fill" style="width: ${percent}%"></div></div>
                    <p class="muted">Вага: ${s.totalWeight}</p>
                </div>`
        } else {
            let lastSeen = "ніколи"
            if (s.lastSeenAt) {
                lastSeen = new Date(s.lastSeenAt).toLocaleTimeString("uk-UA")
            }

            html += `
                <div class="card server dead">
                    <h2>${s.serverId}</h2>
                    <p class="dead-text">● недоступний</p>
                    <p class="muted">Остання відповідь: ${lastSeen}</p>
                </div>`
        }
    }

    document.getElementById("servers").innerHTML = html
}

function showTasks(tasks) {
    let html = ""

    for (let i = 0; i < tasks.length; i++) {
        const t = tasks[i]

        let server = "—"
        if (t.serverId) {
            server = t.serverId
        }

        let progress = "—"
        if (t.status === "InProgress") {
            progress = `
                <div class="progress-cell">
                    <div class="progress"><div class="progress-fill" style="width: ${t.progress}%"></div></div>
                    <span>${t.progress}%</span>
                </div>`
        }

        html += `
            <tr>
                <td class="mono">${t.id.slice(0, 8)}</td>
                <td>${t.userEmail}</td>
                <td>${t.matrixSize}</td>
                <td>${server}</td>
                <td><span class="badge-${t.status}">${statusText[t.status]}</span></td>
                <td>${progress}</td>
            </tr>`
    }

    document.getElementById("admin-tasks").innerHTML = html

    if (tasks.length === 0) {
        document.getElementById("admin-empty").classList.remove("hidden")
    } else {
        document.getElementById("admin-empty").classList.add("hidden")
    }
}

async function loadAdmin() {
    try {
        const overview = await api("/api/admin/overview")
        const servers = await api("/lb/servers")

        showStats(overview.stats)
        showServers(servers)
        showTasks(overview.activeTasks)

        document.getElementById("updated-at").textContent = new Date().toLocaleTimeString("uk-UA")
    } catch (err) {
        console.log(err)
    }
}

loadAdmin()
setInterval(loadAdmin, 2000)