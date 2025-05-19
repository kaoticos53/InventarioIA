using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading;
using Inventario.Core;
using Inventario.Api.Filters;
using Inventario.Core.DTOs;
using Inventario.Core.Entities;
using Inventario.Infrastructure;
using Inventario.Infrastructure.Data;
using Inventario.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Inventario.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
var configuration = builder.Configuration;

// Agregar servicios al contenedor
builder.Services.AddInfrastructure(configuration);

// Agregar Health Checks
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se ha configurado la cadena de conexión 'DefaultConnection' en appsettings.json");
}

builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: connectionString,
        healthQuery: "SELECT 1;",
        name: "sqlserver",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "sqlserver" });

// Configuración del servicio de correo electrónico
var emailSettings = builder.Configuration.GetSection("EmailSettings");
builder.Services.Configure<Inventario.Core.Models.EmailSettings>(emailSettings);

// Registrar servicios personalizados
builder.Services.AddScoped<IAuthService, Inventario.Infrastructure.AuthService>();
builder.Services.AddScoped<IEquipoService, EquipoService>();
builder.Services.AddScoped<IUbicacionService, UbicacionService>();
builder.Services.AddScoped<IFichaAveriaService, FichaAveriaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Registrar MailtrapEmailService
builder.Services.AddScoped<IEmailService, MailtrapEmailService>();

// Configurar autenticación JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtConfig = jwtSettings.Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(jwtSettings);

// Validar configuración JWT
var secret = jwtSettings["Secret"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("La configuración de JWT es incorrecta. Asegúrese de que 'Secret', 'Issuer' y 'Audience' estén configurados en appsettings.json");
}

var key = Encoding.ASCII.GetBytes(secret);

// Registrar la configuración JWT para inyección de dependencias
if (jwtConfig != null)
{
    builder.Services.AddSingleton(jwtConfig);
}
else
{
    throw new InvalidOperationException("No se pudo cargar la configuración JWT. Asegúrese de que la sección 'JwtSettings' esté correctamente configurada en appsettings.json");
}

// Imprimir la configuración para depuración
Console.WriteLine($"JWT Config - Issuer: {issuer}, Audience: {audience}");

// Configurar Identity para incluir los roles en el token
builder.Services.Configure<IdentityOptions>(options =>
{
    // Configuración de claims para asegurar que los roles se incluyan en el token
    options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
});

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    
    // Configurar parámetros de validación del token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar la firma del emisor
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        
        // Validar el emisor (issuer)
        ValidateIssuer = true,
        ValidIssuer = issuer,
        
        // Validar el público objetivo (audience)
        ValidateAudience = true,
        ValidAudience = audience,
        
        // Validar el tiempo de vida del token
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        
        // Configurar los tipos de claims
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
    
    // Configurar eventos para depuración
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Error de autenticación: {context.Exception}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validado correctamente");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // Permitir el token en la cadena de consulta para WebSockets
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}, {context.ErrorUri}");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            Console.WriteLine($"Acceso denegado: {context.Request.Path}");
            return Task.CompletedTask;
        }
    };
});

// Configurar políticas de autorización
builder.Services.AddAuthorization(options =>
{
    // Asegurarse de que los nombres de los roles coincidan exactamente con los de la base de datos
    options.AddPolicy("RequireAdministradorRole", policy => 
        policy.RequireRole("Administrador"));
        
    options.AddPolicy("RequireSupervisorRole", policy => 
        policy.RequireRole("Administrador", "Supervisor"));
        
    options.AddPolicy("RequireTecnicoRole", policy => 
        policy.RequireRole("Administrador", "Supervisor", "Tecnico"));
        
    options.AddPolicy("RequireUsuarioRole", policy => 
        policy.RequireRole("Administrador", "Supervisor", "Tecnico", "Usuario"));
});

// Configurar CORS
var corsOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.WithOrigins(corsOrigins)
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// Configurar controladores
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(GlobalExceptionFilter));
    
    // Hacer que la autenticación sea obligatoria por defecto
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddJsonOptions(options =>
{
    // Configuración para manejar referencias circulares
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    
    // Configuración para ignorar propiedades nulas
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    
    // Configuración para manejar caracteres especiales
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    
    // Configuración para usar el formato de nombre de propiedad en mayúsculas y minúsculas
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    
    // Configuración para leer y escribir números como cadenas (opcional)
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;
});

// Configurar la codificación de caracteres para la aplicación
Console.OutputEncoding = System.Text.Encoding.UTF8;
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);



// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Inventario API", 
        Version = "v1",
        Description = "API para el sistema de inventario de equipos audiovisuales",
        Contact = new OpenApiContact
        {
            Name = "Soporte Técnico",
            Email = "soporte@inventario.com"
        }
    });
    
    // Configuración para JWT en Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Ingresa el token JWT en el campo de abajo.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
    
    // Habilitar anotaciones XML para documentación
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
// Habilitar Swagger en todos los entornos para facilitar la depuración
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventario API V1");
});

app.UseHttpsRedirection();
// Configurar archivos estáticos para las plantillas de correo
var emailTemplatesPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates");
if (!Directory.Exists(emailTemplatesPath))
{
    Directory.CreateDirectory(emailTemplatesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(emailTemplatesPath),
    RequestPath = "/emailtemplates"
});

// Configurar CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Mapear Health Checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

// Aplicar migraciones y crear la base de datos al iniciar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<RolPersonalizado>>();
        
        Console.WriteLine("Inicializando la base de datos...");

        // Intentar varias veces conectar a la base de datos
        int maxRetries = 60; // Aumentamos significativamente el número de reintentos
        int retryCount = 0;
        bool success = false;

        while (!success && retryCount < maxRetries)
        {
            try
            {
                retryCount++;
                Console.WriteLine($"Intento {retryCount} de {maxRetries} para inicializar la base de datos...");
                
                // Primero intentamos conectar a la base de datos master para crear la base de datos InventarioDB
                var connectionStringBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(context.Database.GetConnectionString());
                var databaseName = connectionStringBuilder.InitialCatalog;
                var originalConnectionTimeout = connectionStringBuilder.ConnectTimeout;
                
                // Aumentamos el tiempo de conexión para el intento inicial
                connectionStringBuilder.ConnectTimeout = 60;
                connectionStringBuilder.InitialCatalog = "master";
                connectionStringBuilder.Encrypt = false; // Deshabilitamos el cifrado para simplificar la conexión
                var masterConnectionString = connectionStringBuilder.ToString();
                
                Console.WriteLine($"Intentando conectar a la base de datos master para crear {databaseName}...");
                
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString))
                {
                    try
                    {
                        // Usar un timeout para la operación de apertura
                        var connectionTask = connection.OpenAsync();
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                        
                        if (await Task.WhenAny(connectionTask, timeoutTask) == timeoutTask)
                        {
                            throw new TimeoutException("La conexión a la base de datos tardó demasiado tiempo.");
                        }
                        
                        Console.WriteLine("Conexión a master establecida correctamente.");
                        
                        // Verificar si la base de datos ya existe
                        var checkDbCommand = new Microsoft.Data.SqlClient.SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}';", connection);
                        checkDbCommand.CommandTimeout = 30; // 30 segundos de timeout
                        var result = await checkDbCommand.ExecuteScalarAsync();
                        var dbExists = result != null && result != DBNull.Value && Convert.ToInt32(result) > 0;
                        
                        if (!dbExists)
                        {
                            Console.WriteLine($"La base de datos {databaseName} no existe. Creándola...");
                            var createDbCommand = new Microsoft.Data.SqlClient.SqlCommand($"CREATE DATABASE [{databaseName}];", connection);
                            createDbCommand.CommandTimeout = 60; // 60 segundos de timeout para la creación
                            await createDbCommand.ExecuteNonQueryAsync();
                            Console.WriteLine($"Base de datos {databaseName} creada correctamente.");
                        }
                        else
                        {
                            Console.WriteLine($"La base de datos {databaseName} ya existe.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al conectar a master o crear la base de datos: {ex.Message}");
                        throw; // Relanzamos la excepción para que entre en el bloque catch externo
                    }
                    finally
                    {
                        // Asegurarnos de cerrar la conexión en cualquier caso
                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
                
                // Ahora que la base de datos existe, podemos crear las tablas y esquemas
                Console.WriteLine("Asegurando que todas las tablas y esquemas existen...");
                
                // Restauramos el nombre de la base de datos y el timeout original
                connectionStringBuilder.InitialCatalog = databaseName;
                connectionStringBuilder.ConnectTimeout = originalConnectionTimeout;
                
                // Configuramos el contexto para usar la nueva cadena de conexión
                context.Database.SetConnectionString(connectionStringBuilder.ToString());
                
                // Usamos Migrate en lugar de EnsureCreated para aplicar todas las migraciones
                try
                {
                    await context.Database.MigrateAsync();
                    Console.WriteLine("Migraciones aplicadas correctamente.");
                }
                catch (Exception migrationEx)
                {
                    Console.WriteLine($"Error al aplicar migraciones: {migrationEx.Message}. Intentando EnsureCreated como alternativa.");
                    // Si las migraciones fallan, intentamos EnsureCreated como respaldo
                    await context.Database.EnsureCreatedAsync();
                }
                
                Console.WriteLine("Base de datos inicializada correctamente.");
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar la base de datos (intento {retryCount}): {ex.Message}");
                
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("Se alcanzó el número máximo de intentos. No se pudo inicializar la base de datos.");
                    // No lanzamos la excepción para permitir que la aplicación continúe
                    // La aplicación mostrará errores cuando intente acceder a la base de datos
                    // pero al menos no se reiniciará constantemente
                    break;
                }
                else
                {
                    var waitTime = Math.Min(5000 * retryCount, 30000); // Tiempo de espera progresivo, máximo 30 segundos
                    Console.WriteLine($"Esperando {waitTime/1000} segundos antes de reintentar...");
                    await Task.Delay(waitTime);
                }
            }
        }
        
        // Forzar la creación de todas las tablas necesarias
        try
        {
            Console.WriteLine("Intentando crear todas las tablas necesarias...");
            
            // Forzar la creación de la base de datos y sus tablas
            bool dbCreated = await context.Database.EnsureCreatedAsync();
            
            if (dbCreated)
            {
                Console.WriteLine("Base de datos y tablas creadas correctamente.");
            }
            else
            {
                Console.WriteLine("La base de datos ya existía. Verificando si las tablas existen...");
                
                // Intentar ejecutar una consulta simple para verificar si las tablas existen
                try
                {
                    var roleCount = await context.Roles.CountAsync();
                    Console.WriteLine($"Verificación exitosa. Hay {roleCount} roles en la base de datos.");
                }
                catch (Exception queryEx)
                {
                    Console.WriteLine($"Error al verificar las tablas: {queryEx.Message}");
                    Console.WriteLine("Intentando recrear las tablas...");
                    
                    // Si hay un error, intentar eliminar y recrear la base de datos
                    try
                    {
                        await context.Database.EnsureDeletedAsync();
                        await context.Database.EnsureCreatedAsync();
                        Console.WriteLine("Base de datos recreada correctamente.");
                    }
                    catch (Exception recreateEx)
                    {
                        Console.WriteLine($"Error al recrear la base de datos: {recreateEx.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error general al inicializar la base de datos: {ex.Message}");
            // Continuar con la ejecución, pero la aplicación podría fallar al intentar acceder a las tablas
        }
        
        // Crear roles si no existen
        try
        {
            string[] roles = { "Administrador", "Supervisor", "Tecnico", "Usuario" };
            foreach (var role in roles)
            {
                try
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new RolPersonalizado(role));
                        Console.WriteLine($"Rol '{role}' creado correctamente.");
                    }
                    else
                    {
                        Console.WriteLine($"El rol '{role}' ya existe.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear el rol '{role}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear los roles: {ex.Message}");
            // Continuar con la ejecución, pero la aplicación podría fallar al intentar acceder a los roles
        }
        
        // Crear usuario administrador si no existe
        var adminEmail = "admin@inventario.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nombre = "Administrador",
                Apellido = "Sistema",
                EmailConfirmed = true,
                Activo = true
            };
            
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrador");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos y los roles.");
        
        // En desarrollo, lanzar la excepción para ver el error completo
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();
