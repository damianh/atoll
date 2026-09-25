<#
.SYNOPSIS
    Updates the Mermaid build bundled in Atoll.Mermaid.

.DESCRIPTION
    Downloads the mermaid npm tarball for the given version, verifies it against the
    registry's published integrity hash, copies the chunked ESM build
    (dist/mermaid.esm.min.mjs + dist/chunks/mermaid.esm.min/*.mjs) and LICENSE into
    src/Atoll.Mermaid/Islands/Assets/vendor/mermaid/, writes manifest.json with a
    SHA-256 hash for every bundled file, and updates the bundled version referenced
    by mermaid-init.js.

    Requires PowerShell 7+.

.EXAMPLE
    ./eng/update-mermaid.ps1 -Version 12.0.1
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string] $Version
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$assetsDir = Join-Path $repoRoot 'src/Atoll.Mermaid/Islands/Assets'
$vendorDir = Join-Path $assetsDir 'vendor/mermaid'
$initScript = Join-Path $assetsDir 'mermaid-init.js'

$work = Join-Path ([IO.Path]::GetTempPath()) "atoll-mermaid-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $work | Out-Null

try {
    Write-Host "Resolving mermaid@$Version from the npm registry..."
    $meta = Invoke-RestMethod "https://registry.npmjs.org/mermaid/$Version"
    $tarballUrl = $meta.dist.tarball
    $integrity = $meta.dist.integrity
    if (-not $integrity -or -not $integrity.StartsWith('sha512-')) {
        throw "Registry did not return a sha512 integrity for mermaid@$Version."
    }

    $tarball = Join-Path $work 'mermaid.tgz'
    Invoke-WebRequest $tarballUrl -OutFile $tarball

    $bytes = [IO.File]::ReadAllBytes($tarball)
    $actual = 'sha512-' + [Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData($bytes))
    if ($actual -ne $integrity) {
        throw "Integrity mismatch for $tarballUrl. Expected $integrity, got $actual."
    }
    Write-Host "Tarball integrity verified ($integrity)."

    tar -xzf $tarball -C $work
    if ($LASTEXITCODE -ne 0) { throw "tar failed with exit code $LASTEXITCODE." }

    $pkg = Join-Path $work 'package'
    $entry = Join-Path $pkg 'dist/mermaid.esm.min.mjs'
    $chunks = Join-Path $pkg 'dist/chunks/mermaid.esm.min'
    if (-not (Test-Path $entry) -or -not (Test-Path $chunks)) {
        throw "mermaid@$Version does not contain the expected chunked ESM build."
    }

    if (Test-Path $vendorDir) { Remove-Item $vendorDir -Recurse -Force }
    New-Item -ItemType Directory -Path (Join-Path $vendorDir 'chunks/mermaid.esm.min') -Force | Out-Null

    Copy-Item $entry (Join-Path $vendorDir 'mermaid.esm.min.mjs')
    Get-ChildItem $chunks -Filter '*.mjs' -File |
        Copy-Item -Destination (Join-Path $vendorDir 'chunks/mermaid.esm.min')
    Copy-Item (Join-Path $pkg 'LICENSE') (Join-Path $vendorDir 'LICENSE')

    $files = [ordered]@{}
    Get-ChildItem $vendorDir -Recurse -File -Filter '*.mjs' |
        ForEach-Object {
            [pscustomobject]@{
                Path = [IO.Path]::GetRelativePath($vendorDir, $_.FullName).Replace('\', '/')
                File = $_
            }
        } |
        Sort-Object Path -CaseSensitive |
        ForEach-Object {
            $files[$_.Path] = (Get-FileHash $_.File.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        }

    $manifest = [ordered]@{
        package   = 'mermaid'
        version   = $Version
        tarball   = $tarballUrl
        integrity = $integrity
        files     = $files
    }
    $json = $manifest | ConvertTo-Json -Depth 5
    [IO.File]::WriteAllText((Join-Path $vendorDir 'manifest.json'), $json.Replace("`r`n", "`n") + "`n")

    $init = [IO.File]::ReadAllText($initScript)
    $pattern = "const BUNDLED_VERSION = '[^']*';"
    if (-not [regex]::IsMatch($init, $pattern)) {
        throw "Could not find BUNDLED_VERSION in $initScript."
    }
    $updated = [regex]::Replace($init, $pattern, "const BUNDLED_VERSION = '$Version';")
    [IO.File]::WriteAllText($initScript, $updated)

    Write-Host "Bundled mermaid@$Version ($($files.Count) files) into $vendorDir."
}
finally {
    Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
}
