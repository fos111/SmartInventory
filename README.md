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
- **Caching**: Redis 7 (docker image `redis:7-alpine`) — rate limiting, message dedup, query cache, auth tokens
- **IoT**: HiveMQ Cloud (MQTT TLS 8883) — equipment location tracking

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
| | **Mobile Products** | |
| POST | `/api/mobile/products` | Create product with multipart/photo upload |
| POST | `/api/mobile/products/json` | Create product via JSON (backward compat) |
| PUT | `/api/mobile/products/{id}` | Update product with multipart/photo upload |
| PUT | `/api/mobile/products/{id}/status` | Update product status |
| PUT | `/api/mobile/products/{id}/location` | Move product to another room |
| PUT | `/api/mobile/products/{id}/ble-id` | Update BLE ID (unique — must clear from old asset first) |
| PUT | `/api/mobile/products/{id}/price` | Update price |
| DELETE | `/api/mobile/products/{id}` | Delete product |
| POST | `/api/mobile/products/scan-history` | Record a product scan |
| GET | `/api/mobile/products/scan-history` | Get scan history |
| | **Mobile Lookups** | |
| GET | `/api/mobile/lookups/categories` | List categories |
| GET | `/api/mobile/lookups/departments` | List departments |
| GET | `/api/mobile/lookups/departments/{zoneId}/rooms` | List rooms by department |
| GET | `/api/mobile/lookups/stats` | Inventory stats |
| GET | `/api/mobile/lookups/barcode-check` | Check if asset tag exists (barcode lookup) |
| POST | `/api/mobile/lookups/scan-history` | Record a department QR scan |
| GET | `/api/mobile/lookups/move-log` | Move log history |
| | **Mobile Auth** | |
| POST | `/api/mobile/auth/register` | Register a new user |
| POST | `/api/mobile/auth/login` | Login |
| POST | `/api/mobile/auth/verify-email` | Verify email with OTP |
| POST | `/api/mobile/auth/refresh` | Refresh JWT token |
| POST | `/api/mobile/auth/logout` | Logout (revoke refresh token) |
| GET | `/api/mobile/auth/me` | Get current user profile |
| PUT | `/api/mobile/auth/profile` | Update avatar (multipart) |

### IoT Location Service (MQTT)

The project includes an **IoT Location Tracking** module (`SmartInventory.IoTLocation`) that:
- Connects to **HiveMQ Cloud** (TLS port 8883) on startup
- Subscribes to single topic `iot/location` (QoS 1)
- Processes IoT device reports: validates payloads, updates asset locations, tracks full history
- Runs as a `BackgroundService` hosted inside the API process

See [`docs/business/specs/SPEC.md`](docs/business/specs/SPEC.md) for full specification and design decisions.

---

## Configuration

Configuration is managed via `appsettings.json` files and environment variables.

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | ✅ |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | optional |
| `Jwt__IssuerSigningKey` | JWT signing key | optional (has default) |
| `SMTP_USERNAME` | SMTP username for email (Mailtrap) | ✅ |
| `SMTP_PASSWORD` | SMTP password for email (Mailtrap) | ✅ |
| `ConnectionStrings__Redis` | Redis connection string | optional (default: `localhost:6379`) |
| `HIVEMQ_USERNAME` | HiveMQ Cloud MQTT username | ✅ |
| `HIVEMQ_PASSWORD` | HiveMQ Cloud MQTT password | ✅ |

### Environment File (`.env`)

The project includes a `.env` file (gitignored) loaded by `run-api.sh`. Required variables:

```bash
SMTP_USERNAME=api
SMTP_PASSWORD=...
HIVEMQ_USERNAME=smartinventory
HIVEMQ_PASSWORD=...
```

### Redis

The project uses **Redis 7** for:
- **Rate limiting**: IoT per-asset 30-second rate limiting
- **Message dedup**: QoS-1 redelivery deduplication (24h TTL)
- **Query cache**: Reporting summaries (5min TTL)
- **Entity cache**: Asset by ID/tag, location hierarchy (5-30min TTL)
- **Auth tokens**: JWT refresh tokens, password reset OTPs (self-expiring)

Redis starts automatically via `docker-compose up -d` (service: `redis:7-alpine`, port 6379).

If Redis is unavailable, all cache consumers degrade gracefully — the system continues without caching, rate limiting, or dedup. Only `RedisRefreshTokenRepository` and `RedisPasswordResetTokenRepository` require Redis (auth token storage).

See [`docs/architecture/decisions/REDIS_ADR.md`](docs/architecture/decisions/REDIS_ADR.md) for the full architecture decision record.

### Quick Start with `run-api.sh`

```bash
./run-api.sh
```

This script sources `.env` into shell environment variables, then runs the API. The IoT Location service (MQTT subscriber) starts automatically as an embedded hosted service within the API process — no separate process needed.

### File Storage

Photos and avatars are saved to `{appRoot}/uploads/{container}/` via `LocalFileStorageService`.
- Product photos: saved to `uploads/products/`
- User avatars: saved to `uploads/avatars/`
- URLs are returned as `/uploads/{container}/{fileName}`
- Supported formats: JPEG, PNG (max 5MB)

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
