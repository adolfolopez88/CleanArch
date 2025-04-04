# Clean Architecture Boilerplate

A modern, production-ready boilerplate for building .NET Web APIs following Clean Architecture principles.

## Features

- **Modern .NET 8**: Utilizes the latest features of .NET 8
- **Clean Architecture**: Separation of concerns with a layered architecture
- **CQRS Pattern**: Using MediatR for command/query separation
- **Robust Error Handling**: Centralized exception handling middleware
- **JWT Authentication**: Secure authentication with JWT tokens
- **API Versioning**: Built-in support for API versioning
- **Swagger Documentation**: Comprehensive API documentation
- **Health Checks**: Integrated health checks for the API and dependencies
- **Docker Support**: Docker and Docker Compose files for containerization
- **Logging**: Structured logging with Serilog
- **Validation**: Request validation with FluentValidation
- **Caching**: In-memory and distributed Redis caching
- **Monitoring**: Prometheus and Grafana for monitoring
- **Security**: Implementation of security best practices

## Project Structure

```
CleanArchBoilerplate/
├── src/
│   ├── CleanArch.Application/        # Application layer (use cases, DTOs)
│   ├── CleanArch.Domain/             # Domain layer (entities, interfaces)
│   ├── CleanArch.Infrastructure.*/   # Infrastructure implementations
│   └── CleanArch.WebApi/             # API layer (controllers, middleware)
├── tests/
│   ├── CleanArch.UnitTests/          # Unit tests
│   └── CleanArch.IntegrationTests/   # Integration tests
├── docs/                             # Documentation
├── Dockerfile                        # Docker build file
└── docker-compose.yml                # Docker Compose file
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (optional)
- SQL Server (or Docker version)

### Running Locally

1. Clone the repository
2. Navigate to the project directory
3. Run the application:

```bash
cd src
dotnet restore
dotnet run --project CleanArch.WebApi
```

### Running with Docker

```bash
# Set up environment variables
export DB_PASSWORD=YourStrongPassword
export REDIS_PASSWORD=YourRedisPassword
export JWT_SECRET=YourJwtSecretKey
export GRAFANA_PASSWORD=YourGrafanaPassword

# Start all services
docker-compose up -d
```

The API will be available at `http://localhost:8080/swagger`.

## Development Guidelines

### Adding a New Feature

1. Create the domain entities in `CleanArch.Domain`
2. Define interfaces in `CleanArch.Domain.Interfaces`
3. Implement use cases in `CleanArch.Application`
4. Add API endpoints in `CleanArch.WebApi.Controllers`

### Adding New Infrastructure

1. Implement the domain interfaces in the appropriate Infrastructure project
2. Register the services in the corresponding `ServiceCollectionExtensions.cs`

## Testing

```bash
# Run unit tests
dotnet test tests/CleanArch.UnitTests

# Run integration tests
dotnet test tests/CleanArch.IntegrationTests
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.