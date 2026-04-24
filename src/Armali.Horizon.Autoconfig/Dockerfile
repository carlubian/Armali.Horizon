FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0

# Descargar el CLI standalone de Tailwind CSS para Linux
ADD https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64 /usr/local/bin/tailwindcss
RUN chmod +x /usr/local/bin/tailwindcss

WORKDIR /build

# Copiar todos los .csproj necesarios para el restore
COPY ["src/Armali.Horizon.Autoconfig/Armali.Horizon.Autoconfig.csproj", "src/Armali.Horizon.Autoconfig/"]
COPY ["src/Armali.Horizon.Blazor/Armali.Horizon.Blazor.csproj", "src/Armali.Horizon.Blazor/"]
COPY ["src/Armali.Horizon.Core/Armali.Horizon.Core.csproj", "src/Armali.Horizon.Core/"]
COPY ["src/Armali.Horizon.IO/Armali.Horizon.IO.csproj", "src/Armali.Horizon.IO/"]
COPY ["src/Armali.Horizon.Contracts/Armali.Horizon.Contracts.csproj", "src/Armali.Horizon.Contracts/"]
RUN dotnet restore "src/Armali.Horizon.Autoconfig/Armali.Horizon.Autoconfig.csproj"

COPY . .

# Compilar Tailwind CSS con el binario Linux antes del build de .NET
RUN tailwindcss -i ./src/Armali.Horizon.Blazor/wwwroot/app.css -o ./src/Armali.Horizon.Blazor/wwwroot/tailwind.css --minify
RUN tailwindcss -i ./src/Armali.Horizon.Autoconfig/wwwroot/app.css -o ./src/Armali.Horizon.Autoconfig/wwwroot/tailwind.css --minify

WORKDIR "/build/src/Armali.Horizon.Autoconfig"
RUN dotnet build "./Armali.Horizon.Autoconfig.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:SkipTailwindBuild=true /p:Version=$APP_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0
RUN dotnet publish "./Armali.Horizon.Autoconfig.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:SkipTailwindBuild=true /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para la base de datos SQLite (punto de montaje del volumen)
USER root
RUN mkdir -p /persistence && chown $APP_UID /persistence
USER $APP_UID

# Configurar el puerto HTTP de la aplicación
ENV ASPNETCORE_HTTP_PORTS=5004

ENTRYPOINT ["dotnet", "Armali.Horizon.Autoconfig.dll"]
