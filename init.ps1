# Script de configuración inicial para el sistema de inventario
# Uso: .\init.ps1 [opciones]

param(
    [string]$environment = "production",
    [switch]$skipDockerCheck = $false,
    [switch]$skipDependencies = $false,
    [string]$dbPassword = "YourStrong@Passw0rd",
    [string]$jwtSecret = "YourJwtSecretKey12345678901234567890"
)

# Configuración
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Colores para la salida
$colors = @{
    "reset" = "\x1B[0m"
    "red" = "\x1B[31m"
    "green" = "\x1B[32m"
    "yellow" = "\x1B[33m"
    "blue" = "\x1B[34m"
    "magenta" = "\x1B[35m"
    "cyan" = "\x1B[36m"
    "white" = "\x1B[37m"
}

# Función para mostrar mensajes de éxito
function Write-Success {
    param([string]$message)
    Write-Host "[${green}✓${white}] $message"
}

# Función para mostrar mensajes de error
function Write-Error {
    param([string]$message)
    Write-Host "[${red}✗${white}] $message" -ForegroundColor Red
}

# Función para mostrar mensajes de advertencia
function Write-Warning {
    param([string]$message)
    Write-Host "[${yellow}!${white}] $message" -ForegroundColor Yellow
}

# Función para mostrar mensajes de información
function Write-Info {
    param([string]$message)
    Write-Host "[${blue}i${white}] $message" -ForegroundColor Cyan
}

# Función para verificar si un comando está disponible
function Test-CommandExists {
    param([string]$command)
    $exists = $null -ne (Get-Command $command -ErrorAction SilentlyContinue)
    return $exists
}

try {
    # Mostrar encabezado
    Write-Host "\n${cyan}=== Configuración del Sistema de Inventario ===${white}"
    Write-Host "Fecha: $(Get-Date)"
    Write-Host "Directorio actual: $PWD"
    Write-Host "Entorno: $environment"
    Write-Host "DB Password: $dbPassword"
    Write-Host "JWT Secret: $jwtSecret"

    # Verificar requisitos previos
    Write-Info "Verificando requisitos previos..."

    # Verificar Docker
    if (-not $skipDockerCheck) {
        if (-not (Test-CommandExists "docker")) {
            Write-Error "Docker no está instalado. Por favor, instala Docker Desktop desde https://www.docker.com/products/docker-desktop"
            exit 1
        }
        Write-Success "Docker está instalado"

        # Verificar si Docker está en ejecución
        try {
            docker info | Out-Null
            Write-Success "Docker está en ejecución"
        } catch {
            Write-Error "Docker no está en ejecución. Por favor, inicia Docker Desktop y espera a que esté listo"
            exit 1
        }
    } else {
        Write-Warning "Se omitió la verificación de Docker"
    }

    # Verificar dependencias
    if (-not $skipDependencies) {
        Write-Info "Verificando dependencias..."
        
        # Verificar dotnet
        if (-not (Test-CommandExists "dotnet")) {
            Write-Error ".NET SDK no está instalado. Por favor, instala .NET 9.0 SDK desde https://dotnet.microsoft.com/download/dotnet/9.0"
            exit 1
        }
        Write-Success ".NET SDK está instalado"
        
        # Verificar node
        if (-not (Test-CommandExists "node")) {
            Write-Warning "Node.js no está instalado. Algunas características pueden no funcionar correctamente"
        } else {
            Write-Success "Node.js está instalado"
        }
    } else {
        Write-Warning "Se omitió la verificación de dependencias"
    }

    # Actualizar variables de entorno
    Write-Info "Configurando variables de entorno..."
    
    # Crear archivo .env si no existe
    $envFile = ".env"
    if (-not (Test-Path $envFile)) {
        @"
# Configuración de la base de datos
DB_SA_PASSWORD=$dbPassword
DB_NAME=InventarioDB

# Configuración de la API
JWT_SECRET=$jwtSecret
API_PORT=5000
WEB_PORT=8080

# Configuración de correo (ejemplo para Mailtrap)
SMTP_SERVER=smtp.mailtrap.io
SMTP_PORT=2525
SMTP_USERNAME=your_mailtrap_username
SMTP_PASSWORD=your_mailtrap_password
SMTP_FROM_EMAIL=no-reply@inventario.com
SMTP_FROM_NAME=Sistema de Inventario
"@ | Out-File -FilePath $envFile -Encoding utf8
        
        Write-Success "Archivo .env creado correctamente"
    } else {
        Write-Info "El archivo .env ya existe, se mantendrá sin cambios"
    }

    # Actualizar docker-compose.yml con las credenciales
    $dockerComposeFile = "docker-compose.yml"
    if (Test-Path $dockerComposeFile) {
        $dockerComposeContent = Get-Content $dockerComposeFile -Raw
        $dockerComposeContent = $dockerComposeContent -replace 'YourStrong@Passw0rd', $dbPassword
        $dockerComposeContent = $dockerComposeContent -replace 'YourJwtSecretKey12345678901234567890', $jwtSecret
        $dockerComposeContent | Set-Content $dockerComposeFile -Encoding utf8
        Write-Success "Archivo docker-compose.yml actualizado con las credenciales"
    }

    # Mostrar resumen
    Write-Host "\n${green}=== Configuración completada con éxito ===${white}"
    Write-Host "Para iniciar la aplicación, ejecuta: ${cyan}.\scripts\deploy.ps1${white}"
    Write-Host "Una vez iniciada, la aplicación estará disponible en: ${cyan}http://localhost:8080${white}"
    Write-Host "La API estará disponible en: ${cyan}http://localhost:5000${white}"
    Write-Host "\nPara ver los logs de los contenedores: ${cyan}docker-compose logs -f${white}"

} catch {
    Write-Error "Error durante la configuración: $_"
    Write-Error $_.ScriptStackTrace
    exit 1
}
