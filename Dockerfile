# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SmartInventory.sln .
COPY Directory.Build.props .
COPY global.json .
COPY src/SmartInventory.Domain/SmartInventory.Domain.csproj src/SmartInventory.Domain/
COPY src/SmartInventory.Application/SmartInventory.Application.csproj src/SmartInventory.Application/
COPY src/SmartInventory.Infrastructure/SmartInventory.Infrastructure.csproj src/SmartInventory.Infrastructure/
COPY src/SmartInventory.Api/SmartInventory.Api.csproj src/SmartInventory.Api/
COPY src/SmartInventory.IoTLocation/SmartInventory.IoTLocation.csproj src/SmartInventory.IoTLocation/
COPY tests/SmartInventory.Domain.Tests/SmartInventory.Domain.Tests.csproj tests/SmartInventory.Domain.Tests/
COPY tests/SmartInventory.Application.Tests/SmartInventory.Application.Tests.csproj tests/SmartInventory.Application.Tests/
COPY tests/SmartInventory.Infrastructure.Tests/SmartInventory.Infrastructure.Tests.csproj tests/SmartInventory.Infrastructure.Tests/
COPY tests/SmartInventory.Api.Tests/SmartInventory.Api.Tests.csproj tests/SmartInventory.Api.Tests/

RUN dotnet restore

COPY . .
RUN dotnet build -c Release --no-restore

RUN dotnet test -c Release --no-build --verbosity normal

RUN dotnet publish src/SmartInventory.Api/SmartInventory.Api.csproj -c Release -o /app --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app .

ENTRYPOINT ["dotnet", "SmartInventory.Api.dll"]
