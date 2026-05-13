# Contributing to SmartInventoryApi

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/SmartInventoryApi.git`
3. Create a branch: `git checkout -b feature/your-feature-name`

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- Docker & Docker Compose
- PostgreSQL 16 (via Docker)

### Local Development

```bash
# Start infrastructure
docker-compose up -d db

# Restore packages
dotnet restore

# Run tests
dotnet test

# Run API locally
dotnet run --project src/SmartInventoryApi.Api
```

## Branching Strategy

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - Feature branches (short-lived, < 2 days)
- `bugfix/*` - Bug fix branches
- `hotfix/*` - Emergency production fixes

## Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(api): add user authentication endpoint
fix(auth): handle expired tokens gracefully
docs(readme): update installation instructions
refactor(users): simplify validation logic
ci: add GitHub Actions workflow for testing
chore: update dependencies
```

## Pull Request Process

1. Create a PR from your branch to `develop`
2. Ensure all CI checks pass
3. Get at least 1 approval
4. Squash and merge

## Code Style

- Follow `.editorconfig` settings
- Enable format on save in your IDE
- Run `dotnet format` before committing

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Docker

```bash
# Build image
docker build -t smartinventoryapi .

# Run with docker-compose
docker-compose up -d

# Stop services
docker-compose down
```
