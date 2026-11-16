#requires -Version 5.1
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $root
try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw 'SDK .NET non trovato. Installare .NET SDK 10 e riprovare.'
    }

    function Run-DotNet([string[]]$Arguments) {
        Write-Host ("dotnet " + ($Arguments -join ' '))
        & dotnet @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Comando dotnet fallito (exit code $LASTEXITCODE)."
        }
    }
    Run-DotNet -Arguments @('--version')
    Run-DotNet -Arguments @('restore', 'GenerativeArt.sln')
    Run-DotNet -Arguments @('build', 'GenerativeArt.sln', '--no-restore', '-c', 'Release')
    Run-DotNet -Arguments @('test', 'GenerativeArt.sln', '--no-build', '-c', 'Release', '--logger', 'console;verbosity=normal')
    Write-Host 'Build e test .NET completati con successo.' -ForegroundColor Green
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Pop-Location
}
