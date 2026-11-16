[CmdletBinding()]
param([switch]$NoBrowser)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$logs=Join-Path $root 'Logs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
function Assert-Success([string]$message) { if ($LASTEXITCODE -ne 0) { throw $message } }
function Wait-Url([string]$url) {
    for ($i=0;$i -lt 60;$i++) {
        try { $r=Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2; if ($r.StatusCode -eq 200) { return } } catch {}
        Start-Sleep -Seconds 1
    }
    throw "Service did not become ready: $url. Check Logs."
}
function Is-Ready([string]$url) {
    try { return (Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200 } catch { return $false }
}
foreach ($command in @('dotnet','node','npm','sqlcmd','sqllocaldb')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) { throw "Install prerequisite: $command. See README.md." }
}
& (Join-Path $PSScriptRoot 'initialize-local.ps1')
if (-not $?) { throw 'Private local configuration could not be initialized' }
Get-Content (Join-Path $root '.env') | ForEach-Object {
    if ($_ -match '^LOCAL_JWT_SECRET=(.+)$') { $env:Jwt__Secret=$matches[1] }
}
& sqllocaldb start MSSQLLocalDB | Out-Null
Assert-Success 'SQL Server LocalDB could not start'
$exists=& sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -b -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID('Travel_Agency') IS NULL THEN 0 ELSE 1 END"
Assert-Success 'Could not inspect the local database'
if (($exists -join '').Trim() -eq '0') {
    & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -b -i (Join-Path $root 'Database/Travel_AgencyDB.sql')
    Assert-Success 'Database initialization failed'
}
# The update script only adds absent features; the reset/schema script is never run over an existing database.
& sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -b -i (Join-Path $root 'Database/Travel_Agency_Features_Update.sql') | Out-Null
Assert-Success 'Additive database update failed'
if (-not (Is-Ready 'http://127.0.0.1:8025/readyz')) {
    $tools=Join-Path $root '.tools/mailpit'
    $exe=Join-Path $tools 'mailpit.exe'
    if (-not (Test-Path -LiteralPath $exe)) {
        New-Item -ItemType Directory -Path $tools -Force | Out-Null
        $zip=Join-Path $tools 'mailpit.zip'
        Invoke-WebRequest 'https://github.com/axllent/mailpit/releases/download/v1.33.1/mailpit-windows-amd64.zip' -OutFile $zip
        if ((Get-FileHash $zip -Algorithm SHA256).Hash -ne '0bcf8f1e7c21005b30c206064ff2f37dac602ef66774e558379d326f5673d652') { throw 'Mailpit download checksum did not match the official release.' }
        Expand-Archive -LiteralPath $zip -DestinationPath $tools -Force
    }
    Start-Process -FilePath $exe -ArgumentList @('--listen','127.0.0.1:8025','--smtp','127.0.0.1:1025','--disable-version-check') -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logs 'mailpit.out.log') -RedirectStandardError (Join-Path $logs 'mailpit.err.log')
    Wait-Url 'http://127.0.0.1:8025/readyz'
}
if (-not (Is-Ready 'http://127.0.0.1:5132/health')) {
    $project=Join-Path $root 'Travel Agency Portal/Travel Agency Portal/Travel Agency Portal.csproj'
    Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @('run','--project',('"'+$project+'"'),'--launch-profile','http') -WorkingDirectory $root -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logs 'api.out.log') -RedirectStandardError (Join-Path $logs 'api.err.log')
    Wait-Url 'http://127.0.0.1:5132/health'
}
$ui=Join-Path $root 'UI/travel-agency'
if (-not (Test-Path (Join-Path $ui 'node_modules/vite/bin/vite.js'))) {
    Push-Location $ui
    try { & npm ci; Assert-Success 'Frontend dependency installation failed' } finally { Pop-Location }
}
if (-not (Is-Ready 'http://127.0.0.1:3000')) {
    $vite=Join-Path $ui 'node_modules/vite/bin/vite.js'
    Start-Process -FilePath (Get-Command node).Source -ArgumentList @(('"'+$vite+'"'),'--host','127.0.0.1','--port','3000','--strictPort') -WorkingDirectory $ui -WindowStyle Hidden -RedirectStandardOutput (Join-Path $logs 'frontend.out.log') -RedirectStandardError (Join-Path $logs 'frontend.err.log')
    Wait-Url 'http://127.0.0.1:3000'
}
Write-Host 'Website: http://localhost:3000  API: http://localhost:5132  Local email inbox: http://localhost:8025'
if (-not $NoBrowser) { Start-Process 'http://localhost:3000' }
