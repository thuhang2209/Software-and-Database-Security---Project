# Software and Database Security Project

A secure Learning Management System (LMS) built with ASP.NET Core MVC, ASP.NET Core Identity, Entity Framework Core, and Microsoft SQL Server. The application demonstrates authentication, role-based authorization, secure file handling, audit logging, CSRF protection, account lockout, and database security controls.

## Features

### Authentication and Security

- Authentication and authorization with ASP.NET Core Identity.
- Three application roles:
  - `Admin`
  - `Teacher`
  - `Student`
- Password policy requiring:
  - At least 8 characters.
  - One uppercase letter.
  - One lowercase letter.
  - One digit.
  - One non-alphanumeric character.
- Account lockout after five failed login attempts for 15 minutes.
- Authentication cookies expire after two hours with sliding expiration.
- Custom login and access-denied pages.
- Automatic antiforgery validation for controller POST requests to help prevent CSRF attacks.
- HTTPS redirection and HSTS outside the Development environment.
- Login audit logging, including email, action, IP address, timestamp, and success status.
- Passwords are hashed by ASP.NET Core Identity using the configured Identity password hasher.

### Learning Management Features

- Admin classroom management.
- Assign teachers to classrooms.
- Enroll students in classrooms while preventing duplicate enrollments.
- Teachers can create lessons for their own classrooms.
- Teachers can publish lessons for enrolled students.
- Lessons can include attachments.
- Teachers can create homework assignments or quizzes.
- Assignments can include instructions, deadlines, and attached files.
- Students can view published lessons and assignments in classrooms where they are enrolled.
- Students can submit text responses and files.
- Students can update a submission until it has been graded or the deadline has passed.
- Teachers can view submissions and assign grades from 0 to 10 with feedback.
- Secure download authorization for lesson, assignment, and submission files.

## Authorization Matrix

| Functionality | Admin | Teacher | Student |
|---|:---:|:---:|:---:|
| Manage classrooms | Yes | No | No |
| Assign teachers to classrooms | Yes | No | No |
| Enroll students | Yes | No | No |
| View an assigned classroom | Yes | Yes | Yes, if enrolled |
| Create lessons | No | Yes, in own classroom | No |
| Publish lessons | No | Yes, in own classroom | No |
| Create assignments and quizzes | No | Yes, in own lessons | No |
| View published lessons and assignments | Yes | Yes, in own classroom | Yes, if enrolled |
| Submit assignments | No | No | Yes, if enrolled |
| View submissions | No | Yes, for own classroom | Own submission only |
| Grade submissions | No | Yes, for own classroom | No |

## Technology Stack

- **Framework:** ASP.NET Core MVC
- **Target runtime:** .NET 10 (`net10.0`)
- **Authentication:** ASP.NET Core Identity with Entity Framework Core
- **ORM:** Entity Framework Core 10.0.12
- **Database provider:** Microsoft.EntityFrameworkCore.SqlServer 10.0.12
- **Database:** Microsoft SQL Server / SQL Server LocalDB
- **Language:** C#
- **Frontend:** Razor Views, Bootstrap, jQuery Validation

## Data Model

The application includes the following main entities:

- `ApplicationUser`: Identity user with an additional `FullName` property.
- `Classroom`: A class assigned to a teacher.
- `Enrollment`: The relationship between a classroom and a student.
- `Lesson`: Learning content belonging to a classroom, with optional attachments and publication status.
- `Assignment`: Homework or quiz belonging to a lesson, with optional deadline and attached question file.
- `Submission`: A student's text/file submission, grade, feedback, and submission timestamp.
- `AuditLog`: Records successful, failed, and locked-out login attempts.

## Project Structure

```text
.
├── App_Data/
│   ├── Assignments/       # Uploaded assignment files
│   ├── Submissions/       # Uploaded student submission files
│   └── Uploads/           # Uploaded lesson attachments
├── Controllers/
│   ├── AccountController.cs
│   ├── ClassroomsController.cs
│   ├── HomeController.cs
│   ├── LessonsController.cs
│   └── SubmissionsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Migrations/
│   ├── 20261006061258_InitialCreate.cs
│   ├── 20261006142342_UpdateDatabase.cs
│   ├── 20261006143012_FixPendingChanges.cs
│   └── ApplicationDbContextModelSnapshot.cs
├── Models/
│   ├── Entities.cs
│   ├── ErrorViewModel.cs
│   └── LoginViewModel.cs
├── Views/
│   ├── Account/
│   ├── Classrooms/
│   ├── Home/
│   ├── Lessons/
│   ├── Shared/
│   └── Submissions/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── Software-and-Database-Security---Project.csproj
```

## Prerequisites

Install the following before running the project:

- .NET 10 SDK.
- SQL Server LocalDB, SQL Server Express, or another compatible SQL Server instance.
- Visual Studio 2022 or another compatible .NET IDE/editor.

Verify the SDK installation:

```bash
dotnet --version
```

## Configuration

The default connection string in `appsettings.json` uses SQL Server LocalDB:

```text
Server=(localdb)\\mssqllocaldb;Database=SecureLmsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

To use another SQL Server instance, update `ConnectionStrings:DefaultConnection` in `appsettings.json` or provide it through environment-specific configuration. Do not commit production credentials or connection strings to source control.

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

3. Build the project:

   ```bash
   dotnet build
   ```

4. Apply the existing Entity Framework Core migrations if required:

   ```bash
   dotnet ef database update
   ```

   If the EF Core CLI is not installed:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. Run the application:

   ```bash
   dotnet run
   ```

6. Open the HTTPS URL displayed in the terminal. The default route opens the login page at `/Account/Login`.

At startup, `DbInitializer.SeedAsync` calls `EnsureCreatedAsync`, creates the required roles, and creates the demonstration users if they do not already exist.

## Seeded Development Accounts

The development initializer creates these accounts with the password `SecurePass@123`:

| Role | Email |
|---|---|
| Admin | `admin@lms.com` |
| Teacher | `teacher1@lms.com` |
| Teacher | `teacher2@lms.com` |
| Student | `student1@lms.com` |
| Student | `student2@lms.com` |

These credentials are intended only for local testing. Change or remove them before deploying the application to a shared or production environment.

## File Upload Rules

Uploaded files are stored outside the publicly served `wwwroot` directory under `App_Data` and are given GUID-based storage names.

- Lesson attachments:
  - Maximum size: 15 MB.
  - Supported extensions: `.ppt`, `.pptx`, `.doc`, `.docx`, `.pdf`, `.md`, `.txt`.
- Assignment files:
  - Maximum size: 20 MB.
  - Supported extensions: `.pdf`, `.docx`, `.doc`, `.ppt`, `.pptx`, `.zip`, `.rar`, `.txt`, `.md`.
- Student submission files:
  - Maximum size: 20 MB.
  - Supported extensions include `.pdf`, `.docx`, `.doc`, `.zip`, `.rar`, `.txt`, `.cs`, `.cpp`, `.sql`, and `.py`.

Downloads are served through authorized controller actions rather than direct public file URLs. Uploaded files should still be scanned and reviewed as part of a production deployment process.

## Security Notes

- Entity Framework Core is used for database access instead of manually concatenated SQL queries.
- Role and ownership checks restrict teachers to their own classrooms and students to enrolled classrooms.
- Students can access only published lessons and assignments.
- Students cannot modify submissions after they have been graded.
- Submission deadlines are checked using UTC timestamps.
- Grades are constrained to the range 0–10.
- Antiforgery validation is applied globally to MVC POST requests and explicitly on protected POST actions.
- Login successes, failures, and lockouts are recorded in `AuditLogs` with the originating IP address.
- File names are sanitized with `Path.GetFileName`, while files are stored under generated GUID names.

For production deployment, replace LocalDB with a secured SQL Server instance, keep secrets outside source control, configure HTTPS certificates, review role and ownership policies, secure the `App_Data` directory, add malware scanning for uploads, and avoid using the seeded development credentials.

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

# Apply migrations
dotnet ef database update
```
