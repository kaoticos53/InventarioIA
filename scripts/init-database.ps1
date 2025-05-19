# Script para inicializar la base de datos SQL Server para Inventario
# Este script crea la base de datos InventarioDB si no existe

$ErrorActionPreference = "Stop"

Write-Host "Iniciando la configuración de la base de datos..." -ForegroundColor Green

# Parámetros de conexión
$serverName = "db"
$userName = "sa"
$securePassword = ConvertTo-SecureString "YourStrong@Passw0rd" -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($userName, $securePassword)
$databaseName = "InventarioDB"

# Función para verificar si la base de datos existe
function Test-DatabaseExists {
    param (
        [string]$serverName,
        [string]$userName,
        [System.Management.Automation.PSCredential]$credential,
        [string]$databaseName
    )

    try {
        $connectionString = "Server=$serverName;User ID=$userName;Password=$password;Trusted_Connection=False;Encrypt=False"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()

        $query = "SELECT COUNT(*) FROM sys.databases WHERE name = '$databaseName'"
        $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
        $result = $command.ExecuteScalar()

        $connection.Close()
        return $result -gt 0
    }
    catch {
        Write-Host "Error al verificar si la base de datos existe: $_" -ForegroundColor Red
        return $false
    }
}

# Función para crear la base de datos
function New-Database {
    param (
        [string]$serverName,
        [string]$userName,
        [System.Management.Automation.PSCredential]$credential,
        [string]$databaseName
    )

    try {
        $connectionString = "Server=$serverName;User ID=$userName;Password=$($credential.GetNetworkCredential().Password);Trusted_Connection=False;Encrypt=False"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()

        $query = "CREATE DATABASE [$databaseName]"
        $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
        $command.ExecuteNonQuery()

        $connection.Close()
        Write-Host "Base de datos $databaseName creada correctamente." -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Error al crear la base de datos: $_" -ForegroundColor Red
        return $false
    }
}

# Verificar si la base de datos existe
Write-Host "Verificando si la base de datos $databaseName existe..." -ForegroundColor Yellow
$maxRetries = 10
$retryCount = 0
$success = $false

while (-not $success -and $retryCount -lt $maxRetries) {
    $retryCount++
    Write-Host "Intento $retryCount de $maxRetries..." -ForegroundColor Yellow
    
    try {
        # Verificar si la base de datos existe
        $dbExists = Test-DatabaseExists -serverName $serverName -userName $userName -credential $credential -databaseName $databaseName
        
        if ($dbExists) {
            Write-Host "La base de datos $databaseName ya existe." -ForegroundColor Green
            $success = $true
        }
        else {
            Write-Host "La base de datos $databaseName no existe. Creándola..." -ForegroundColor Yellow
            $success = New-Database -serverName $serverName -userName $userName -credential $credential -databaseName $databaseName
        }
    }
    catch {
        Write-Host "Error en el intento ${retryCount}: $($_.Exception.Message)" -ForegroundColor Red
        if ($retryCount -lt $maxRetries) {
            Write-Host "Esperando 5 segundos antes de reintentar..." -ForegroundColor Yellow
            Start-Sleep -Seconds 5
        }
    }
}

if (-not $success) {
    Write-Host "No se pudo crear la base de datos después de $maxRetries intentos." -ForegroundColor Red
    exit 1
}

Write-Host "Configuración de la base de datos completada correctamente." -ForegroundColor Green
exit 0
