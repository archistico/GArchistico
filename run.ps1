#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$exitCode = 0
$previousTelemetry = $env:AVALONIA_TELEMETRY_OPTOUT
Push-Location -LiteralPath $PSScriptRoot
try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw 'SDK .NET non trovato. Installare .NET SDK 10 e riprovare.'
    }
    $env:AVALONIA_TELEMETRY_OPTOUT = '1'
    $arguments = @('run', '--project', (Join-Path $PSScriptRoot 'src\GenerativeArt.Studio\GenerativeArt.Studio.csproj'),
        '--configuration', $Configuration)
    if ($NoBuild) { $arguments += '--no-build' }
    Write-Host "Avvio GenerativeArt Studio ($Configuration)..."
    & dotnet @arguments
    $exitCode = $LASTEXITCODE
}
catch {
    [Console]::Error.WriteLine("Avvio non riuscito: $($_.Exception.Message)")
    $exitCode = 1
}
finally {
    $env:AVALONIA_TELEMETRY_OPTOUT = $previousTelemetry
    Pop-Location
}
exit $exitCode
