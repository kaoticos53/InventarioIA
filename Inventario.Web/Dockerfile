# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["Inventario.sln", "./"]
COPY ["Inventario.Web/*.csproj", "Inventario.Web/"]
COPY ["Inventario.Core/*.csproj", "Inventario.Core/"]
COPY ["Inventario.Infrastructure/*.csproj", "Inventario.Infrastructure/"]

RUN dotnet restore "Inventario.Web/Inventario.Web.csproj"

# Copiar el resto del código
COPY . .

# Construir la aplicación
WORKDIR "/src/Inventario.Web"
RUN dotnet build "Inventario.Web.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "Inventario.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM nginx:alpine AS final

# Copiar archivos estáticos
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .

# Copiar configuración de Nginx
COPY Inventario.Web/nginx.conf /etc/nginx/nginx.conf

# Exponer el puerto
EXPOSE 80

# Iniciar Nginx
ENTRYPOINT ["nginx", "-g", "daemon off;"]
