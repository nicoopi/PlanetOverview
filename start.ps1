$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'PlanetOverview/PlanetOverview.csproj'
$localDotnet = Join-Path $env:LOCALAPPDATA 'PlanetOverview/dotnet/dotnet.exe'
if (Test-Path -LiteralPath $localDotnet) {
    & $localDotnet run --project $projectPath --launch-profile http
} else {
    & dotnet run --project $projectPath --launch-profile http
}
exit $LASTEXITCODE
