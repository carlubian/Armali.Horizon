FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0
WORKDIR /build
COPY ["src/Armali.Horizon.Autoconfig/Armali.Horizon.Autoconfig.csproj", "src/Armali.Horizon.Autoconfig/"]
RUN dotnet restore "src/Armali.Horizon.Autoconfig/Armali.Horizon.Autoconfig.csproj"
COPY . .
WORKDIR "/build/src/Armali.Horizon.Autoconfig"
RUN dotnet build "./Armali.Horizon.Autoconfig.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:Version=$APP_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0
RUN dotnet publish "./Armali.Horizon.Autoconfig.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Armali.Horizon.Autoconfig.dll"]
