# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["Inventario.sln", "./"]
COPY ["Inventario.Api/*.csproj", "Inventario.Api/"]
COPY ["Inventario.Core/*.csproj", "Inventario.Core/"]
COPY ["Inventario.Infrastructure/*.csproj", "Inventario.Infrastructure/"]

RUN dotnet restore "Inventario.Api/Inventario.Api.csproj"

# Copiar el resto del código
COPY . .

# Construir la aplicación
WORKDIR "/src/Inventario.Api"
RUN dotnet build "Inventario.Api.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "Inventario.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Instalar herramientas de diagnóstico
# Instalar herramientas básicas
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copiar la aplicación publicada
COPY --from=publish /app/publish .

# Exponer el puerto
EXPOSE 80

# Definir el punto de entrada
ENTRYPOINT ["dotnet", "Inventario.Api.dll"]
