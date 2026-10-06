# Software and Database Security Project

A secure Learning Management System (LMS) built with ASP.NET Core MVC, ASP.NET Core Identity, Entity Framework Core, and Microsoft SQL Server. The application demonstrates role-based access control, secure file uploads, database constraints, audit logging, and protected learning workflows for administrators, teachers, and students.

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
- Automatic antiforgery validation for MVC POST requests to help prevent CSRF attacks.
- HTTPS redirection and HSTS outside the Development environment.
- Login audit logging for successful logins, failed logins, and account lockouts.
- Audit records include the email address, action, IP address, timestamp, and success status.
- Passwords are hashed by ASP.NET Core Identity using its configured password hasher.

### Learning Management Features

- Admins can create classrooms and assign teachers.
- Admins can enroll students in classrooms.
- Database and application checks prevent duplicate enrollments.
- Teachers can view and manage their own classrooms.
- Teachers can create lessons for their classrooms.
- Teachers can publish lessons for enrolled students.
- Lessons can contain text content and optional attachments.
- Teachers can create homework assignments or quizzes.
- Assignments can contain instructions, deadlines, assignment types, and optional files.
- Students can view published lessons and assignments only in classrooms where they are enrolled.
- Students can submit text responses and optional files.
- Students can update a submission while it is not graded and the deadline has not passed.
- Each student can have at most one submission for an assignment.
- Teachers can view submissions for their own classrooms.
- Teachers can grade submissions from 0 to 10 and provide feedback.
- Lesson, assignment, and submission downloads are protected by role and ownership checks.

## Authorization Matrix

| Functionality | Admin | Teacher | Student |
|---|:---:|:---:|:---:|
| Create and manage classrooms | Yes | No | No |
| Assign teachers to classrooms | Yes | No | No |
| Enroll students | Yes | No | No |
| View a classroom | Yes | Yes, if assigned | Yes, if enrolled |
| Create lessons | No | Yes, in assigned classrooms | No |
| Publish lessons | No | Yes, in assigned classrooms | No |
| Create assignments and quizzes | No | Yes, in owned lessons | No |
| View published lessons and assignments | Yes | Yes, in owned classrooms | Yes, if enrolled |
| Submit assignments | No | No | Yes, if enrolled |
| View submissions | No | Yes, for owned classrooms | Own submission only |
| Download submission files | No | Yes, for owned classrooms | Own submission only |
| Grade submissions | No | Yes, for owned classrooms | No |

## Technology Stack

- **Framework:** ASP.NET Core MVC
- **Target runtime:** .NET 10 (`net10.0`)
- **Language:** C#
- **Authentication:** ASP.NET Core Identity with Entity Framework Core
- **ORM:** Entity Framework Core 10.0.12
- **Database provider:** Microsoft.EntityFrameworkCore.SqlServer 10.0.12
- **Database:** Microsoft SQL Server or SQL Server LocalDB
- **Frontend:** Razor Views, Bootstrap, and jQuery Validation

## Data Model

The application contains the following main entities:

- `ApplicationUser`: ASP.NET Core Identity user with an additional `FullName` property.
- `Classroom`: A classroom assigned to a teacher.
- `Enrollment`: The relationship between a classroom and a student.
- `Lesson`: Learning content belonging to a classroom, with publication status and an optional attachment.
- `Assignment`: Homework or quiz belonging to a lesson, with instructions, type, deadline, and an optional file.
- `Submission`: A student's text/file submission, submission timestamp, grade, and feedback.
- `AuditLog`: Records login successes, failures, and lockouts.

### Database Security Constraints

`ApplicationDbContext` configures unique indexes to enforce important rules at the database level:

- An enrollment must be unique for each classroom and student pair.
- A submission must be unique for each assignment and student pair.
- Restrict delete behavior is used for teacher, student, and submission relationships to avoid SQL Server cascade-delete conflicts.

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

To use another SQL Server instance, update `ConnectionStrings:DefaultConnection` in `appsettings.json` or provide it through environment-specific configuration. Do not commit production passwords, connection strings, or other secrets to source control.

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

4. Install the EF Core CLI if it is not already installed:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. Apply the existing migrations when using a database managed through EF Core migrations:

   ```bash
   dotnet ef database update
   ```

6. Run the application:

   ```bash
   dotnet run
   ```

7. Open the HTTPS URL displayed in the terminal. The default route opens the login page at `/Account/Login`.

At startup, `DbInitializer.SeedAsync` calls `EnsureCreatedAsync`, creates the `Admin`, `Teacher`, and `Student` roles, and creates the demonstration users if they do not already exist.

> **Database note:** The application currently uses `EnsureCreatedAsync` during startup as well as checked-in EF Core migrations. For production deployments, choose and consistently use an appropriate database initialization strategy, preferably reviewed EF Core migrations.

## Seeded Development Accounts

The development initializer creates these accounts with the password `SecurePass@123`:

| Role | Email | Full name |
|---|---|---|
| Admin | `admin@lms.com` | System Admin |
| Teacher | `teacher1@lms.com` | Giang Vien 1 |
| Teacher | `teacher2@lms.com` | Giang Vien 2 |
| Student | `student1@lms.com` | Sinh Vien 1 |
| Student | `student2@lms.com` | Sinh Vien 2 |

These credentials are intended only for local testing. Change or remove them before deploying the application to a shared or production environment.

## File Upload Rules

Uploaded files are stored outside the publicly served `wwwroot` directory under `App_Data`. Storage names are generated with GUIDs, while the original file names are retained for download display.

### Lesson Attachments

- Maximum size: 15 MB.
- Storage directory: `App_Data/Uploads`.
- Supported extensions: `.ppt`, `.pptx`, `.doc`, `.docx`, `.pdf`, `.md`, `.txt`.

### Assignment Files

- Maximum size: 20 MB.
- Storage directory: `App_Data/Assignments`.
- Supported extensions: `.pdf`, `.docx`, `.doc`, `.ppt`, `.pptx`, `.zip`, `.rar`, `.txt`, `.md`.

### Student Submission Files

- Maximum size: 20 MB.
- Storage directory: `App_Data/Submissions`.
- Supported extensions: `.pdf`, `.docx`, `.doc`, `.zip`, `.rar`, `.txt`, `.cs`, `.cpp`, `.sql`, `.py`.

File names are sanitized with `Path.GetFileName`. Downloads are served through authorized controller actions rather than direct public file URLs. Uploaded files should still be scanned and reviewed as part of a production deployment process.

## Security Notes

- Entity Framework Core is used for database access instead of manually concatenated SQL queries.
- Role checks and ownership checks restrict teachers to their own classrooms and students to enrolled classrooms.
- Students can access only published lessons and assignments.
- Students cannot modify submissions after they have been graded.
- Submission deadlines are checked using UTC timestamps.
- Grades are clamped to the range 0–10 before saving.
- Antiforgery validation is applied globally to MVC POST requests and explicitly on protected POST actions.
- Login successes, failures, and lockouts are recorded in `AuditLogs` with the originating IP address.
- Uploaded files are stored with generated names outside `wwwroot`.
- SQL Server foreign-key delete behavior is restricted where necessary to prevent cascade-delete conflicts.

For production deployment:

- Replace LocalDB with a secured SQL Server instance.
- Keep passwords and connection strings outside source control.
- Configure valid HTTPS certificates.
- Review role, ownership, and enrollment policies.
- Add malware scanning and content validation for uploaded files.
- Avoid exposing seeded development credentials.
- Review the database initialization strategy before deployment.

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
