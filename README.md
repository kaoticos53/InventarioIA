# Sistema de Inventario

Sistema de gestión de inventario desarrollado con .NET 9.0, Blazor WebAssembly y SQL Server en contenedores Docker.

## Requisitos previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js](https://nodejs.org/) (recomendado para desarrollo)
- [Git](https://git-scm.com/)

## Configuración inicial

1. Clona el repositorio:
   ```bash
   git clone <url-del-repositorio>
   cd Inventario
   ```

2. Ejecuta el script de configuración:
   ```powershell
   .\init.ps1
   ```

   Este script verificará las dependencias necesarias y configurará las variables de entorno.

## Despliegue

### Despliegue local con Docker

Para desplegar la aplicación en un entorno local utilizando Docker:

1. Asegúrate de que Docker Desktop esté en ejecución y configurado correctamente.

2. Abre una terminal PowerShell y navega hasta la carpeta raíz del proyecto:
   ```powershell
   cd ruta\a\Inventario
   ```

3. Ejecuta el script de despliegue:
   ```powershell
   .\scripts\deploy.ps1
   ```

   Este script realizará las siguientes acciones:
   - Construirá las imágenes Docker para la API y la aplicación web
   - Creará y configurará la red Docker necesaria
   - Iniciará el contenedor de SQL Server con la configuración adecuada
   - Ejecutará las migraciones de base de datos
   - Iniciará los contenedores de la API y la aplicación web
   - Verificará que todos los servicios estén funcionando correctamente

4. Espera a que el script confirme que todos los servicios están en funcionamiento.

### Despliegue manual (sin script)

Si prefieres realizar el despliegue manualmente, sigue estos pasos:

1. Construye las imágenes Docker:
   ```bash
   docker-compose build
   ```

2. Inicia los servicios:
   ```bash
   docker-compose up -d
   ```

3. Verifica que los contenedores estén funcionando:
   ```bash
   docker-compose ps
   ```

### Despliegue en entorno de producción

Para desplegar en un entorno de producción, sigue estos pasos adicionales:

1. Configura las variables de entorno para producción en un archivo `.env.production`.

2. Ejecuta el script de despliegue de producción:
   ```powershell
   .\scripts\deploy-prod.ps1
   ```

   O manualmente con Docker Compose:
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

3. Configura un servidor proxy inverso (como Nginx o Traefik) para gestionar el tráfico HTTPS.

### Solución de problemas comunes

- **Error de conexión a la base de datos**: Verifica que el contenedor de SQL Server esté funcionando y que la cadena de conexión sea correcta.
  ```bash
  docker logs inventario-db
  ```

- **La API no responde**: Comprueba los logs del contenedor de la API.
  ```bash
  docker logs inventario-api
  ```

- **Problemas con la aplicación web**: Revisa los logs del contenedor web.
  ```bash
  docker logs inventario-web
  ```

## Acceso a la aplicación

- **Aplicación web**: http://localhost:8080
- **API**: http://localhost:5000
- **API Health Check**: http://localhost:5000/health
- **Base de datos**: localhost,1433
  - Usuario: sa
  - Contraseña: (la configurada en el archivo .env)

## Estructura del proyecto

- **Inventario.Api**: API RESTful desarrollada con ASP.NET Core
- **Inventario.Web**: Aplicación web desarrollada con Blazor WebAssembly
- **Inventario.Core**: Lógica de negocio y entidades del dominio
- **Inventario.Infrastructure**: Implementación de la infraestructura (repositorios, servicios, etc.)
- **Inventario.Tests**: Pruebas unitarias y de integración

## Variables de entorno

El proyecto utiliza un archivo `.env` para configurar las variables de entorno. Se crea automáticamente al ejecutar `init.ps1`.

## Desarrollo

Para desarrollo local sin Docker:

1. Configura la base de datos SQL Server:
   - Asegúrate de tener SQL Server instalado o usa el contenedor Docker
   - Actualiza la cadena de conexión en `appsettings.Development.json`

2. Ejecuta la API:
   ```bash
   cd Inventario.Api
   dotnet run
   ```

3. Ejecuta la aplicación web:
   ```bash
   cd Inventario.Web
   dotnet run
   ```

## Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.

## Soporte

Para soporte técnico, contacta al equipo de desarrollo en soporte@inventario.com.
