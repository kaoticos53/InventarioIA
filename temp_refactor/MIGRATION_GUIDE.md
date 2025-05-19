# Guía de Migración a Producción

Este documento proporciona instrucciones detalladas para migrar la aplicación desde el directorio temporal de migración al directorio de producción.

## Requisitos Previos

- Windows 10/11 o Windows Server 2016+
- PowerShell 5.1 o superior
- Permisos de administrador (recomendado para copias de seguridad)

## Instrucciones de Uso del Script de Migración

### Parámetros del Script

El script `Migrate-ToProduction.ps1` acepta los siguientes parámetros:

| Parámetro         | Obligatorio | Descripción |
|-------------------|-------------|-------------|
| `-TargetDirectory` | Sí | Ruta al directorio de producción donde se copiarán los archivos |
| `-BackupDirectory` | Sí | Directorio donde se guardarán las copias de seguridad |
| `-Force` | No | Si se especifica, no solicita confirmación antes de sobrescribir |

### Ejemplo de Uso

```powershell
# Ejecutar con confirmación interactiva
.\Migrate-ToProduction.ps1 -TargetDirectory "C:\Proyectos\Inventario" -BackupDirectory "C:\Backups"

# Ejecutar sin confirmación (usar con precaución)
.\Migrate-ToProduction.ps1 -TargetDirectory "C:\Proyectos\Inventario" -BackupDirectory "C:\Backups" -Force
```

## Proceso de Migración Paso a Paso

### 1. Preparación

1. Asegúrate de que todos los cambios en el directorio temporal estén confirmados en el control de versiones.
2. Cierra cualquier instancia de Visual Studio o VS Code que esté abierta en los directorios de origen o destino.
3. Verifica que tengas espacio suficiente en disco para la copia de seguridad.

### 2. Ejecución del Script

1. Abre una consola de PowerShell como administrador.
2. Navega al directorio del proyecto temporal:
   ```powershell
   cd C:\Ruta\Al\Directorio\Temporal\Inventario\temp_refactor
   ```
3. Ejecuta el script de migración:
   ```powershell
   .\Migrate-ToProduction.ps1 -TargetDirectory "C:\Ruta\Al\Directorio\Principal\Inventario" -BackupDirectory "C:\Ruta\Para\Backups"
   ```
4. Sigue las indicaciones en pantalla.

### 3. Verificación

Después de la migración, verifica que:

1. La aplicación se compila correctamente:
   ```powershell
   cd "C:\Ruta\Al\Directorio\Principal\Inventario\src\Inventario.API"
   dotnet build
   ```
2. Las migraciones de base de datos están actualizadas:
   ```powershell
   dotnet ef database update
   ```
3. La aplicación se inicia correctamente:
   ```powershell
   dotnet run
   ```

## Directorios y Archivos Excluidos

El script excluye automáticamente los siguientes directorios y archivos:

- Directorios: `bin`, `obj`, `.vs`, `.vscode`, `.git`, `.idea`, `node_modules`, `TestResults`, `coverage`, `Migrations`
- Archivos: `*.user`, `*.suo`, `*.bak`, `*.tmp`, `*.log`, etc.

## Resolución de Problemas

### Error: "Acceso denegado"

Si recibes un error de acceso denegado:
1. Cierra todas las aplicaciones que puedan estar usando los archivos.
2. Ejecuta PowerShell como administrador.
3. Verifica los permisos en los directorios de origen y destino.

### Error: "No se pudo crear la copia de seguridad"

Si el script no puede crear una copia de seguridad:
1. Verifica que el directorio de respaldo exista y tengas permisos de escritura.
2. Asegúrate de que haya suficiente espacio en disco.
3. Intenta especificar una ruta de respaldo diferente.

### La aplicación no se inicia después de la migración

1. Verifica que todos los paquetes NuGet estén restaurados:
   ```powershell
   dotnet restore
   ```
2. Revisa los logs de la aplicación para ver errores específicos.
3. Verifica que las cadenas de conexión en `appsettings.json` sean correctas.

## Revertir la Migración

Si necesitas volver a la versión anterior:

1. Localiza la copia de seguridad en el directorio especificado con `-BackupDirectory`.
2. Copia manualmente los archivos de vuelta al directorio de producción.

## Consideraciones Adicionales

- **Variables de entorno**: Asegúrate de configurar las variables de entorno necesarias en el entorno de producción.
- **Secretos de aplicación**: No olvides configurar los secretos de la aplicación en el entorno de producción.
- **Configuración de IIS**: Si usas IIS, asegúrate de que el pool de aplicaciones tenga los permisos adecuados.
- **Tareas programadas**: Verifica que las tareas programadas estén configuradas correctamente.

## Soporte

Si encuentras problemas durante la migración, por favor:
1. Revisa los mensajes de error detallados en la consola.
2. Verifica los logs de la aplicación.
3. Si el problema persiste, contacta al equipo de desarrollo proporcionando:
   - El comando exacto que ejecutaste
   - Los mensajes de error completos
   - La versión de PowerShell (`$PSVersionTable`)
   - El sistema operativo y versión
