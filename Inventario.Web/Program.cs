using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Inventario.Web;
using Inventario.Web.Services.Implementations;
using Inventario.Web.Services.Http;
using Inventario.Core;
using Inventario.Web.Models;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuración de la aplicación
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuración de HttpClient
var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5000";
Console.WriteLine($"Configurando HttpClient con URL base: {apiUrl}");

// Configurar HttpClient para la aplicación
builder.Services.AddScoped<AuthTokenHandler>();

// Configurar servicios de autenticación
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

// Configurar HttpClient con manejador de autenticación
builder.Services.AddHttpClient("ServerAPI", client => 
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<AuthTokenHandler>();

// Configurar HttpClient por defecto
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

// Registrar servicios personalizados
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<Inventario.Web.Services.Interfaces.IAuthService, Inventario.Web.Services.Implementations.AuthService>();

// Registrar IAppSettingsService
builder.Services.AddScoped<Inventario.Core.Interfaces.IAppSettingsService, Inventario.Infrastructure.Services.AppSettingsService>();

// Configurar servicios de MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Registrar IEmailService
builder.Services.AddScoped<Inventario.Core.Interfaces.IEmailService, Inventario.Infrastructure.Services.MailtrapEmailService>();
// El servicio ILocalStorageService ya está registrado por AddBlazoredLocalStorage()

// Registrar servicios de dominio
// TODO: Implementar servicios de dominio cuando sean necesarios

// Configurar opciones de correo electrónico
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configuración de MudBlazor
builder.Services.AddMudServices(config =>
{
    // Configuración de Snackbar
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 5000;
    config.SnackbarConfiguration.HideTransitionDuration = 200;
    config.SnackbarConfiguration.ShowTransitionDuration = 200;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Configuración de idioma
var culture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Construir y ejecutar la aplicación
await builder.Build().RunAsync();
