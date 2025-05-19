<#
.SYNOPSIS
    Limpia la solución eliminando bin, obj, node_modules y archivos temporales.
.DESCRIPTION
    Este script elimina todos los directorios de compilación y archivos temporales
    para forzar una reconstrucción limpia del proyecto.
#>

Write-Host "Limpiando la solución..." -ForegroundColor Green

# Directorios a eliminar
$directoriesToDelete = @(
    "**/bin",
    "**/obj",
    "**/node_modules",
    "**/.vs",
    "**/.vscode",
    "**/.idea",
    "**/TestResults",
    "**/coverage"
)

# Archivos a eliminar
$filesToDelete = @(
    "**/*.user",
    "**/*.suo",
    "**/*.userosscache",
    "**/*.sln.docstates",
    "**/project.lock.json",
    "**/package-lock.json",
    "**/yarn.lock",
    "**/npm-debug.log"
)

# Eliminar directorios
foreach ($dir in $directoriesToDelete) {
    Get-ChildItem -Path . -Include $dir -Recurse -Force | ForEach-Object {
        Write-Host "Eliminando directorio: $($_.FullName)" -ForegroundColor Yellow
        Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Eliminar archivos
foreach ($file in $filesToDelete) {
    Get-ChildItem -Path . -Include $file -Recurse -Force | ForEach-Object {
        Write-Host "Eliminando archivo: $($_.FullName)" -ForegroundColor Yellow
        Remove-Item -Path $_.FullName -Force -ErrorAction SilentlyContinue
    }
}

# Limpiar paquetes NuGet
if (Test-Path "packages") {
    Write-Host "Limpiando paquetes NuGet..." -ForegroundColor Green
    Remove-Item -Path "packages" -Recurse -Force -ErrorAction SilentlyContinue
}

# Limpiar caché de .NET
Write-Host "Limpiando caché de .NET..." -ForegroundColor Green
dotnet nuget locals all --clear

Write-Host "Limpieza completada. Ejecuta 'dotnet restore' y 'dotnet build' para reconstruir la solución." -ForegroundColor Green
