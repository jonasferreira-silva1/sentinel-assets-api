# Reseta o PostgreSQL - use quando receber "password authentication failed"
# Remove o volume antigo e recria o container com credenciais corretas

$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

Write-Host "Parando containers..." -ForegroundColor Yellow
docker compose down -v

Write-Host "Iniciando PostgreSQL limpo..." -ForegroundColor Green
docker compose up -d

Write-Host "Aguardando 5 segundos para o Postgres inicializar..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

$apiPath = Join-Path $projectRoot "src\SentinelAssetsAPI"
Set-Location $apiPath
Write-Host "Aplicando migrations..." -ForegroundColor Green
dotnet ef database update

Write-Host "`nPronto! Banco resetado. Execute 'dotnet run' para iniciar a API." -ForegroundColor Green
