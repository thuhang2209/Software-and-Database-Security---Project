# Software and Database Security Project

A secure learning management system (LMS) built with ASP.NET Core MVC and Microsoft SQL Server. The project demonstrates how application and database security controls can be integrated into a web application for managing users, classrooms, lessons, and submissions.

## Features

- User authentication and authorization with ASP.NET Core Identity
- Secure login workflow with access-denied handling
- Password policy requiring:
  - At least 8 characters
  - One uppercase letter
  - One lowercase letter
  - One digit
  - One non-alphanumeric character
- Account lockout after five failed login attempts for 15 minutes
- Two-hour authentication cookies with sliding expiration
- Role-based identity support
- Automatic antiforgery validation for POST requests to help protect against CSRF attacks
- HTTPS redirection and HSTS in non-development environments
- SQL Server persistence through Entity Framework Core
- Database initialization and sample data seeding
- Management of classrooms, lessons, and submissions

## Technology Stack

- **Framework:** ASP.NET Core MVC
- **Runtime:** .NET 10
- **Authentication:** ASP.NET Core Identity
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server / SQL Server LocalDB
- **Language:** C#

## Project Structure

```text
.
├── Controllers/
│   ├── AccountController.cs
│   ├── ClassroomsController.cs
│   ├── HomeController.cs
│   ├── LessonsController.cs
│   └── SubmissionsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Database initialization and seed logic
├── Migrations/
├── Models/
│   ├── Entities.cs
│   ├── ErrorViewModel.cs
│   └── LoginViewModel.cs
├── Views/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── Software-and-Database-Security---Project.csproj
```

## Prerequisites

Install the following before running the project:

- .NET 10 SDK
- SQL Server LocalDB, SQL Server Express, or another SQL Server instance
- Visual Studio 2022 or a compatible IDE/editor

Verify the .NET SDK installation:

```bash
dotnet --version
```

## Configuration

The default development connection string uses SQL Server LocalDB:

```text
Server=(localdb)\\mssqllocaldb;Database=SecureLmsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

To use another SQL Server instance, update `ConnectionStrings:DefaultConnection` in `appsettings.json` or provide the value through an environment-specific configuration file. Do not commit production credentials or passwords to the repository.

## Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/thuhang2209/Software-and-Database-Security---Project.git
   cd Software-and-Database-Security---Project
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Apply Entity Framework Core migrations, if required:

   ```bash
   dotnet ef database update
   ```

   If the EF Core CLI tool is not installed, install it with:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. Run the application:

   ```bash
   dotnet run
   ```

5. Open the HTTPS URL shown in the terminal. The default route starts at the account login page.

On startup, the application attempts to initialize the database and seed sample data through the configured database initializer.

## Security Notes

This project includes several security-focused configurations:

- Password hashing is provided by ASP.NET Core Identity.
- Failed login attempts trigger temporary account lockout to reduce brute-force attacks.
- Antiforgery validation is applied globally to controller actions handling POST requests.
- Authentication cookies expire after two hours and use sliding expiration.
- HTTPS is enforced through redirection, and HSTS is enabled outside development.
- Database access is handled through Entity Framework Core rather than manually concatenated SQL queries.

For production deployment, replace LocalDB with a managed or secured SQL Server instance, store secrets outside source control, use HTTPS certificates, review authorization policies, and run the application with production-specific configuration.

## Development Commands

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Create a migration
dotnet ef migrations add <MigrationName>

# Update the database
dotnet ef database update
```

