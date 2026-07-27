[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$KeepRunning
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-Compose {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & docker compose @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

try {
    Write-Host "1. Validating Docker Compose configuration..."
    Invoke-Compose config --quiet

    if (-not $NoBuild) {
        Write-Host "2. Building API images..."
        Invoke-Compose build
    }

    Write-Host "3. Starting services and waiting for health checks..."
    Invoke-Compose up --detach --wait --wait-timeout 240

    Write-Host "4. Container status:"
    Invoke-Compose ps

    Write-Host "5. Testing Catalog API health..."
    Invoke-RestMethod -Uri "http://localhost:8081/health" -Method Get | Out-Host

    Write-Host "6. Testing Cart API health..."
    Invoke-RestMethod -Uri "http://localhost:8082/health" -Method Get | Out-Host

    Write-Host "7. Testing RabbitMQ management API..."
    $rabbitCredentials = [Convert]::ToBase64String(
        [Text.Encoding]::ASCII.GetBytes("mentoring:mentoring-password")
    )
    Invoke-RestMethod `
        -Uri "http://localhost:15672/api/overview" `
        -Headers @{ Authorization = "Basic $rabbitCredentials" } `
        -Method Get |
        Select-Object rabbitmq_version, cluster_name |
        Format-List

    Write-Host "8. Testing SQL Server and checking CatalogServiceDb..."
    Invoke-Compose exec -T sqlserver bash -c '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT name FROM sys.databases ORDER BY name" -b'

    Write-Host ""
    Write-Host "Compose smoke tests passed."
    Write-Host "Catalog API: http://localhost:8081"
    Write-Host "Cart API:    http://localhost:8082"
    Write-Host "RabbitMQ UI: http://localhost:15672"
}
catch {
    Write-Error $_
    Write-Host "`nRecent container logs:"
    & docker compose logs --tail 150
    exit 1
}
finally {
    if (-not $KeepRunning) {
        Write-Host "`nStopping containers (volumes are preserved)..."
        & docker compose down
    }
}
