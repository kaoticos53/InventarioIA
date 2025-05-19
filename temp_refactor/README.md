# Sistema de Inventario - Documentación del Proyecto

Este es un sistema de gestión de inventario desarrollado con .NET 9.0, siguiendo la arquitectura hexagonal y los principios SOLID.

> **Importante**: Este es un directorio temporal de refactorización. Para migrar estos cambios al directorio principal, sigue las instrucciones en la sección [Migración a Producción](#migración-a-producción).

## 🚀 Comenzando

### Configuración en un Ordenador Nuevo

Para comenzar a trabajar en este proyecto en un ordenador nuevo, sigue estos pasos:

1. **Clonar el repositorio**
   ```bash
   git clone [URL_DEL_REPOSITORIO]
   cd Inventario/temp_refactor  # Importante: entrar al directorio de refactorización
   ```

2. **Configurar el entorno de desarrollo**
   - Instalar [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Instalar [SQL Server 2022](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) o [Docker Desktop](https://www.docker.com/products/docker-desktop)
   - Instalar [Git](https://git-scm.com/)
   - Instalar [Visual Studio 2022](https://visualstudio.microsoft.com/es/vs/) o [VS Code](https://code.visualstudio.com/)

3. **Configurar variables de entorno**
   ```bash
   # Copiar el archivo de configuración de ejemplo
   copy src\Inventario.API\appsettings.Development.json.example src\Inventario.API\appsettings.Development.json
   ```
   - Editar el archivo `appsettings.Development.json` con tus configuraciones locales

4. **Iniciar la base de datos**
   - Usando Docker (recomendado):
     ```bash
     docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
     ```
   - O instalar SQL Server 2022 localmente

5. **Restaurar paquetes y migraciones**
   ```bash
   dotnet restore
   cd src/Inventario.API
   dotnet ef database update
   ```

6. **Ejecutar la aplicación**
   ```bash
   dotnet run --project src/Inventario.API
   ```
   La aplicación estará disponible en `https://localhost:5001`

## 📋 Requisitos Previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2022](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) o [Docker](https://www.docker.com/products/docker-desktop) para ejecutar SQL Server en contenedor
- [Node.js](https://nodejs.org/) (para el frontend, si se implementa más adelante)
- [Git](https://git-scm.com/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/es/vs/) o [Visual Studio Code](https://code.visualstudio.com/)

## 🏗️ Estructura del Proyecto

```
Inventario/
├── src/
│   ├── Inventario.API/           # Capa de presentación (Web API)
│   ├── Inventario.Core/           # Dominio y lógica de negocio
│   │   ├── Application/           # Casos de uso, DTOs, interfaces
│   │   └── Domain/                # Entidades y reglas de negocio
│   └── Inventario.Infrastructure/ # Implementaciones de infraestructura
│       ├── Persistence/           # Contexto de base de datos
│       └── Services/              # Servicios externos
├── tests/                         # Pruebas unitarias y de integración
└── README.md                     # Este archivo
```

## 🛠️ Configuración del Entorno

1. **Clonar el repositorio**
   ```bash
   git clone [URL_DEL_REPOSITORIO]
   cd Inventario
   ```

2. **Configurar la base de datos**
   - Opción 1: Usando Docker (recomendado para desarrollo)
     ```bash
     docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
        -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
     ```
   - Opción 2: Instalar SQL Server 2022 localmente

3. **Configurar la cadena de conexión**
   - Copiar el archivo `appsettings.Development.json.example` a `appsettings.Development.json`
   - Ajustar la cadena de conexión según tu configuración

4. **Aplicar migraciones**
   ```bash
   cd src/Inventario.API
   dotnet ef database update --startup-project ../Inventario.API/Inventario.API.csproj
   ```

5. **Ejecutar la aplicación**
   ```bash
   dotnet run --project src/Inventario.API/Inventario.API.csproj
   ```



## ⚙️ Configuración de Variables de Entorno

Crear un archivo `appsettings.Development.json` en `src/Inventario.API/` con el siguiente contenido:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=InventarioDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "clave_secreta_muy_larga_para_jwt_token",
    "Issuer": "InventarioAPI",
    "Audience": "InventarioClient",
    "ExpireMinutes": 1440,
    "RefreshTokenExpireDays": 7
  },
  "SendGrid": {
    "ApiKey": "tu_api_key_de_sendgrid",
    "FromEmail": "no-reply@inventario.com",
    "FromName": "Sistema de Inventario"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## 🔄 Migración a Producción

Para migrar los cambios del directorio temporal al directorio principal de producción:

1. **Revisa los cambios pendientes**
   ```bash
   git status
   git diff
   ```

2. **Ejecuta el script de migración** (desde el directorio raíz del proyecto):
   ```powershell
   .\Migrate-ToProduction.ps1 -TargetDirectory "..\Inventario" -BackupDirectory "..\Backups"
   ```

3. **Sigue las instrucciones** en pantalla para completar la migración.

> ℹ️ Para más detalles, consulta la [Guía de Migración](MIGRATION_GUIDE.md).

## 📋 Tareas Pendientes

### 1. Implementación de Controladores
- [ ] Controlador de Autenticación (`AuthController`)
- [ ] Controlador de Usuarios (`UsuariosController`)
- [ ] Controlador de Ubicaciones (`UbicacionesController`)
- [ ] Controlador de Equipos (`EquiposController`)
- [ ] Controlador de Fichas de Avería (`FichasAveriasController`)

### 2. Documentación de la API
- [ ] Configurar Swagger/OpenAPI
- [ ] Documentar endpoints con ejemplos
- [ ] Configurar versionado de API

### 3. Pruebas
- [ ] Pruebas unitarias para servicios de dominio
- [ ] Pruebas de integración para controladores
- [ ] Pruebas E2E para flujos completos

### 4. Seguridad
- [ ] Implementar rate limiting
- [ ] Configurar CORS
- [ ] Configurar políticas de autorización detalladas

### 5. Despliegue
- [ ] Configurar Docker Compose
- [ ] Configurar CI/CD
- [ ] Configurar monitoreo y logging

## 🚀 Comandos Útiles

```bash
# Crear una nueva migración
dotnet ef migrations add NombreDeLaMigracion --project src/Inventario.Infrastructure --startup-project src/Inventario.API

# Aplicar migraciones
dotnet ef database update --startup-project src/Inventario.API

# Ejecutar pruebas
dotnet test

# Ejecutar con perfil de desarrollo
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Inventario.API

# Limpiar y reconstruir
./clean.ps1  # Ejecutar en PowerShell
```

## ✨ Guía de Estilo

- Usar nombres descriptivos para variables y métodos
- Seguir las convenciones de C#
- Documentar métodos públicos y clases
- Escribir pruebas unitarias para nueva funcionalidad
- Usar commits atómicos con mensajes descriptivos

## 🛠️ Solución de Problemas

Si encuentras problemas al configurar el proyecto:

1. Verifica que todas las dependencias estén instaladas correctamente
2. Asegúrate de que SQL Server esté en ejecución
3. Revisa los logs de la aplicación para mensajes de error detallados
4. Si usas Docker, verifica que los contenedores estén en ejecución

## 🤝 Contribución

1. Haz fork del repositorio
2. Crea una rama para tu característica (`git checkout -b feature/nueva-funcionalidad`)
3. Haz commit de tus cambios (`git commit -m 'Añadir nueva funcionalidad'`)
4. Haz push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.
