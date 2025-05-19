# Instrucciones para Continuar en Ordenador Nuevo

## 1. Configuración Inicial

1. **Clonar el repositorio**:
   ```bash
   git clone [URL_DEL_REPOSITORIO]
   cd Inventario/temp_refactor
   ```

2. **Instalar dependencias**:
   - .NET 9.0 SDK
   - SQL Server 2022 o Docker Desktop
   - Git
   - Visual Studio 2022 o VS Code

3. **Configurar base de datos con Docker**:
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
   ```

4. **Configurar la aplicación**:
   ```bash
   copy src\Inventario.API\appsettings.Development.json.example src\Inventario.API\appsettings.Development.json
   # Editar el archivo con tus configuraciones
   ```

## 2. Iniciar el Proyecto

```bash
dotnet restore
cd src/Inventario.API
dotnet ef database update
dotnet run
```

## 3. Para Continuar con la IA de Windsurf

Cuando abras la conversación con la IA, copia y pega este mensaje:

```
Hola, estoy trabajando en el proyecto de Inventario. Aquí está el contexto:

- Estamos refactorizando a una arquitectura hexagonal
- El código está en un directorio temporal: temp_refactor
- Hemos creado scripts de migración a producción
- El README.md contiene la documentación actualizada
- El archivo TASKS.md tiene la lista de tareas pendientes

Las tareas pendientes principales son:
1. Implementar controladores (Auth, Usuarios, Ubicaciones, Equipos, FichasAverias)
2. Configurar Swagger/OpenAPI
3. Crear migraciones de base de datos
4. Implementar pruebas unitarias

¿Por dónde deberíamos continuar?
```

## 4. Comandos Útiles

```bash
# Limpiar la solución
.\clean.ps1

# Crear migración
dotnet ef migrations add NombreMigracion --project src/Inventario.Infrastructure --startup-project src/Inventario.API

# Aplicar migraciones
dotnet ef database update --startup-project src/Inventario.API

# Ejecutar pruebas
dotnet test
```

## 5. Estructura del Proyecto

```
src/
  Inventario.API/         # Web API
  Inventario.Core/         # Lógica de negocio
    Application/           # Casos de uso, DTOs
    Domain/                # Entidades
  Inventario.Infrastructure/
    Persistence/          # Base de datos
    Services/             # Servicios externos
tests/                    # Pruebas
```

## 6. Siguientes Pasos Recomendados

1. Revisar el estado actual del proyecto:
   ```
   ¿Puedes mostrarme un resumen del estado actual del proyecto?
   ```

2. Para implementar un controlador:
   ```
   Necesito ayuda para implementar el [Nombre]Controller siguiendo la arquitectura hexagonal.
   ```

3. Para configurar Swagger:
   ```
   ¿Puedes ayudarme a configurar Swagger para documentar la API?
   ```

4. Para crear migraciones:
   ```
   Necesito crear una migración para [entidad]. ¿Qué comandos debo usar?
   ```

## 7. Enlaces Importantes

- [Guía de Migración](./MIGRATION_GUIDE.md)
- [README Principal](./README.md)
- [Script de Migración](./Migrate-ToProduction.ps1)

## 8. Notas Adicionales

- La base de datos usa SQL Server con autenticación SQL
- La autenticación usa JWT (configurada en appsettings.json)
- Hay un archivo .gitignore configurado para excluir archivos innecesarios
- Usamos el patrón Repository y Unit of Work en la capa de Infraestructura
