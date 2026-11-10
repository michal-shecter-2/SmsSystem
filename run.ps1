$ErrorActionPreference = "Stop"
$processes = @()

try {
    foreach ($name in @("Sms.MockProvider", "Sms.Api")) {
        $projectPath = Join-Path $PSScriptRoot "$name\$name.csproj"

        if (-not (Test-Path $projectPath)) {
            throw "Project not found: $projectPath"
        }

        $arguments = "run --project `"$projectPath`" --launch-profile https"

        $processes += Start-Process `
            -FilePath "dotnet" `
            -ArgumentList $arguments `
            -WorkingDirectory $PSScriptRoot `
            -PassThru
    }

    Write-Host "Application: https://localhost:7071"
    Write-Host "Mock provider: https://localhost:7172"

    Read-Host "Press Enter to stop both servers"
}
finally {
    foreach ($process in $processes) {
        if (-not $process.HasExited) {
            taskkill /PID $process.Id /T /F | Out-Null
        }
    }
}