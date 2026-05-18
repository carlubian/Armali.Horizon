FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5210

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0

WORKDIR /build

# Descargar tailwindcss standalone para Linux y compilar CSS
RUN apt-get update && apt-get install -y curl && \
    curl -sLo /usr/local/bin/tailwindcss \
    https://github.com/tailwindlabs/tailwindcss/releases/latest/download/tailwindcss-linux-x64 && \
    chmod +x /usr/local/bin/tailwindcss

COPY ["src/Armali.Horizon.Althes.UI/Armali.Horizon.Althes.UI.csproj",  "src/Armali.Horizon.Althes.UI/"]
COPY ["src/Armali.Horizon.Core/Armali.Horizon.Core.csproj",            "src/Armali.Horizon.Core/"]
COPY ["src/Armali.Horizon.IO/Armali.Horizon.IO.csproj",                "src/Armali.Horizon.IO/"]
COPY ["src/Armali.Horizon.Contracts/Armali.Horizon.Contracts.csproj",  "src/Armali.Horizon.Contracts/"]
COPY ["src/Armali.Horizon.Blazor/Armali.Horizon.Blazor.csproj",        "src/Armali.Horizon.Blazor/"]
RUN dotnet restore "src/Armali.Horizon.Althes.UI/Armali.Horizon.Althes.UI.csproj"

COPY . .

# Compilar CSS de la biblioteca compartida y de la app
WORKDIR "/build/src/Armali.Horizon.Blazor"
RUN tailwindcss -i ./wwwroot/app.css -o ./wwwroot/tailwind.css --minify

WORKDIR "/build/src/Armali.Horizon.Althes.UI"
RUN tailwindcss -i ./wwwroot/app.css -o ./wwwroot/tailwind.css --minify

RUN dotnet publish "./Armali.Horizon.Althes.UI.csproj" -c $BUILD_CONFIGURATION \
    -o /app/publish /p:UseAppHost=false /p:SkipTailwindBuild=true /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

USER root
RUN mkdir -p /persistence && chown $APP_UID /persistence
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=5210
ENTRYPOINT ["dotnet", "Armali.Horizon.Althes.UI.dll"]

