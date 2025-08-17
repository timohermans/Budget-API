FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Budget.MigrationsRunner/Budget.MigrationsRunner.csproj", "src/Budget.MigrationsRunner/"]
COPY ["src/Budget.Infrastructure/Budget.Infrastructure.csproj", "src/Budget.Infrastructure/"]
COPY ["src/Budget.Application/Budget.Application.csproj", "src/Budget.Application/"]
COPY ["src/Budget.Domain/Budget.Domain.csproj", "src/Budget.Domain/"]
RUN dotnet restore "src/Budget.MigrationsRunner/Budget.MigrationsRunner.csproj"
COPY . .
WORKDIR "/src/src/Budget.MigrationsRunner"
RUN dotnet build "./Budget.MigrationsRunner.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Budget.MigrationsRunner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Budget.MigrationsRunner.dll"]
