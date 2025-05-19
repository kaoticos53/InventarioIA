# Script de despliegue para el Sistema de Inventario
# Este script automatiza el proceso de despliegue de la aplicación en contenedores Docker
#
# Uso: .\scripts\deploy.ps1 [entorno]
# Ejemplos:
#   .\scripts\deploy.ps1                  # Despliega en entorno de producción (por defecto)
#   .\scripts\deploy.ps1 development       # Despliega en entorno de desarrollo
#   .\scripts\deploy.ps1 testing           # Despliega en entorno de pruebas
#
# Requisitos:
#   - Docker Desktop instalado y en ejecución
#   - Docker Compose disponible
#   - Permisos de administrador para ejecutar comandos Docker

param(
    [string]$environment = "production"
)

# Función para verificar si un comando falló
# Esta función comprueba el código de salida del último comando ejecutado
# y muestra un mensaje de error si el comando falló
function Check-CommandStatus {
    param(
        [string]$command,      # Comando que se ejecutó
        [string]$errorMessage  # Mensaje de error a mostrar si el comando falló
    )
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: $errorMessage" -ForegroundColor Red
        Write-Host "Código de salida: $LASTEXITCODE" -ForegroundColor Red
        Write-Host "Comando: $command" -ForegroundColor Red
        exit 1
    }
}

try {
    # Mostrar información del sistema
    Write-Host "=== Sistema de Despliegue de Inventario ===" -ForegroundColor Cyan
    Write-Host "Fecha: $(Get-Date)" -ForegroundColor Cyan
    Write-Host "Directorio actual: $PSScriptRoot" -ForegroundColor Cyan
    Write-Host "Entorno: $environment" -ForegroundColor Cyan

    # Verificar si Docker está instalado y en ejecución
    # Esta comprobación es crucial ya que todo el despliegue depende de Docker
    Write-Host "`nVerificando Docker..." -ForegroundColor Yellow
    docker info | Out-Null
    Check-CommandStatus "docker info" "Docker no está en ejecución o no está instalado correctamente"
    Write-Host "Docker está funcionando correctamente" -ForegroundColor Green

    # Detener y eliminar contenedores existentes
    # Esto asegura que no haya conflictos con contenedores anteriores
    Write-Host "`nDeteniendo contenedores existentes..." -ForegroundColor Yellow
    docker-compose down --remove-orphans
    Check-CommandStatus "docker-compose down" "Error al detener los contenedores"
    Write-Host "Contenedores anteriores detenidos y eliminados correctamente" -ForegroundColor Green

    # Limpiar recursos no utilizados (solo imágenes no utilizadas)
    # Esto libera espacio en disco eliminando imágenes antiguas y recursos no utilizados
    Write-Host "`nLimpiando recursos no utilizados..." -ForegroundColor Yellow
    docker system prune -f --filter "until=24h"
    Write-Host "Limpieza de recursos completada" -ForegroundColor Green

    # Reconstruir y ejecutar contenedores
    # Selecciona el archivo docker-compose adecuado según el entorno
    Write-Host "`nConstruyendo y ejecutando contenedores..." -ForegroundColor Yellow
    Write-Host "Entorno seleccionado: $environment" -ForegroundColor Cyan
    
    # En entorno de desarrollo, usamos el archivo override que contiene configuraciones adicionales
    # para facilitar el desarrollo (como volúmenes montados para hot-reload)
    $composeCommand = if ($environment -eq "development") {
        "docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build --force-recreate"
    } else {
        "docker-compose -f docker-compose.yml up -d --build --force-recreate"
    }
    
    Write-Host "Ejecutando: $composeCommand" -ForegroundColor Cyan
    
    Invoke-Expression $composeCommand
    Check-CommandStatus $composeCommand "Error al construir o iniciar los contenedores"

    # Esperar a que los servicios estén listos
    # Verificamos que la API esté respondiendo antes de continuar
    # Esto es importante para asegurar que podemos ejecutar las migraciones
    Write-Host "`nEsperando a que los servicios estén listos..." -ForegroundColor Yellow
    Write-Host "Comprobando el estado de la API en http://localhost:5000/health" -ForegroundColor Cyan
    
    $maxAttempts = 30      # Número máximo de intentos (30 * 5 segundos = 2.5 minutos)
    $attempt = 0           # Contador de intentos
    $apiReady = $false     # Bandera para indicar si la API está lista
    
    while ($attempt -lt $maxAttempts -and -not $apiReady) {
        $attempt++
        try {
            $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get -TimeoutSec 5 -ErrorAction Stop
            if ($health.status -eq "Healthy") {
                $apiReady = $true
                Write-Host "La API está lista después de $attempt intentos" -ForegroundColor Green
            }
        } catch {
            Write-Host "Esperando a que la API esté lista (intento $attempt/$maxAttempts)..."
            Start-Sleep -Seconds 5
        }
    }

    if (-not $apiReady) {
        Write-Host "Advertencia: La API no está respondiendo después de $maxAttempts intentos" -ForegroundColor Yellow
    } else {
        # Ejecutar migraciones
        Write-Host "`nEjecutando migraciones..." -ForegroundColor Yellow
        docker-compose exec -T api dotnet ef database update
        Check-CommandStatus "dotnet ef database update" "Error al ejecutar migraciones"
    }

    # Mostrar resumen del despliegue
    # Proporciona información útil sobre cómo acceder a los servicios desplegados
    Write-Host "`n=== Resumen del Despliegue ===" -ForegroundColor Green
    Write-Host "Estado: Completado" -ForegroundColor Green
    Write-Host "Fecha y hora: $(Get-Date)" -ForegroundColor Cyan
    
    # URLs y puntos de acceso
    Write-Host "`nPuntos de acceso:" -ForegroundColor Yellow
    Write-Host "- Aplicación web: http://localhost:8080" -ForegroundColor Cyan
    Write-Host "- API: http://localhost:5000" -ForegroundColor Cyan
    Write-Host "- API Health: http://localhost:5000/health" -ForegroundColor Cyan
    Write-Host "- SQL Server: localhost,1433" -ForegroundColor Cyan
    Write-Host "  Usuario: sa" -ForegroundColor Cyan
    Write-Host "  Contraseña: (configurada en el archivo docker-compose.yml)" -ForegroundColor Cyan
    
    # Comandos útiles para gestionar los contenedores
    Write-Host "`nComandos útiles:" -ForegroundColor Yellow
    Write-Host "- Ver logs en tiempo real: docker-compose logs -f" -ForegroundColor Cyan
    Write-Host "- Ver logs de un servicio específico: docker-compose logs -f [servicio]" -ForegroundColor Cyan
    Write-Host "- Detener todos los servicios: docker-compose down" -ForegroundColor Cyan
    Write-Host "- Reiniciar un servicio: docker-compose restart [servicio]" -ForegroundColor Cyan
    
    Write-Host "`nDespliegue completado exitosamente!" -ForegroundColor Green

} catch {
    Write-Host "`nError durante el despliegue: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
