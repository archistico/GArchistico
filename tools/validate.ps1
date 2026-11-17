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
    # La pulizia senza -c rimuove normalmente Debug: per evitare mix di DLL
    # precedenti e sorgenti R3, pulire la STESSA configurazione che compiliamo.
    Run-DotNet -Arguments @('clean', 'GenerativeArt.sln', '-c', 'Release')
    Run-DotNet -Arguments @('restore', 'GenerativeArt.sln')
    Run-DotNet -Arguments @('build', 'GenerativeArt.sln', '--no-restore', '-c', 'Release')
    # Gate rapido: prima della suite completa, il catalogo effettivamente
    # compilato deve avere i controlli Crinis, i preset G4 e il catalogo Cincinnus C5.
    Run-DotNet -Arguments @('test', 'tests/GenerativeArt.Application.Tests/GenerativeArt.Application.Tests.csproj',
        '--no-build', '--no-restore', '-c', 'Release',
        '--filter', 'FullyQualifiedName~CrinisMicroCatalogTests|FullyQualifiedName~CrinisReleaseCatalogContractTests|FullyQualifiedName~GartG4CatalogTests|FullyQualifiedName~CincinnusC5CatalogTests',
        '--logger', 'console;verbosity=minimal')
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
