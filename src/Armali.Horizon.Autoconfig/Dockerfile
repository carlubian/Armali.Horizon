FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build
COPY ["src/Armali.Horizon.Autoconfig/Armali.Horizon.Autoconfig.csproj", "src/Armali.Horizon.Autoconfig/"]
RUN dotnet restore "src/Armali.Horizon.Autoconfig/Armali.Horizon.Autoconfig.csproj"
COPY . .
WORKDIR "/build/src/Armali.Horizon.Autoconfig"
RUN dotnet build "./Armali.Horizon.Autoconfig.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Armali.Horizon.Autoconfig.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Armali.Horizon.Autoconfig.dll"]
