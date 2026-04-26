FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5180

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0

WORKDIR /build

# Copiar todos los .csproj necesarios para el restore
COPY ["src/Armali.Horizon.MCP/Armali.Horizon.MCP.csproj", "src/Armali.Horizon.MCP/"]
COPY ["src/Armali.Horizon.Core/Armali.Horizon.Core.csproj", "src/Armali.Horizon.Core/"]
COPY ["src/Armali.Horizon.IO/Armali.Horizon.IO.csproj", "src/Armali.Horizon.IO/"]
COPY ["src/Armali.Horizon.Contracts/Armali.Horizon.Contracts.csproj", "src/Armali.Horizon.Contracts/"]
RUN dotnet restore "src/Armali.Horizon.MCP/Armali.Horizon.MCP.csproj"

COPY . .

WORKDIR "/build/src/Armali.Horizon.MCP"
RUN dotnet build "./Armali.Horizon.MCP.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Version=$APP_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0
RUN dotnet publish "./Armali.Horizon.MCP.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar el puerto HTTP de la aplicación
ENV ASPNETCORE_HTTP_PORTS=5180

ENTRYPOINT ["dotnet", "Armali.Horizon.MCP.dll"]

