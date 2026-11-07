Set-Location "$PSScriptRoot/../SlaeSolver.API"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Server__ServerId = "srv2"
dotnet run --no-build --no-launch-profile --urls "http://localhost:5002"