param([string]$OutputDirectory = 'artifacts/production-4k')
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    dotnet build tools/GenerativeArt.FourKValidation -c Release --no-restore --verbosity quiet
    if ($LASTEXITCODE) { throw 'Build del validatore 4K fallita.' }
    $worker = 'tools/GenerativeArt.FourKValidation/bin/Release/net10.0/GenerativeArt.FourKValidation.dll'
    dotnet $worker prepare $OutputDirectory
    if ($LASTEXITCODE) { throw 'Convalida preliminare 4K fallita.' }
    $failed = @()
    foreach ($recipe in Get-ChildItem (Join-Path $OutputDirectory 'recipes') -Filter '*.json' | Sort-Object Name) {
        dotnet $worker render $recipe.FullName (Join-Path $OutputDirectory 'renders')
        if ($LASTEXITCODE) { $failed += $recipe.Name }
    }
    if ($failed.Count) { throw ('Render 4K falliti: ' + ($failed -join ', ')) }
    dotnet $worker sheet (Join-Path $OutputDirectory 'renders') (Join-Path $OutputDirectory 'contact-sheet.png')
    if ($LASTEXITCODE) { throw 'Contact sheet fallita.' }
}
finally { Pop-Location }
