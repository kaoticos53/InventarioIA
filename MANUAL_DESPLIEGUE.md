# Manual de Despliegue - Sistema de Inventario

## Tabla de Contenidos
1. [Requisitos Previos](#requisitos-previos)
2. [Configuración del Entorno](#configuración-del-entorno)
3. [Despliegue con Docker](#despliegue-con-docker)
4. [Configuración de la Base de Datos](#configuración-de-la-base-de-datos)
5. [Variables de Entorno](#variables-de-entorno)
6. [Mantenimiento](#mantenimiento)
7. [Solución de Problemas](#solución-de-problemas)

## Requisitos Previos

- Docker Engine 20.10.0 o superior
- Docker Compose 1.29.0 o superior
- Git (opcional, solo para clonar el repositorio)
- 4 GB de RAM mínimo (8 GB recomendado)
- 20 GB de espacio en disco

## Configuración del Entorno

### 1. Clonar el Repositorio (opcional)

```bash
git clone <url-del-repositorio>
cd Inventario
```

### 2. Configurar Variables de Entorno

Crea un archivo `.env` en la raíz del proyecto con las siguientes variables:

```env
# Configuración de la base de datos
DB_SA_PASSWORD=YourStrong@Passw0rd
DB_NAME=InventarioDB

# Configuración de la API
JWT_SECRET=YourJwtSecretKey12345678901234567890
API_PORT=5000
WEB_PORT=8080

# Configuración de correo (ejemplo para Mailtrap)
SMTP_SERVER=smtp.mailtrap.io
SMTP_PORT=2525
SMTP_USERNAME=your_mailtrap_username
SMTP_PASSWORD=your_mailtrap_password
SMTP_FROM_EMAIL=no-reply@inventario.com
SMTP_FROM_NAME=Sistema de Inventario
```

## Despliegue con Docker

### 1. Construir y Ejecutar los Contenedores

```bash
# Ejecutar en modo producción
docker-compose up -d --build

# Verificar el estado de los contenedores
docker-compose ps

# Ver logs en tiempo real
docker-compose logs -f
```

### 2. Acceder a la Aplicación

- **Aplicación Web**: http://localhost:8080
- **API**: http://localhost:5000
- **Base de Datos**:
  - Servidor: localhost,1433
  - Usuario: sa
  - Contraseña: [La configurada en DB_SA_PASSWORD]

## Configuración de la Base de Datos

### 1. Aplicar Migraciones

```bash
# Ejecutar migraciones en el contenedor de la API
docker-compose exec api dotnet ef database update
```

### 2. Crear Usuario Administrador

1. Accede a la interfaz web
2. Regístrate con el primer usuario (será automáticamente administrador)
3. O ejecuta el siguiente comando para crear un administrador:

```bash
docker-compose exec api dotnet run user-create-admin
```

## Variables de Entorno

### API

| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| ASPNETCORE_ENVIRONMENT | Entorno de ejecución | Production |
| ConnectionStrings__DefaultConnection | Cadena de conexión a SQL Server | - |
| JwtSettings__Secret | Clave secreta para JWT | - |
| JwtSettings__TokenLifetimeMinutes | Tiempo de vida del token | 1440 |
| JwtSettings__RefreshTokenLifetimeDays | Tiempo de vida del refresh token | 7 |
| CORS__AllowedOrigins | Orígenes permitidos para CORS | * |

### Aplicación Web

| Variable | Descripción | Valor por Defecto |
|----------|-------------|-------------------|
| ApiUrl | URL de la API | http://api |
| NODE_ENV | Entorno de ejecución | production |

## Mantenimiento

### Actualizar la Aplicación

```bash
# Detener los contenedores
docker-compose down

# Actualizar el código
git pull

# Reconstruir y ejecutar
docker-compose up -d --build
```

### Respaldar la Base de Datos

```bash
# Crear un respaldo
docker-compose exec db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "$DB_SA_PASSWORD" \
    -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'/var/opt/mssql/backups/$DB_NAME.bak' WITH NOFORMAT, NOINIT, NAME = '$DB_NAME-full', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Copiar el respaldo al host
docker cp inventario-db-1:/var/opt/mssql/backups/$DB_NAME.bak .
```

## Solución de Problemas

### Error con nginx.conf

Si al ejecutar `docker-compose up -d` aparece el siguiente error:

```
ERROR [web final 4/4] COPY --from=publish /app/publish/nginx.conf /etc/nginx/nginx.conf
failed to solve: failed to compute cache key: failed to calculate checksum of ref [...]: "/app/publish/nginx.conf": not found
```

Este error indica que el archivo `nginx.conf` no se encuentra en la ruta esperada durante la construcción de la imagen Docker. Para solucionarlo:

1. Verifica que el archivo `nginx.conf` existe en el directorio `Inventario.Web/`.

2. Modifica el Dockerfile de la aplicación web (`Inventario.Web/Dockerfile`) cambiando la línea:

   ```dockerfile
   COPY --from=publish /app/publish/nginx.conf /etc/nginx/nginx.conf
   ```

   por:

   ```dockerfile
   COPY Inventario.Web/nginx.conf /etc/nginx/nginx.conf
   ```

3. Reconstruye las imágenes:

   ```bash
   docker-compose build web
   docker-compose up -d
   ```

### Error con RolPersonalizado

Si al iniciar la aplicación aparece un error en la página web indicando "¡Ups! Algo salió mal" y en los logs de la API se muestra un error como:

```
Cannot create a DbSet for 'IdentityRole' because this type is not included in the model for the context.
```

O un error similar a:

```
'RolPersonalizado' does not contain a constructor that takes 1 arguments
```

O un error relacionado con la tabla `AspNetRoles`:

```
Invalid object name 'AspNetRoles'.
```

Estos errores se deben a problemas con la configuración de Identity en la aplicación. Para solucionarlos:

1. Verifica que la clase `RolPersonalizado` tiene un constructor que acepta un parámetro de nombre. Abre el archivo `Inventario.Core/Entities/RolPersonalizado.cs` y asegúrate de que contiene el siguiente código:

   ```csharp
   public class RolPersonalizado : IdentityRole
   {
       public RolPersonalizado() : base()
       {
       }

       public RolPersonalizado(string roleName) : base(roleName)
       {
       }

       public string? Descripcion { get; set; }
       public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
   }
   ```

2. Asegúrate de que en el archivo `Program.cs` se está utilizando `RolPersonalizado` en lugar de `IdentityRole` en todos los lugares:

   ```csharp
   // En la configuración de Identity
   builder.Services.AddIdentity<ApplicationUser, RolPersonalizado>(options => {...
   
   // Al obtener el RoleManager
   var roleManager = services.GetRequiredService<RoleManager<RolPersonalizado>>();
   
   // Al crear roles
   await roleManager.CreateAsync(new RolPersonalizado(role));
   ```

3. También en el archivo `UsuarioService.cs`:

   ```csharp
   private readonly RoleManager<RolPersonalizado> _roleManager;
   
   // Y en el constructor
   public UsuarioService(
       UserManager<ApplicationUser> userManager,
       RoleManager<RolPersonalizado> roleManager,
       ...
   ```

4. Modifica la inicialización de la base de datos en `Program.cs` para asegurarte de que se crea correctamente:

   ```csharp
   // Asegurarse de que la base de datos existe y aplicar migraciones
   context.Database.EnsureCreated();
   // Intentar aplicar migraciones si es necesario
   try
   {
       context.Database.Migrate();
   }
   catch (Exception ex)
   {
       Console.WriteLine($"Error al aplicar migraciones: {ex.Message}");
       // Continuar de todos modos, ya que EnsureCreated debería haber creado la base de datos
   }
   ```

5. Si sigues teniendo problemas con la tabla `AspNetRoles`, configura explícitamente el nombre de la tabla en el método `OnModelCreating` del contexto de la base de datos (`ApplicationDbContext.cs`):

   ```csharp
   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       base.OnModelCreating(modelBuilder);
       
       // Configurar nombres de tablas de Identity
       modelBuilder.Entity<RolPersonalizado>().ToTable("AspNetRoles");
       
       // Resto de la configuración...
   }
   ```

5. Reconstruye la imagen de la API, elimina los contenedores existentes y vuelve a iniciarlos:

   ```bash
   docker-compose down
   docker-compose build api
   docker-compose up -d
   ```

### Error con SQL Server

Si al ejecutar `docker-compose up -d` aparece el siguiente error:

```
dependency failed to start: container inventario-db-1 is unhealthy
```

Este error indica que el contenedor de SQL Server no ha podido iniciar correctamente. Para solucionarlo:

1. Simplifica la configuración de SQL Server en el archivo `docker-compose.yml`:

   ```yaml
   # Base de datos SQL Server
   db:
     image: mcr.microsoft.com/mssql/server:2019-latest
     environment:
       - ACCEPT_EULA=Y
       - SA_PASSWORD=YourStrong@Passw0rd
       - MSSQL_PID=Express
     ports:
       - "1433:1433"
     volumes:
       - sql_data:/var/opt/mssql
     networks:
       - inventario-network
   ```

2. Elimina la verificación de salud (healthcheck) y las dependencias condicionales:

   ```yaml
   depends_on:
     - db  # En lugar de db: condition: service_healthy
   ```

3. Elimina la versión obsoleta del archivo `docker-compose.yml`:

   ```yaml
   # Eliminar esta línea
   version: '3.8'
   ```

4. Limpia los contenedores y volúmenes existentes:

   ```bash
   docker-compose down -v
   docker volume prune -f
   ```

5. Reconstruye e inicia los contenedores:

   ```bash
   docker-compose up -d
   ```

### Verificar Logs

```bash
# Ver logs de todos los servicios
docker-compose logs

# Ver logs de un servicio específico
docker-compose logs api
```

### Problemas Comunes

1. **La aplicación no se inicia**
   - Verifica que los puertos no estén en uso
   - Revisa los logs con `docker-compose logs`

2. **Error de conexión a la base de datos**
   - Verifica que el contenedor de SQL Server esté en ejecución
   - Comprueba las credenciales en el archivo .env

3. **Problemas con migraciones**
   - Asegúrate de que la base de datos existe y es accesible
   - Verifica que las credenciales sean correctas

### Soporte

Para soporte técnico, contacta al equipo de desarrollo en soporte@inventario.com

---

**Última actualización**: 16 de Mayo de 2025
