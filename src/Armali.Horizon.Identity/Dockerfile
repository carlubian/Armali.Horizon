FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Armali.Horizon.Identity/Armali.Horizon.Identity.csproj", "src/Armali.Horizon.Identity/"]
RUN dotnet restore "src/Armali.Horizon.Identity/Armali.Horizon.Identity.csproj"
COPY . .
WORKDIR "/src/src/Armali.Horizon.Identity"
RUN dotnet build "./Armali.Horizon.Identity.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Armali.Horizon.Identity.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Armali.Horizon.Identity.dll"]
