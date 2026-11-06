$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'PlanetOverview/PlanetOverview.csproj'
$examplePath = Join-Path $PSScriptRoot 'PlanetOverview/appsettings.example.json'
$settingsPath = Join-Path $PSScriptRoot 'PlanetOverview/appsettings.Development.json'
if (-not (Test-Path -LiteralPath $settingsPath)) {
    Copy-Item -LiteralPath $examplePath -Destination $settingsPath
    Write-Host 'Configuracao local criada. Preencha ApiConfigs em PlanetOverview/appsettings.Development.json para consultar as APIs externas.' -ForegroundColor Yellow
}
$localDotnet = Join-Path $env:LOCALAPPDATA 'PlanetOverview/dotnet/dotnet.exe'
if (Test-Path -LiteralPath $localDotnet) {
    $dotnetPath = $localDotnet
} else {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnetCommand) {
        Write-Host 'Instale o SDK .NET 8 e execute novamente. Veja LEIA-ME-GRUPO.md.' -ForegroundColor Red
        exit 1
    }
    $dotnetPath = $dotnetCommand.Source
}
$installedSdks = & $dotnetPath --list-sdks
if (-not $installedSdks) {
    Write-Host 'O SDK .NET e necessario (apenas o Runtime nao basta). Veja LEIA-ME-GRUPO.md.' -ForegroundColor Red
    exit 1
}
Write-Host 'Abra http://localhost:5049 quando o servidor iniciar. Para encerrar, pressione Ctrl+C.' -ForegroundColor Cyan
& $dotnetPath run --project $projectPath --launch-profile http
exit $LASTEXITCODE
