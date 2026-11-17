# Network Monitoring API

A backend application for monitoring network devices, built with ASP.NET Core and C#.

This project is a personal learning project focused on backend development, REST API design, authentication, database integration, and background services.

## Tech Stack

- **C# / .NET 10**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **PostgreSQL**
- **ASP.NET Core Identity**
- **JWT Authentication**
- **Scalar / OpenAPI**
- **Mapster**

## Features

- User registration and authentication
- JWT-based authentication
- User management with ASP.NET Core Identity
- Network device management
- Background service for device monitoring
- Configurable monitoring interval and ping timeout
- PostgreSQL database integration
- Entity Framework Core migrations
- REST API with a service-based architecture
- API documentation with Scalar and OpenAPI
- Centralized exception handling

## Project Structure

```text
network-monitoring/
│
├── BackgroundServices/   # Background monitoring services
├── Configuration/        # Application configuration
├── Controllers/          # API controllers
├── DTOs/                 # Data transfer objects
├── Data/                 # Database context
├── Identity/             # Authentication and identity
├── Interfaces/           # Service interfaces
├── Migrations/           # EF Core migrations
├── Models/               # Domain models
├── Properties/
├── Services/             # Business logic
│
├── Program.cs            # Application entry point
├── appsettings.json      # Application settings
└── a_webapi.csproj       # Project dependencies
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Git

### Installation

1. Clone the repository:

```bash
git clone https://github.com/devladik/network-monitoring.git
cd network-monitoring
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Configure your PostgreSQL connection string and JWT settings.

4. Apply database migrations:

```bash
dotnet ef database update
```

5. Run the application:

```bash
dotnet run
```

### API Documentation

When running in the Development environment, API documentation is available through Scalar.

Open the URL displayed in the application output and navigate to `/scalar`.

## Project Goals

The main goal of this project is to develop practical backend engineering skills and build a solid understanding of the .NET ecosystem.

The project is being developed incrementally, with a focus on clean architecture, maintainable code, security, and real-world backend concepts.

## Author

**Vladyslav Kuznenko**

[GitHub](https://github.com/devladik)
