# Crear la solución
dotnet new sln -n Inventario

# Crear los proyectos
# API
New-Item -ItemType Directory -Path "Inventario.Api" -Force
Set-Location "Inventario.Api"
dotnet new webapi --force
Set-Location ".."

# Web (Blazor)
New-Item -ItemType Directory -Path "Inventario.Web" -Force
Set-Location "Inventario.Web"
dotnet new blazorwasm --no-https --force
Set-Location ".."

# Core (Class Library)
New-Item -ItemType Directory -Path "Inventario.Core" -Force
Set-Location "Inventario.Core"
dotnet new classlib --force
Set-Location ".."

# Infrastructure (Class Library)
New-Item -ItemType Directory -Path "Inventario.Infrastructure" -Force
Set-Location "Inventario.Infrastructure"
dotnet new classlib --force
Set-Location ".."

# Tests (xUnit)
New-Item -ItemType Directory -Path "Inventario.Tests" -Force
Set-Location "Inventario.Tests"
dotnet new xunit --force
Set-Location ".."

# Agregar proyectos a la solución
dotnet sln add "Inventario.Api/Inventario.Api.csproj"
dotnet sln add "Inventario.Web/Inventario.Web.csproj"
dotnet sln add "Inventario.Core/Inventario.Core.csproj"
dotnet sln add "Inventario.Infrastructure/Inventario.Infrastructure.csproj"
dotnet sln add "Inventario.Tests/Inventario.Tests.csproj"

# Agregar referencias entre proyectos
# API depende de Core e Infrastructure
Set-Location "Inventario.Api"
dotnet add reference "../Inventario.Core/Inventario.Core.csproj"
dotnet add reference "../Inventario.Infrastructure/Inventario.Infrastructure.csproj"
Set-Location ".."

# Web depende de Core
Set-Location "Inventario.Web"
dotnet add reference "../Inventario.Core/Inventario.Core.csproj"
Set-Location ".."

# Infrastructure depende de Core
Set-Location "Inventario.Infrastructure"
dotnet add reference "../Inventario.Core/Inventario.Core.csproj"
Set-Location ".."

# Tests dependen de los proyectos a probar
Set-Location "Inventario.Tests"
dotnet add reference "../Inventario.Api/Inventario.Api.csproj"
dotnet add reference "../Inventario.Core/Inventario.Core.csproj"
dotnet add reference "../Inventario.Infrastructure/Inventario.Infrastructure.csproj"
Set-Location ".."

Write-Host "Configuración completada. La solución está lista para usar."
