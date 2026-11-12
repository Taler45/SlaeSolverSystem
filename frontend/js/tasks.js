requireAuth()

const taskForm = document.getElementById("task-form")
const manualFields = document.getElementById("manual-fields")
const taskError = document.getElementById("task-error")
const tasksBody = document.getElementById("tasks-body")
const resultCard = document.getElementById("result-card")
const submitBtn = taskForm.querySelector('button[type="submit"]')

let page = 1
let isSubmitting = false
let activeCount = 0

const statusText = {
    Queued: "У черзі",
    InProgress: "Рахується",
    Done: "Готово",
    Canceled: "Скасовано",
    Error: "Помилка"
}

const user = decodeJwt(getToken())

const firstName = user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"]
const lastName = user["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"]
const role = user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]

document.getElementById("user-name").textContent = firstName + " " + lastName

if (role === "Admin") {
    document.getElementById("admin-link").classList.remove("hidden")
}

document.getElementById("logout-btn").addEventListener("click", () => logout())

document.querySelectorAll('input[name="mode"]').forEach(radio => {
    radio.addEventListener("change", () => {
        if(radio.value === "manual"){
            manualFields.classList.remove("hidden")
        } else {
            manualFields.classList.add("hidden")
        }
    })
})

function updateSubmitButton(){
    submitBtn.disabled = isSubmitting || activeCount >= 3
}

function progressHtml(task){
    if(task.status === "Queued"){
        return task.queuePosition + "-а в черзі"
    }

    if(task.status === "Error"){
        return task.errorMessage
    }

    if(task.status === "Canceled"){
        return "—"
    }

    let text = task.progress + "%"

    if(task.status === "InProgress" && task.progress >= 5){
        const passed = (Date.now() - new Date(task.startedAt)) / 1000
        const left = passed / task.progress * (100 - task.progress)
        text += " · ≈ " + Math.round(left) + " с"
    }

    return `
        <div class="progress-cell">
            <div class="progress"><div class="progress-fill" style="width: ${task.progress}%"></div></div>
            <span>${text}</span>
        </div>`
}

function buttonHtml(task){
    if(task.status === "Queued" || task.status === "InProgress"){
        return `<button class="danger small" onclick="cancelTask('${task.id}')">Скасувати</button>`
    }

    if(task.status === "Done"){
        return `<button class="secondary small" onclick="showResult('${task.id}')">Результат</button>`
    }

    return ""
}

async function loadTasks(){
    try {
        const tasks = await api("/api/tasks/history?page=" + page + "&pageSize=10")

        tasksBody.innerHTML = ""
        let active = 0

        for(const task of tasks){
            tasksBody.innerHTML += `
                <tr>
                    <td class="mono">${task.id.slice(0, 8)}</td>
                    <td>${task.matrixSize}</td>
                    <td><span class="badge-${task.status}">${statusText[task.status]}</span></td>
                    <td>${progressHtml(task)}</td>
                    <td class="muted">${new Date(task.createdAt).toLocaleString("uk-UA")}</td>
                    <td>${buttonHtml(task)}</td>
                </tr>`

            if(task.status === "Queued" || task.status === "InProgress"){
                active++
            }
        }

        document.getElementById("active-count").textContent = active
        activeCount = active
        updateSubmitButton()
        document.getElementById("page-number").textContent = page
        document.getElementById("tasks-empty").classList.toggle("hidden", tasks.length > 0)
        document.getElementById("prev-page").disabled = page === 1
        document.getElementById("next-page").disabled = tasks.length < 10
    } catch (err) {
        console.error(err)
    }
}

async function cancelTask(id){
    try {
        await api("/api/tasks/" + id + "/cancel", "POST")
        loadTasks()
    } catch (err) {
        showError(taskError, err.message)
    }
}

async function showResult(id){
    try {
        const task = await api("/api/tasks/" + id)
        const result = await api("/api/tasks/" + id + "/result")

        const seconds = (new Date(task.completedAt) - new Date(task.startedAt)) / 1000

        let solution = ""
        for(let i = 0; i < result.solution.length; i++){
            solution += "x" + (i + 1) + " = " + result.solution[i].toFixed(10) + "\n"
        }

        document.getElementById("result-id").textContent = id.slice(0, 8)
        document.getElementById("result-n").textContent = task.matrixSize
        document.getElementById("result-iterations").textContent = result.iterations
        document.getElementById("result-residual").textContent = result.residual.toExponential(2)
        document.getElementById("result-time").textContent = seconds.toFixed(1) + " с"
        document.getElementById("result-solution").textContent = solution

        resultCard.classList.remove("hidden")
    } catch (err) {
        showError(taskError, err.message)
    }
}

document.getElementById("result-close").addEventListener("click", () => {
    resultCard.classList.add("hidden")
})

document.getElementById("prev-page").addEventListener("click", () => {
    page--
    loadTasks()
})

document.getElementById("next-page").addEventListener("click", () => {
    page++
    loadTasks()
})

taskForm.addEventListener("submit", async (e) => {
    e.preventDefault()
    hideError(taskError)

    const matrixSize = Number(document.getElementById("matrix-size").value)
    const mode = document.querySelector('input[name="mode"]:checked').value

    const body = { matrixSize: matrixSize, matrix: null, rightSide: null }

    if(mode === "manual"){
        const lines = document.getElementById("matrix").value.trim().split("\n")
        body.matrix = lines.map(line => line.trim().split(/\s+/).map(Number))
        body.rightSide = document.getElementById("right-side").value.trim().split(/\s+/).map(Number)
    }

    isSubmitting = true
    updateSubmitButton()

    try {
        await api("/api/tasks/create/task", "POST", body)
        page = 1
    } catch (err) {
        showError(taskError, err.message)
    } finally {
        await loadTasks()
        isSubmitting = false
        updateSubmitButton()
    }
})

function showError(box, text){
    box.textContent = text
    box.classList.remove("hidden")
}

function hideError(box){
    box.classList.add("hidden")
}

loadTasks()
setInterval(loadTasks, 2000)