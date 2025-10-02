# Documentation

This folder contains all project documentation files.

## 📚 Available Documents

### Setup & Configuration
- **[DATABASE-SETUP.md](DATABASE-SETUP.md)** - Initial database configuration guide
  - Connection string setup (user-secrets)
  - Package installation
  - DbContext registration
  - Security best practices

### Development Guides
- **[CREATING-ENTITIES.md](CREATING-ENTITIES.md)** - Complete guide for creating entities
  - Step-by-step entity creation
  - Entity configuration (Fluent API)
  - Migration commands
  - Usage examples in handlers
  - Best practices & troubleshooting

### Other Documentation
- **[../src/Infrastructure/Data/README.md](../src/Infrastructure/Data/README.md)** - Data layer technical documentation
- **[../.cursorrules](../.cursorrules)** - Project architecture rules and conventions

## 🚀 Quick Start for New Developers

1. **First Time Setup**
   - Read `DATABASE-SETUP.md` to configure your local database
   - Ensure you have the connection string in user-secrets

2. **Creating Features**
   - Follow `CREATING-ENTITIES.md` when adding new entities
   - Review `.cursorrules` for architecture patterns (CQRS, KISS, YAGNI)

3. **Running Migrations**
   - All migration commands are documented in `CREATING-ENTITIES.md`

## 💡 Architecture Overview

This project follows:
- **Clean Architecture** (Core → Infrastructure → Web)
- **CQRS Light** (Commands/Queries in Core)
- **Direct DbContext Usage** (No generic repository pattern - KISS/YAGNI)
- **Pluggable JWT Auth** (LocalSymmetric/LocalAsymmetric/OIDC)
- **RBAC** (Role-based authorization)

For complete architecture details, see `.cursorrules`.

