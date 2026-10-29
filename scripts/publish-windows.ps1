param(
    [string]$RuntimeIdentifier = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repositoryRoot 'MiniPharmacie.App\MiniPharmacie.App.csproj'
$publishDirectory = Join-Path $repositoryRoot "artifacts\publish\$RuntimeIdentifier"
$installerDirectory = Join-Path $repositoryRoot "artifacts\installer\$RuntimeIdentifier"

& dotnet publish $project -c Release -r $RuntimeIdentifier --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDirectory
if ($LASTEXITCODE -ne 0) { throw "La publication .NET a échoué (code $LASTEXITCODE)." }

$application = Join-Path $publishDirectory 'MiniPharmacie.App.exe'
if (-not (Test-Path -LiteralPath $application)) { throw 'Le programme publié est absent.' }

& (Join-Path $PSScriptRoot 'build-installer.ps1') -PublishDirectory $publishDirectory -OutputDirectory $installerDirectory
if ($LASTEXITCODE -ne 0) { throw "La generation de l'installeur a echoue (code $LASTEXITCODE)." }

Write-Output "Exécutable autonome : $application"
Write-Output "Installeur par utilisateur : $(Join-Path $installerDirectory 'MiniPharmacie-Setup.exe')"
