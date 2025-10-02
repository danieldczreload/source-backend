# SourceBackend

ASP.NET Core 8 backend with Clean Architecture, CQRS Light, and EF Core.

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server (local or remote)
- Git

### First Time Setup

1. **Clone and restore packages**
   ```bash
   git clone <repository-url>
   cd source-backend
   dotnet restore
   ```

2. **Configure database**
   
   Follow the [Database Setup Guide](Documentation/DATABASE-SETUP.md) to:
   - Configure connection string in user-secrets
   - Apply initial migrations

3. **Run the application**
   ```bash
   dotnet run --project src/Web/SourceBackend.Web.csproj
   ```

## 📚 Documentation

- **[Documentation Index](Documentation/README.md)** - Complete documentation overview
- **[Database Setup](Documentation/DATABASE-SETUP.md)** - Initial database configuration
- **[Creating Entities](Documentation/CREATING-ENTITIES.md)** - Step-by-step guide for new entities
- **[Data Layer](src/Infrastructure/Data/README.md)** - Technical documentation for data access

## 🏗️ Architecture

This project follows:

- **Clean Architecture** - Core → Infrastructure → Web (no circular dependencies)
- **CQRS Light** - Commands and Queries separated in Core
- **Direct DbContext** - No generic repository pattern (KISS/YAGNI)
- **Pluggable JWT Auth** - Supports LocalSymmetric, LocalAsymmetric, and OIDC
- **RBAC** - Role-based authorization with `[Authorize(Roles="...")]`

For complete architecture rules, see [.cursorrules](.cursorrules).

## 🔧 Tech Stack

- **.NET 8** - Latest LTS version
- **ASP.NET Core** - Web API with Controllers
- **Entity Framework Core 8** - SQL Server provider
- **Serilog** - Structured logging
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API documentation (Dev only)

## 📁 Project Structure

```
src/
├── Core/                          # Business logic & use cases
│   ├── Entities/                  # Domain entities
│   ├── Features/                  # CQRS handlers (Queries/Commands)
│   └── Abstractions/              # Interfaces/ports
├── Infrastructure/                # External adapters
│   └── Data/                      # EF Core & database
└── Web/                           # HTTP entry point
    ├── Controllers/               # API endpoints
    └── Auth/                      # JWT configuration
```

## 🛠️ Common Tasks

### Run migrations
```bash
dotnet ef database update \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

### Create new migration
```bash
dotnet ef migrations add YourMigrationName \
  --project src/Infrastructure/SourceBackend.Infrastructure.csproj \
  --startup-project src/Web/SourceBackend.Web.csproj
```

### Build
```bash
dotnet build
```

### Run tests
```bash
dotnet test
```

## 🔐 Security

- Secrets stored in **user-secrets** (local dev)
- Connection strings **never committed** to Git
- JWT keys stored securely (user-secrets or vault)
- All API endpoints require authentication by default

## 📝 Contributing

When creating new features:

1. Follow the architecture patterns in `.cursorrules`
2. Use the [Creating Entities Guide](Documentation/CREATING-ENTITIES.md)
3. Keep controllers thin (just parse/validate → call handler → return)
4. Use `TimeProvider` for testable time
5. Always propagate `CancellationToken`

## 📄 License

[Add your license here]