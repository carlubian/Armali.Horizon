FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0

WORKDIR /build

# Copiar todos los .csproj necesarios para el restore (Megnir → IO → Core)
COPY ["src/Armali.Horizon.Megnir/Armali.Horizon.Megnir.csproj", "src/Armali.Horizon.Megnir/"]
COPY ["src/Armali.Horizon.IO/Armali.Horizon.IO.csproj", "src/Armali.Horizon.IO/"]
COPY ["src/Armali.Horizon.Core/Armali.Horizon.Core.csproj", "src/Armali.Horizon.Core/"]
RUN dotnet restore "src/Armali.Horizon.Megnir/Armali.Horizon.Megnir.csproj"

COPY . .

WORKDIR "/build/src/Armali.Horizon.Megnir"
RUN dotnet build "./Armali.Horizon.Megnir.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Version=$APP_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0
RUN dotnet publish "./Armali.Horizon.Megnir.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Armali.Horizon.Megnir.dll"]

