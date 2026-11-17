#requires -Version 5.1
[CmdletBinding(SupportsShouldProcess = $true)]
param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $PSScriptRoot).ProviderPath.TrimEnd('\', '/')
$rootPrefix = $root + [IO.Path]::DirectorySeparatorChar
$packageCache = [IO.Path]::GetFullPath((Join-Path $root 'artifacts\nuget'))
$pending = New-Object 'System.Collections.Generic.Stack[string]'
$targets = New-Object 'System.Collections.Generic.List[string]'
$pending.Push($root)

try {
    # Non seguire junction/symlink o attraversare cache di dipendenze.
    while ($pending.Count -gt 0) {
        foreach ($directory in Get-ChildItem -LiteralPath $pending.Pop() -Directory -Force) {
            if (($directory.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { continue }
            if ($directory.Name -in @('.git', '.nuget', 'node_modules') -or
                $directory.FullName -eq $packageCache -or
                (Test-Path -LiteralPath (Join-Path $directory.FullName '.nupkg.metadata'))) { continue }
            if ($directory.Name -in @('bin', 'obj')) {
                $targets.Add($directory.FullName)
            }
            else { $pending.Push($directory.FullName) }
        }
    }

    $removed = 0
    foreach ($target in $targets) {
        # Verifica il percorso assoluto PRIMA di ogni eliminazione ricorsiva.
        $resolved = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $target).ProviderPath)
        if (-not $resolved.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase) -or
            (Split-Path -Leaf $resolved) -notin @('bin', 'obj')) {
            throw "Percorso di pulizia non valido: $resolved"
        }
        $check = New-Object 'System.Collections.Generic.Stack[string]'
        $check.Push($resolved)
        while ($check.Count -gt 0) {
            foreach ($entry in Get-ChildItem -LiteralPath $check.Pop() -Force) {
                if (($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                    throw "La cartella contiene un collegamento: $($entry.FullName). Pulizia interrotta."
                }
                if ($entry.PSIsContainer) { $check.Push($entry.FullName) }
            }
        }
        if ($PSCmdlet.ShouldProcess($resolved, 'Elimina cartella bin/obj')) {
            Remove-Item -LiteralPath $resolved -Recurse -Force
            $removed++
            Write-Host "Eliminata: $resolved"
        }
    }
    if ($WhatIfPreference) { Write-Host "Cartelle bin/obj individuate: $($targets.Count)." }
    else { Write-Host "Pulizia completata: $removed cartelle eliminate." -ForegroundColor Green }
}
catch {
    [Console]::Error.WriteLine("Pulizia non riuscita: $($_.Exception.Message)")
    exit 1
}
exit 0
