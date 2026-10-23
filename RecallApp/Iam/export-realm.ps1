<#
.SYNOPSIS
    Экспорт realm Keycloak в указанную папку.

.DESCRIPTION
    Запускает `kc.sh export` внутри контейнера iam (с пользователями),
    затем копирует результат на хост в <OutDir>\<realm>-<timestamp>.
    Стек должен быть поднят (docker compose up).

.PARAMETER OutDir
    Папка назначения на хосте.

.PARAMETER Realm
    Имя realm. По умолчанию recallapp.

.EXAMPLE
    ./Iam/export-realm.ps1 -OutDir D:\backups\recallapp
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $OutDir,
    [string] $Realm = "recallapp"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent   # корень RecallApp (docker-compose.yml)
$svc = "iam"
$inside = "/tmp/kc-export"

Push-Location $root
try {
    Write-Host "Экспорт realm '$Realm' внутри контейнера '$svc'..."
    docker compose exec -T $svc /opt/keycloak/bin/kc.sh export --dir $inside --users realm_file --realm $Realm
    if ($LASTEXITCODE -ne 0) { throw "kc.sh export завершился с кодом $LASTEXITCODE" }

    if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
    $dest = Join-Path $OutDir ("{0}-{1}" -f $Realm, (Get-Date -Format "yyyyMMdd-HHmmss"))

    Write-Host "Копирование из контейнера в '$dest'..."
    docker compose cp "${svc}:$inside" $dest
    docker compose exec -T $svc rm -rf $inside

    Write-Host "Готово: $dest"
}
finally {
    Pop-Location
}
