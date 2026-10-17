FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ShiftManager.sln ./
COPY src/ShiftManager.Domain/*.csproj src/ShiftManager.Domain/
COPY src/ShiftManager.Application/*.csproj src/ShiftManager.Application/
COPY src/ShiftManager.Infrastructure/*.csproj src/ShiftManager.Infrastructure/
COPY src/ShiftManager.Api/*.csproj src/ShiftManager.Api/
COPY tests/ShiftManager.UnitTests/*.csproj tests/ShiftManager.UnitTests/
RUN dotnet restore

COPY src/ src/
RUN dotnet publish src/ShiftManager.Api -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ShiftManager.Api.dll"]
