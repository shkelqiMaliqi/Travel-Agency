[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) { throw 'Install kubectl and select a local cluster first.' }
$context = & kubectl config current-context
if ($LASTEXITCODE -ne 0 -or $context -notmatch '^(docker-desktop|kind-travel-agency-ci|minikube)$') {
    throw "This initializer is restricted to a local disposable/demo cluster; current context: $context"
}
$values=@{}
Get-Content (Join-Path $root '.env') | ForEach-Object {
    if ($_ -match '^([A-Z_]+)=(.+)$') { $values[$matches[1]]=$matches[2] }
}
foreach ($name in @('LOCAL_SQL_PASSWORD','LOCAL_JWT_SECRET')) { if (-not $values[$name]) { throw 'Run initialize-local.ps1 first.' } }
& kubectl apply -f (Join-Path $root 'Infrastructure/kubernetes/namespace.yaml')
if ($LASTEXITCODE -ne 0) { throw 'Could not create namespace' }
# Never replace an existing cluster's credentials on re-run.
& kubectl get secret travel-agency-secrets -n travel-agency 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    $rabbit = [guid]::NewGuid().ToString('N')
    $secret=@{apiVersion='v1';kind='Secret';metadata=@{name='travel-agency-secrets';namespace='travel-agency'};type='Opaque';stringData=@{
        Jwt__Secret=$values.LOCAL_JWT_SECRET
        ConnectionStrings__CRUDCS="Server=sqlserver,1433;Database=Travel_Agency;User Id=sa;Password=$($values.LOCAL_SQL_PASSWORD);TrustServerCertificate=true;MultipleActiveResultSets=true"
        MSSQL_SA_PASSWORD=$values.LOCAL_SQL_PASSWORD
        RABBITMQ_DEFAULT_PASS=$rabbit
        RabbitMq__ConnectionString="amqp://travelagency:${rabbit}@rabbitmq:5672"
        Storage__S3__AccessKey='minioadmin'
        Storage__S3__SecretKey='minioadmin'
    }}
    $secret | ConvertTo-Json -Depth 6 -Compress | & kubectl apply -f - | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not create private local secrets' }
}
& kubectl apply -k (Join-Path $root 'Infrastructure/kubernetes')
if ($LASTEXITCODE -ne 0) { throw 'Kubernetes apply failed' }
& kubectl wait --for=condition=complete job/sqlserver-init -n travel-agency --timeout=300s
if ($LASTEXITCODE -ne 0) { throw 'Database initialization did not complete' }
Write-Host 'Local dependencies initialized. Run deploy-kubernetes.sh with the three application images.'
