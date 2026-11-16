[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$ui=Join-Path $root 'UI/travel-agency'
$logs=Join-Path $root 'Logs/verification'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$name='TravelAgencyE2E_'+[guid]::NewGuid().ToString('N')
if ($name -notmatch '^TravelAgencyE2E_[a-f0-9]{32}$') { throw 'Invalid disposable database name' }
$sql='(localdb)\MSSQLLocalDB'
$apiProcess=$null;$uiProcess=$null
$prior=@{}
foreach($key in @('ASPNETCORE_ENVIRONMENT','ConnectionStrings__CRUDCS','Jwt__Secret','Workers__Enabled','REACT_APP_API_URL','E2E_BASE_URL','E2E_API_URL','E2E_MAIL_URL','E2E_FULL_STACK')) { $prior[$key]=[Environment]::GetEnvironmentVariable($key) }
function Check([string]$message) { if ($LASTEXITCODE -ne 0) { throw $message } }
function Wait-Ready([string]$url) {
    for($i=0;$i -lt 60;$i++) { try { if ((Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { return } } catch {}; Start-Sleep -Seconds 1 }
    throw "Test service did not start: $url"
}
try {
    # A real SMTP capture server must already be running; no external email is sent.
    Wait-Ready 'http://127.0.0.1:8025/readyz'
    & sqllocaldb start MSSQLLocalDB | Out-Null; Check 'LocalDB did not start'
    $schema=Get-Content (Join-Path $root 'Database/Travel_AgencyDB.sql') -Raw
    $schema=$schema.Replace('CREATE DATABASE Travel_Agency;',"CREATE DATABASE [$name];").Replace('USE Travel_Agency;',"USE [$name];")
    $schemaFile=Join-Path $logs ($name+'.sql')
    [IO.File]::WriteAllText($schemaFile,$schema)
    & sqlcmd -S $sql -E -C -b -i $schemaFile; Check 'Isolated schema initialization failed'
    $env:ASPNETCORE_ENVIRONMENT='Development'
    $env:ConnectionStrings__CRUDCS="Server=$sql;Database=$name;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
    $env:Jwt__Secret=([guid]::NewGuid().ToString('N')+[guid]::NewGuid().ToString('N'))
    $env:Workers__Enabled='false'
    $env:REACT_APP_API_URL='http://127.0.0.1:5152/api/v1'
    $project=Join-Path $root 'Travel Agency Portal/Travel Agency Portal/Travel Agency Portal.csproj'
    $apiProcess=Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @('run','--no-build','--configuration','Release','--no-launch-profile','--project',('"'+$project+'"'),'--urls','http://127.0.0.1:5152') -WorkingDirectory $root -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logs 'e2e-api.out.log') -RedirectStandardError (Join-Path $logs 'e2e-api.err.log')
    $vite=Join-Path $ui 'node_modules/vite/bin/vite.js'
    $uiProcess=Start-Process -FilePath (Get-Command node).Source -ArgumentList @(('"'+$vite+'"'),'--host','127.0.0.1','--port','3001','--strictPort') -WorkingDirectory $ui -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logs 'e2e-ui.out.log') -RedirectStandardError (Join-Path $logs 'e2e-ui.err.log')
    Wait-Ready 'http://127.0.0.1:5152/health';Wait-Ready 'http://127.0.0.1:3001'
    $env:E2E_BASE_URL='http://127.0.0.1:3001';$env:E2E_API_URL='http://127.0.0.1:5152';$env:E2E_MAIL_URL='http://127.0.0.1:8025';$env:E2E_FULL_STACK='true'
    Push-Location $ui
    try { & npx playwright test '--reporter=line,json' | Tee-Object -FilePath (Join-Path $logs 'browser-results.txt'); Check 'Browser integration checks failed' } finally { Pop-Location }
} finally {
    # Kill only process trees this harness created, before dropping only its generated database.
    foreach($process in @($uiProcess,$apiProcess)) { if($process -and -not $process.HasExited) { & taskkill /PID $process.Id /T /F | Out-Null } }
    & sqlcmd -S $sql -E -C -b -Q "IF DB_ID('$name') IS NOT NULL BEGIN ALTER DATABASE [$name] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$name]; END" | Out-Null
    foreach($key in $prior.Keys) { [Environment]::SetEnvironmentVariable($key,$prior[$key]) }
}

