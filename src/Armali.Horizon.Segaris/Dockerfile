FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5122

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

# Descargar el CLI standalone de Tailwind CSS para Linux
ADD https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64 /usr/local/bin/tailwindcss
RUN chmod +x /usr/local/bin/tailwindcss

WORKDIR /build

# Copiar todos los .csproj necesarios para el restore (Segaris → Blazor → Core)
COPY ["src/Armali.Horizon.Segaris/Armali.Horizon.Segaris.csproj", "src/Armali.Horizon.Segaris/"]
COPY ["src/Armali.Horizon.Blazor/Armali.Horizon.Blazor.csproj", "src/Armali.Horizon.Blazor/"]
COPY ["src/Armali.Horizon.Core/Armali.Horizon.Core.csproj", "src/Armali.Horizon.Core/"]
RUN dotnet restore "src/Armali.Horizon.Segaris/Armali.Horizon.Segaris.csproj"

COPY . .

# Compilar Tailwind CSS con el binario Linux antes del build de .NET
RUN tailwindcss -i ./src/Armali.Horizon.Blazor/wwwroot/app.css -o ./src/Armali.Horizon.Blazor/wwwroot/tailwind.css --minify
RUN tailwindcss -i ./src/Armali.Horizon.Segaris/wwwroot/app.css -o ./src/Armali.Horizon.Segaris/wwwroot/tailwind.css --minify

WORKDIR "/build/src/Armali.Horizon.Segaris"
RUN dotnet build "./Armali.Horizon.Segaris.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:SkipTailwindBuild=true

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Armali.Horizon.Segaris.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:SkipTailwindBuild=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para la base de datos SQLite (punto de montaje del volumen)
USER root
RUN mkdir -p /persistence && chown $APP_UID /persistence
USER $APP_UID

# Configurar el puerto HTTP de la aplicación
ENV ASPNETCORE_HTTP_PORTS=5122

ENTRYPOINT ["dotnet", "Armali.Horizon.Segaris.dll"]
