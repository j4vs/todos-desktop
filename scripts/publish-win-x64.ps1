$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectPath = Join-Path $repositoryRoot 'src\Todos.App\Todos.App.csproj'
$publishDirectory = Join-Path $repositoryRoot 'publish\win-x64'
$executablePath = Join-Path $publishDirectory 'Todos.exe'
$projectXml = [xml](Get-Content -LiteralPath $projectPath -Raw)
$version = [string]($projectXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1)
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'La versión del proyecto no es válida.' }
$archiveName = "Todos-$version-win-x64.zip"
$archivePath = Join-Path $repositoryRoot "publish\$archiveName"
$checksumPath = Join-Path $repositoryRoot "publish\Todos-$version-win-x64.sha256"
$noticesPath = Join-Path $repositoryRoot 'THIRD-PARTY-NOTICES.md'

Push-Location $repositoryRoot
try {
    dotnet restore $projectPath --runtime win-x64
    if ($LASTEXITCODE -ne 0) { throw 'No fue posible restaurar los runtime packs.' }

    dotnet publish $projectPath --configuration Release --runtime win-x64 `
        --self-contained true --no-restore --property:PublishProfile=win-x64
    if ($LASTEXITCODE -ne 0) { throw 'No fue posible publicar Todo''s.' }

    if (-not (Test-Path -LiteralPath $executablePath)) {
        throw "No se generó el ejecutable esperado: $executablePath"
    }

    Compress-Archive -LiteralPath @($executablePath, $noticesPath) -DestinationPath $archivePath `
        -CompressionLevel Optimal -Force

    $executableHash = (Get-FileHash -LiteralPath $executablePath -Algorithm SHA256).Hash
    $archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
    @(
        "$executableHash  Todos.exe"
        "$archiveHash  $archiveName"
    ) | Set-Content -LiteralPath $checksumPath -Encoding utf8

    Write-Host "Paquete listo: $archivePath"
    Write-Host "Archivo SHA-256: $checksumPath"
}
finally {
    Pop-Location
}
