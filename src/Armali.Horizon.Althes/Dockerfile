FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5200

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0

WORKDIR /build

# Copiar todos los .csproj necesarios para el restore
COPY ["src/Armali.Horizon.Althes/Armali.Horizon.Althes.csproj", "src/Armali.Horizon.Althes/"]
COPY ["src/Armali.Horizon.Core/Armali.Horizon.Core.csproj", "src/Armali.Horizon.Core/"]
COPY ["src/Armali.Horizon.IO/Armali.Horizon.IO.csproj", "src/Armali.Horizon.IO/"]
COPY ["src/Armali.Horizon.Contracts/Armali.Horizon.Contracts.csproj", "src/Armali.Horizon.Contracts/"]
RUN dotnet restore "src/Armali.Horizon.Althes/Armali.Horizon.Althes.csproj"

COPY . .

WORKDIR "/build/src/Armali.Horizon.Althes"
RUN dotnet build "./Armali.Horizon.Althes.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Version=$APP_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0
RUN dotnet publish "./Armali.Horizon.Althes.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Volumen para la BD SQLite de la instancia
USER root
RUN mkdir -p /persistence && chown $APP_UID /persistence
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=5200

ENTRYPOINT ["dotnet", "Armali.Horizon.Althes.dll"]

