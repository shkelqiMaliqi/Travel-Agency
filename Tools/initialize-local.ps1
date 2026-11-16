[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$path = Join-Path $root '.env'
if (Test-Path -LiteralPath $path) {
    $existing = Get-Content -LiteralPath $path -Raw
    foreach ($key in @('LOCAL_SQL_PASSWORD','LOCAL_JWT_SECRET','LOCAL_GRAFANA_PASSWORD')) {
        if ($existing -notmatch "(?m)^$key=.+$") { throw "$key must be set in .env. Existing secrets were not overwritten." }
    }
    Write-Host 'Existing private .env retained.'
    exit 0
}
function New-LocalSecret {
    param([int]$Bytes = 48)
    $buffer = New-Object byte[] $Bytes
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($buffer) } finally { $rng.Dispose() }
    return -join ($buffer | ForEach-Object { $_.ToString('x2') })
}
$lines = @(
    "LOCAL_SQL_PASSWORD=Local!$(New-LocalSecret 24)",
    "LOCAL_JWT_SECRET=$(New-LocalSecret)",
    "LOCAL_GRAFANA_PASSWORD=$(New-LocalSecret 24)"
)
[IO.File]::WriteAllLines($path, $lines)
Write-Host 'Generated private random local configuration. Values are not printed.'
