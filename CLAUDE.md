# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

- **Build solution**: `dotnet build src/CleanArch.sln`
- **Run API**: `dotnet run --project src/CleanArch.WebApi`
- **Restore packages**: `dotnet restore src/CleanArch.sln`
- **Run unit tests**: `dotnet test tests/CleanArch.UnitTests`
- **Run integration tests**: `dotnet test tests/CleanArch.IntegrationTests`
- **Run specific test**: `dotnet test tests/CleanArch.UnitTests --filter "FullyQualifiedName~TestClassName.TestMethodName"`
- **Apply migrations**: `dotnet ef database update --project src/CleanArch.WebApi`

## Code Style Guidelines

- **Architecture**: Follow Clean Architecture principles with Domain, Application, Infrastructure, and WebApi layers
- **Naming**: PascalCase for classes/methods/properties, camelCase for variables, prefixed interfaces with 'I'
- **Error Handling**: Use the `SafeExecuteResult<T>` pattern for domain operations, handle exceptions in middleware
- **CQRS Pattern**: Define commands (write) and queries (read) in Application layer using MediatR
- **Validation**: Create validators using FluentValidation, register in DI container
- **Logging**: Use structured logging with Serilog, inject ILogger<T>, include relevant context
- **Testing**: Use xUnit with Moq and FluentAssertions, follow AAA pattern (Arrange-Act-Assert)
- **Nullable Reference Types**: Enabled project-wide, properly annotate nullable references
- **Asynchronous Code**: Use async/await consistently, return Task<T> for asynchronous methods

## Database Setup

1. Make sure Docker is installed and running
2. Run the setup script to create a database user with full permissions:

```bash
cd /app/CleanArchBoilerPlateGit
./setup-database.sh
```

This script will:
- Start the Docker containers
- Create a user 'dbadmin' with password 'DbAdmin123!' in SQL Server
- Grant the user sysadmin and db_owner permissions
- Update the connection string in appsettings.Development.json

## Default Admin User (Seeded)

The application includes a default admin user that will be created on first run:

- Username: admin
- Email: admin@cleanarch.com
- Password: Admin123!

This user has full administrative privileges in the application.