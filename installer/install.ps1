$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

$packageDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$appName = 'MiniPharmacie'
$installDirectory = Join-Path $env:LOCALAPPDATA 'Programs\MiniPharmacie'
$startMenuDirectory = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\MiniPharmacie'
$desktopShortcut = Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'MiniPharmacie.lnk'

if (Get-Process -Name 'MiniPharmacie.App' -ErrorAction SilentlyContinue) {
    throw 'Fermez MiniPharmacie avant toute installation ou mise à jour.'
}

$appFiles = @(Get-ChildItem -LiteralPath $packageDirectory -File | Where-Object {
    $_.Name -notin @('install.ps1', 'uninstall.ps1', 'LICENSE.txt', 'README.md') -and $_.Extension -ne '.pdb'
})
$application = $appFiles | Where-Object { $_.Extension -eq '.exe' } | Select-Object -First 1
if ($null -eq $application) {
    throw 'Le programme MiniPharmacie.App.exe est absent du paquet.'
}

New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
foreach ($file in $appFiles) {
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $installDirectory $file.Name) -Force
}
foreach ($supportFile in @('uninstall.ps1', 'LICENSE.txt', 'README.md')) {
    $source = Join-Path $packageDirectory $supportFile
    if (Test-Path -LiteralPath $source) {
        Copy-Item -LiteralPath $source -Destination (Join-Path $installDirectory $supportFile) -Force
    }
}

$installedExecutable = Join-Path $installDirectory $application.Name
New-Item -ItemType Directory -Path $startMenuDirectory -Force | Out-Null
$shell = New-Object -ComObject WScript.Shell
foreach ($shortcutPath in @(
    (Join-Path $startMenuDirectory 'MiniPharmacie.lnk'),
    $desktopShortcut
)) {
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $installedExecutable
    $shortcut.WorkingDirectory = $installDirectory
    $shortcut.Description = 'Gestion de la mini-pharmacie'
    $shortcut.IconLocation = $installedExecutable
    $shortcut.Save()
}

$uninstallKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\MiniPharmacie'
$uninstallCommand = '"{0}" -NoProfile -ExecutionPolicy Bypass -File "{1}"' -f `
    (Join-Path $PSHOME 'powershell.exe'), (Join-Path $installDirectory 'uninstall.ps1')
New-Item -Path $uninstallKeyPath -Force | Out-Null
New-ItemProperty -Path $uninstallKeyPath -Name 'DisplayName' -Value $appName -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKeyPath -Name 'DisplayVersion' -Value '1.0.0' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKeyPath -Name 'Publisher' -Value 'MiniPharmacie contributors' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKeyPath -Name 'InstallLocation' -Value $installDirectory -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKeyPath -Name 'UninstallString' -Value $uninstallCommand -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKeyPath -Name 'DisplayIcon' -Value $installedExecutable -PropertyType String -Force | Out-Null

[System.Windows.Forms.MessageBox]::Show(
    "MiniPharmacie est installée.`n`nLa base et les sauvegardes restent dans votre profil Windows et ne sont pas supprimées lors de la désinstallation.",
    'Installation terminée',
    [System.Windows.Forms.MessageBoxButtons]::OK,
    [System.Windows.Forms.MessageBoxIcon]::Information
) | Out-Null
