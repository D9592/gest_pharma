$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

$appName = 'MiniPharmacie'
$installDirectory = Join-Path $env:LOCALAPPDATA 'Programs\MiniPharmacie'
$startMenuDirectory = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\MiniPharmacie'
$desktopShortcut = Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'MiniPharmacie.lnk'
$uninstallKeyPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\MiniPharmacie'

if (Get-Process -Name 'MiniPharmacie.App' -ErrorAction SilentlyContinue) {
    [System.Windows.Forms.MessageBox]::Show('Fermez MiniPharmacie avant de la désinstaller.', 'Désinstallation', 'OK', 'Warning') | Out-Null
    exit 1
}

$answer = [System.Windows.Forms.MessageBox]::Show(
    "Désinstaller MiniPharmacie ?`n`nLa base de données et les sauvegardes de votre profil Windows seront conservées.",
    'Confirmer la désinstallation',
    [System.Windows.Forms.MessageBoxButtons]::YesNo,
    [System.Windows.Forms.MessageBoxIcon]::Question
)
if ($answer -ne [System.Windows.Forms.DialogResult]::Yes) { exit 0 }

Set-Location -LiteralPath $env:TEMP
Remove-Item -LiteralPath $startMenuDirectory -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $desktopShortcut -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $uninstallKeyPath -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installDirectory -Recurse -Force

[System.Windows.Forms.MessageBox]::Show(
    'MiniPharmacie a été désinstallée. Les données et les sauvegardes ont été conservées.',
    'Désinstallation terminée',
    [System.Windows.Forms.MessageBoxButtons]::OK,
    [System.Windows.Forms.MessageBoxIcon]::Information
) | Out-Null
