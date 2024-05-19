FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NSwag.ConsoleApplication/NSwag.ConsoleApplication.csproj", "NSwag.ConsoleApplication/"]
RUN dotnet restore "NSwag.ConsoleApplication/NSwag.ConsoleApplication.csproj"
COPY . .
WORKDIR "/src/NSwag.ConsoleApplication"
RUN dotnet build "NSwag.ConsoleApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NSwag.ConsoleApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NSwag.ConsoleApplication.dll"]
