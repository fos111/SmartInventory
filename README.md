# SmartInventoryApi

A .NET 8 Web API backend for Smart Inventory management with Clean Architecture.

## Tech Stack

- **Framework**: .NET 8.0
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API layers)
- **Database**: PostgreSQL 16 with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Testing**: xUnit + FluentAssertions + Moq
- **Containerization**: Docker + docker-compose
- **CI/CD**: GitHub Actions

## Project Structure

```
SmartInventoryApi/
├── src/
│   ├── SmartInventoryApi.Domain/         # Domain entities, interfaces
│   ├── SmartInventoryApi.Application/      # Use cases, DTOs, services
│   ├── SmartInventoryApi.Infrastructure/   # EF Core, repositories
│   └── SmartInventoryApi.Api/              # Controllers, Program.cs
├── tests/
│   ├── SmartInventoryApi.Domain.Tests/
│   ├── SmartInventoryApi.Application.Tests/
│   ├── SmartInventoryApi.Infrastructure.Tests/
│   └── SmartInventoryApi.Api.Tests/
├── .github/workflows/                      # CI/CD pipelines
├── docker-compose.yml
├── Dockerfile
└── SmartInventoryApi.sln
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL 16 (optional, can use Docker)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/your-username/SmartInventoryApi.git
cd SmartInventoryApi

# Start infrastructure
docker-compose up -d

# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run API
dotnet run --project src/SmartInventoryApi.Api
```

### Using Docker

```bash
# Build and run all services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/swagger` | API documentation (Development) |

## Configuration

Configuration is managed via `appsettings.json` files and environment variables.

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) |
| `Jwt__IssuerSigningKey` | JWT signing key |

## Development

### Adding a New Layer

```bash
# Create new project
dotnet new classlib -n SmartInventoryApi.NewLayer -o src/SmartInventoryApi.NewLayer

# Add to solution
dotnet sln add src/SmartInventoryApi.NewLayer/SmartInventoryApi.NewLayer.csproj

# Add reference
dotnet add src/SmartInventoryApi.NewLayer reference SmartInventoryApi.Domain
```

### Creating Migrations

```bash
dotnet ef migrations add InitialCreate --project src/SmartInventoryApi.Infrastructure --startup-project src/SmartInventoryApi.Api
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/SmartInventoryApi.Domain.Tests
```

## CI/CD

The project uses GitHub Actions for:
- Build verification
- Unit testing
- Docker image building
- Code quality checks

See `.github/workflows/` for configuration.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our development workflow.

## License

This project is licensed under the MIT License.
