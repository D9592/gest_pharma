param(
    [Parameter(Mandatory = $true)][string]$PublishDirectory,
    [Parameter(Mandatory = $true)][string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$iexpress = Join-Path $env:WINDIR 'System32\iexpress.exe'
if (-not (Test-Path -LiteralPath $iexpress)) {
    throw 'IExpress est introuvable; la création du paquet est impossible.'
}

$publishPath = (Resolve-Path -LiteralPath $PublishDirectory).Path
$outputPath = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('MiniPharmacie-Setup-' + [guid]::NewGuid().ToString('N'))
$payloadPath = Join-Path $temporaryRoot 'payload'
New-Item -ItemType Directory -Path $payloadPath -Force | Out-Null

try {
    Get-ChildItem -LiteralPath $publishPath -File | Where-Object { $_.Extension -ne '.pdb' } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $payloadPath $_.Name) -Force
    }
    foreach ($file in @(
        (Join-Path $repositoryRoot 'installer\install.ps1'),
        (Join-Path $repositoryRoot 'installer\uninstall.ps1'),
        (Join-Path $repositoryRoot 'LICENSE.txt'),
        (Join-Path $repositoryRoot 'README.md')
    )) {
        if (-not (Test-Path -LiteralPath $file)) { throw "Fichier de paquet manquant : $file" }
        Copy-Item -LiteralPath $file -Destination (Join-Path $payloadPath (Split-Path -Leaf $file)) -Force
    }

    $packageFiles = @(Get-ChildItem -LiteralPath $payloadPath -File | Sort-Object Name)
    if (-not ($packageFiles | Where-Object { $_.Extension -eq '.exe' })) {
        throw 'Aucun programme autonome trouvé dans le dossier publié.'
    }
    if ($packageFiles.Count -gt 30) { throw 'Le paquet publié dépasse la limite prise en charge par le script IExpress.' }

    $fileMappings = for ($i = 0; $i -lt $packageFiles.Count; $i++) {
        "FILE$i=$($packageFiles[$i].Name)"
    }
    $fileEntries = for ($i = 0; $i -lt $packageFiles.Count; $i++) {
        "%FILE$i%="
    }
    $targetName = Join-Path $temporaryRoot 'MiniPharmacie-Setup.exe'
    $sedPath = Join-Path $temporaryRoot 'MiniPharmacie.sed'
    $sed = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=1
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=1
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
SourceFiles=SourceFiles
[Strings]
InstallPrompt="Install MiniPharmacie for this Windows account?"
DisplayLicense=LICENSE.txt
FinishMessage="MiniPharmacie is installed. Your data remains in your Windows profile."
TargetName=$targetName
FriendlyName="MiniPharmacie Setup"
AppLaunched="powershell.exe -NoProfile -ExecutionPolicy Bypass -File install.ps1"
$($fileMappings -join "`r`n")
[SourceFiles]
SourceFiles0=$payloadPath\
[SourceFiles0]
$($fileEntries -join "`r`n")
"@
    [IO.File]::WriteAllText($sedPath, $sed, [Text.Encoding]::ASCII)

    & $iexpress /N /Q /M $sedPath
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $targetName)) {
        throw "Échec IExpress (code $LASTEXITCODE)."
    }

    $installerPath = Join-Path $outputPath 'MiniPharmacie-Setup.exe'
    Copy-Item -LiteralPath $targetName -Destination $installerPath -Force
    Write-Output "Installeur généré : $installerPath"
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot) {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
