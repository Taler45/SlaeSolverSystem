function getToken(){
    return localStorage.getItem("token")
}

function setToken(token){
    localStorage.setItem("token", token)
}

function logout(){
    localStorage.removeItem("token")
    location.href = "login.html"
}

async function api(url, method = "GET", body = null){
    const headers = {}

    if(body !== null){
        headers["Content-Type"] = "application/json"
    }

    const token = getToken()

    if(token){
        headers["Authorization"] = "Bearer " + token
    }

    const response = await fetch(url,{
        method: method,
        headers: headers,
        body: body ? JSON.stringify(body) : null
    })

    if(response.status === 401 && token !== null){
        logout()
        return
    }

    if(!response.ok){
        const problem = await response.json().catch(() => null)
        throw new Error(getErrorMessage(problem, response.status))
    }

    return response.json()
}

function getErrorMessage(problem, status){
    if(problem?.detail){
        return problem.detail
    }

    if(problem?.errors){
        return Object.values(problem.errors)[0][0]
    }

    if(problem?.title){
        return problem.title
    }

    return "Помилка " + status
}

function requireAuth(){
    if(!getToken()){
        location.href = "login.html"
    }
}

function decodeJwt(token) {
    const parts = token.split(".")
    const payload = parts[1]
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/")
    const text = decodeURIComponent(escape(atob(base64)))
    return JSON.parse(text)
}