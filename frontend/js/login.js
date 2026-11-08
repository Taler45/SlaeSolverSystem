if(getToken()){
    location.href = "index.html"
}

const loginTab = document.getElementById("tab-login")
const registerTab = document.getElementById("tab-register")

const loginForm = document.getElementById("login-form")
const registerForm = document.getElementById("register-form")

const loginErr = document.getElementById("login-error")
const registerErr = document.getElementById("register-error")

function showTab(name){
    loginTab.classList.toggle("active", name === "login")
    registerTab.classList.toggle("active", name === "register")

    loginForm.classList.toggle("hidden", name !== "login")
    registerForm.classList.toggle("hidden", name !== "register")
}

loginTab.addEventListener("click", () => showTab("login"))
registerTab.addEventListener("click", () => showTab("register"))


const securityEye = document.querySelectorAll(".show-password")

securityEye.forEach(btn => {btn.addEventListener("click", ()=> {
    const target = document.getElementById(btn.dataset.target)
     
    if(target.type === "password"){
        target.type = "text"
    } else{
        target.type = "password"
    }

    })
})  

function showError(box, text){
    box.textContent = text
    box.classList.remove("hidden")
}

function hideError(box){
    box.classList.add("hidden")
}

loginForm.addEventListener("submit", async (e) => {
    e.preventDefault()
    hideError(loginErr)

    const email = document.getElementById("login-email").value.trim()
    const password = document.getElementById("login-password").value

    const submitBtn = loginForm.querySelector('button[type="submit"]')
    submitBtn.disabled = true

    try {
        const data = await api("/api/auth/login", "POST", { email, password })
        setToken(data.token)
        location.href = "index.html"
    } catch (err) {
        showError(loginErr, err.message)
    } finally {
        submitBtn.disabled = false
    }
})

registerForm.addEventListener("submit", async (e) => {
    e.preventDefault()
    hideError(registerErr)

    const firstName = document.getElementById("reg-first").value.trim()
    const lastName = document.getElementById("reg-last").value.trim()
    const email = document.getElementById("reg-email").value.trim()
    const password = document.getElementById("reg-password").value
    const repeat = document.getElementById("reg-password-repeat").value

    if(password !== repeat){
        showError(registerErr, "Паролі не збігаються")
        return
    }

    const submitBtn = registerForm.querySelector('button[type="submit"]')
    submitBtn.disabled = true

    try {
        await api("/api/auth/register", "POST", { firstName, lastName, email, password })

        const data = await api("/api/auth/login", "POST", { email, password })
        setToken(data.token)
        location.href = "index.html"
    } catch (err) {
        showError(registerErr, err.message)
    } finally {
        submitBtn.disabled = false
    }
})