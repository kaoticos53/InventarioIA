<#
.SYNOPSIS
    Migra la aplicación desde el directorio temporal al directorio de producción.
.DESCRIPTION
    Este script copia los archivos necesarios desde el directorio temporal al directorio
    de producción, manteniendo la estructura de directorios y excluyendo archivos innecesarios.
    También crea una copia de seguridad del directorio de producción antes de realizar cambios.
.EXAMPLE
    .\Migrate-ToProduction.ps1 -TargetDirectory "..\Inventario" -BackupDirectory "..\Backup"
#>

param (
    [Parameter(Mandatory=$true)]
    [string]$TargetDirectory,
    
    [Parameter(Mandatory=$true)]
    [string]$BackupDirectory,
    
    [switch]$Force = $false
)

# Configuración
$ErrorActionPreference = "Stop"
$script:startTime = Get-Date
$script:sourceDir = $PSScriptRoot
$script:backupDir = $BackupDirectory
$script:targetDir = $TargetDirectory

# Directorios a excluir de la copia
$excludeDirs = @(
    "bin",
    "obj",
    ".vs",
    ".vscode",
    ".git",
    ".idea",
    "node_modules",
    "TestResults",
    "coverage",
    "Migrations"
)

# Archivos a excluir de la copia
$excludeFiles = @(
    "*.user",
    "*.suo",
    "*.userosscache",
    "*.sln.docstates",
    "*.ps1",
    "*.ps1xml",
    "*.psm1",
    "*.psd1",
    "*.ps1xml",
    "*.psc1",
    "*.pssc",
    "*.psrc",
    "*.cdxml",
    "*.bak",
    "*.tmp",
    "*.log",
    "*.swp",
    "_ReSharper*",
    "*.resharper",
    "*.dotCover",
    "*.sln.docstates"
)

# Función para mostrar mensajes de log
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    switch ($Level) {
        "ERROR" { Write-Host $logMessage -ForegroundColor Red }
        "WARN"  { Write-Host $logMessage -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $logMessage -ForegroundColor Green }
        default { Write-Host $logMessage }
    }
}

# Función para crear una copia de seguridad
function Backup-TargetDirectory {
    Write-Log "Creando copia de seguridad del directorio de destino..."
    
    $backupPath = Join-Path -Path $script:backupDir -ChildPath "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    
    try {
        if (-not (Test-Path -Path $script:targetDir)) {
            Write-Log "El directorio de destino no existe. Se creará uno nuevo." "WARN"
            return $true
        }
        
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
        
        # Copiar directorio de destino a la ubicación de respaldo
        Copy-Item -Path "$($script:targetDir)\*" -Destination $backupPath -Recurse -Force
        
        Write-Log "Copia de seguridad creada en: $backupPath" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "Error al crear la copia de seguridad: $_" "ERROR"
        return $false
    }
}

# Función para copiar archivos excluyendo los especificados
function Copy-ProjectFiles {
    param(
        [string]$Source,
        [string]$Destination
    )
    
    Write-Log "Copiando archivos desde $Source a $Destination"
    
    try {
        # Crear directorio de destino si no existe
        if (-not (Test-Path -Path $Destination)) {
            New-Item -ItemType Directory -Path $Destination -Force | Out-Null
        }
        
        # Copiar todos los archivos y directorios excepto los excluidos
        Get-ChildItem -Path $Source -Force | ForEach-Object {
            $relativePath = $_.FullName.Substring($Source.Length).TrimStart('\\', '/')
            $destinationPath = Join-Path -Path $Destination -ChildPath $relativePath
            
            # Verificar si el archivo/directorio debe ser excluido
            $shouldExclude = $false
            
            # Verificar si es un directorio excluido
            if ($_.PSIsContainer) {
                $shouldExclude = $excludeDirs -contains $_.Name
            } 
            # Verificar si es un archivo excluido
            else {
                $shouldExclude = $excludeFiles | Where-Object { $_.EndsWith('*') } | 
                    ForEach-Object { $_.TrimEnd('*') } | 
                    Where-Object { $_.Length -gt 0 -and $_.Name -like "$($_.TrimEnd('*'))*" }
                
                if (-not $shouldExclude) {
                    $shouldExclude = $excludeFiles -contains $_.Name
                }
            }
            
            if (-not $shouldExclude) {
                if ($_.PSIsContainer) {
                    # Crear directorio de destino
                    if (-not (Test-Path -Path $destinationPath)) {
                        New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
                    }
                    
                    # Copiar contenido del directorio recursivamente
                    Copy-ProjectFiles -Source $_.FullName -Destination $destinationPath
                }
                else {
                    # Copiar archivo
                    Copy-Item -Path $_.FullName -Destination $destinationPath -Force
                }
                
                Write-Log "Copiado: $relativePath"
            }
            else {
                Write-Log "Excluido: $relativePath" "WARN"
            }
        }
        
        return $true
    }
    catch {
        Write-Log "Error al copiar archivos: $_" "ERROR"
        return $false
    }
}

# Función principal
function Invoke-Migration {
    Write-Log "Iniciando migración de la aplicación..." "INFO"
    
    # Validar directorio de origen
    if (-not (Test-Path -Path $script:sourceDir)) {
        Write-Log "El directorio de origen no existe: $script:sourceDir" "ERROR"
        exit 1
    }
    
    # Validar directorio de destino
    if (Test-Path -Path $script:targetDir) {
        if (-not $Force) {
            $confirmation = Read-Host "El directorio de destino ya existe. ¿Desea continuar? (S/N)"
            if ($confirmation -ne "S" -and $confirmation -ne "s") {
                Write-Log "Migración cancelada por el usuario." "WARN"
                exit 0
            }
        }
        
        # Crear copia de seguridad
        if (-not (Backup-TargetDirectory)) {
            $confirmation = Read-Host "No se pudo crear la copia de seguridad. ¿Desea continuar de todos modos? (S/N)"
            if ($confirmation -ne "S" -and $confirmation -ne "s") {
                Write-Log "Migración cancelada por el usuario." "WARN"
                exit 0
            }
        }
    }
    
    # Crear directorio de destino si no existe
    if (-not (Test-Path -Path $script:targetDir)) {
        New-Item -ItemType Directory -Path $script:targetDir -Force | Out-Null
    }
    
    # Copiar archivos
    if (Copy-ProjectFiles -Source $script:sourceDir -Destination $script:targetDir) {
        $elapsedTime = (Get-Date) - $script:startTime
        Write-Log "Migración completada exitosamente en $($elapsedTime.TotalSeconds.ToString('0.00')) segundos" "SUCCESS"
        Write-Log "Directorio de destino: $script:targetDir" "SUCCESS"
    }
    else {
        Write-Log "Error durante la migración. Revise los mensajes de error anteriores." "ERROR"
        exit 1
    }
}

# Ejecutar migración
Invoke-Migration
