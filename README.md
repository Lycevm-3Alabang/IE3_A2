```bash
# Install EF Core tools globally (if not already available)
# On Windows, you can install via dotnet tool
dotnet tool install --global dotnet-ef

# Verify installation
 dotnet ef --version
```


## Prerequisites

- .NET 8 SDK (or later) – required to build and run the project.
- Entity Framework Core tools (`dotnet ef`) – installed with the SDK; used for migrations.
- SQLite – the database engine used by the application. No separate installation needed on Windows; the runtime pulls the required native libraries.
- Optional: Visual Studio or VS Code with C# extensions for editing.

## Reminders

- **Connection string** – The application expects a `DefaultConnection` key in `appsettings.json` or an environment variable. Example:

  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=./app.db"
    }
  }
  ```

- **Migrations** – When you add or modify the data model, run:

  ```bash
  dotnet ef migrations add <MigrationName>
  dotnet ef database update
  ```

  to update the database schema.

- **API docs** – Swagger UI is available only in Development mode. For production, consider enabling OpenAPI or use tools like Postman.

- **Unit tests** – No tests are included yet. Consider adding tests for controllers and repository methods.

- **Future work** – MySQL and MongoDB repository stubs exist but are not functional; implement them when you need support for those databases.


## Overview

An **ASP.NET Core Web API** project that provides CRUD endpoints for managing student records, backed by a SQLite database via Entity Framework Core.

## Tech Stack

| Layer           | Technology                          |
|-----------------|-------------------------------------|
| Runtime         | .NET (ASP.NET Core)                 |
| Database        | SQLite                              |
| ORM             | Entity Framework Core               |
| API Docs        | Swagger / OpenAPI                   |

## Architecture

```
├── Controllers/
│   └── StudentsController.cs    → REST API endpoints for Student CRUD
├── Data/
│   ├── Student.cs               → Student entity & Gender enum
│   └── SqliteDbContext.cs       → EF Core DbContext (DbSet<Student>)
├── DTO/Students/
│   ├── CreateStudentRequest.cs  → Input model for POST
│   └── CreateStudentResponse.cs → Output model for POST
├── DTO/Courses/
│   └── CreateCourseRequest.cs  → Input model for POST
├── DTO/Subjects/
│   └── CreateSubjectRequest.cs  → Input model for POST
├── DTO/Enrolments/
│   └── CreateEnrollmentRequest.cs  → Input model for POST
├── Repository/
│   ├── IStudentRepository.cs    → Repository interface
│   ├── StudentRepository.cs     → SQLite implementation (working)
│   ├── StudentMySqlRepo.cs      → MySQL stub (placeholder)
│   └── StudentMongoDbRepository → MongoDB stub (NotImplemented)
├── Migrations/                  → EF Core migrations (initial create)
└── Program.cs                   → App startup, DI, Swagger config
```

## API Endpoints

| Method | Endpoint        | Description       |
|--------|-----------------|--------------------|
| GET    | /api/students   | Get all students  |
| GET    | /api/students/{id} | Get student by ID |
| POST   | /api/students   | Create a new student |
| PUT    | /api/students/{id} | Update an existing student |
| DELETE | /api/students/{id} | Delete a student  |

## Entity Model

**Student**

| Property  | Type       | Description         |
|-----------|------------|---------------------|
| Id        | `int`      | Primary key         |
| Name      | `string?`  | Student name        |
| Email     | `string?`  | Student email       |
| Gender    | `Gender`   | Male / Female / DoNotSpecify |

## Current State

- **Working**: SQLite-backed CRUD for students is fully functional.
- **In Progress**: Repository layer includes stubs for MySQL (`StudentMySqlRepo`) and MongoDB (`StudentMongoDbRepository`) — these are not yet implemented.
- **Migrations**: An initial migration (`20261125103402_initial-create`) exists. Run `dotnet ef database update` to apply it.

## Design Decisions

### Repository Pattern
The repository pattern is an abstraction layer that encapsulates data access logic.  
It defines an **interface** (`IStudentRepository`) that exposes domain‑level operations (e.g. `CreateStudentAsync`) and leaves the concrete data‑store implementation to separate classes.

**Why we use it**

- **Decoupling** – Controllers talk to an interface, not a concrete database context.  
  If we change the underlying database or move to a different storage mechanism, we only replace the repository implementation, not the controller code.  
- **Testability** – In unit tests we can inject a mock implementation of `IStudentRepository` and verify that the controller behaves correctly without touching a real database.  
- **Single Responsibility** – The repository owns all persistence concerns, keeping controllers focused on HTTP semantics.

**Swapping data sources**

| Data source | What changes? | Example code |
|-------------|---------------|--------------|
| SQLite (current) | Keep `StudentRepository` that uses `SqliteDbContext`. | `builder.Services.AddScoped<IStudentRepository, StudentRepository>();` |
| PostgreSQL | Use a PostgreSQL `DbContext` (or keep the same context class but configure Npgsql). | `builder.Services.AddDbContext<SqliteDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));` |
| In‑memory list | Replace `StudentRepository` with an `InMemoryStudentRepository` that stores data in a `List<Student>`. | `builder.Services.AddScoped<IStudentRepository, InMemoryStudentRepository>();` |
| External Web API | Create `WebApiStudentRepository` that uses `HttpClient` to call the API. | `builder.Services.AddScoped<IStudentRepository, WebApiStudentRepository>();` |

Example `InMemoryStudentRepository` skeleton:

```csharp
public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new();
    private int _nextId = 1;

    public Task<CreateStudentResponse> CreateStudentAsync(CreateStudentRequest request)
    {
        var student = new Student
        {
            Id = _nextId++,
            Name = request.StudentName,
            Email = request.StudentEmail,
            Gender = request.StudentGender
        };
        _students.Add(student);
        return Task.FromResult(new CreateStudentResponse { Id = student.Id, Student = student.Name });
    }
}
```

Because the controller never sees the concrete type, we can switch to any data source without touching its code.

*If you'd like a deeper dive* you can find a separate guide in **[RepositoryPattern.md](RepositoryPattern.md)**.

## Repository Pattern – Quick Guide

**TL;DR:** The repository pattern is like a middleman that lets your API talk to the database without knowing the details. Change the database later? Just swap the repository, no code changes in the API.

- **Why it matters**:  
  • Keeps your API (controller) **unaware** of the database.  
  • Makes it super easy to test – just give a fake repo.  
  • Switches storage (SQLite ➜ PostgreSQL ➜ In‑memory ➜ Web API) without touching the controller.

- **How it works**:  
  1️⃣ Controller calls `IStudentRepository`.  
  2️⃣ The concrete repo (e.g. `StudentRepository`) implements the call.  
  3️⃣ Repo talks to `SqliteDbContext` (or any other DbContext).  
  4️⃣ DbContext talks to the real database.

- **Swap it fast**:  
  • Replace `StudentRepository` with `InMemoryStudentRepository` in `Program.cs`.  
  • No code changes elsewhere.

---

## Dependencies

NuGet packages are the package manager for .NET. They provide reusable libraries and tools that can be added to a project. In this project we use:

- **Microsoft.EntityFrameworkCore** – the core ORM library for working with relational data.
- **Microsoft.EntityFrameworkCore.Sqlite** – the SQLite provider that allows EF Core to talk to a SQLite database.
- **Swashbuckle.AspNetCore** – generates Swagger/OpenAPI documentation and UI for the Web API.
- **Microsoft.EntityFrameworkCore.Tools** – command‑line tools for migrations and database operations.

These dependencies are listed in the `.csproj` file and restored via `dotnet restore`.


**TL;DR:** The repository pattern is like a middleman that lets your API talk to the database without knowing the details. Change the database later? Just swap the repository, no code changes in the API.

- **Why it matters**:  
  • Keeps your API (controller) **unaware** of the database.  
  • Makes it super easy to test – just give a fake repo.  
  • Switches storage (SQLite ➜ PostgreSQL ➜ In‑memory ➜ Web API) without touching the controller.

- **How it works**:  
  1️⃣ Controller calls `IStudentRepository`.  
  2️⃣ The concrete repo (e.g. `StudentRepository`) implements the call.  
  3️⃣ Repo talks to `SqliteDbContext` (or any other DbContext).  
  4️⃣ DbContext talks to the real database.

- **Swap it fast**:  
  • Replace `StudentRepository` with `InMemoryStudentRepository` in `Program.cs`.  
  • No code changes elsewhere.

---

## Dependencies

NuGet packages are the package manager for .NET. They provide reusable libraries and tools that can be added to a project. In this project we use:

- **Microsoft.EntityFrameworkCore** – the core ORM library for working with relational data.
- **Microsoft.EntityFrameworkCore.Sqlite** – the SQLite provider that allows EF Core to talk to a SQLite database.
- **Swashbuckle.AspNetCore** – generates Swagger/OpenAPI documentation and UI for the Web API.
- **Microsoft.EntityFrameworkCore.Tools** – command‑line tools for migrations and database operations.

These dependencies are listed in the `.csproj` file and restored via `dotnet restore`.


NuGet packages are the package manager for .NET. They provide reusable libraries and tools that can be added to a project. In this project we use:

- **Microsoft.EntityFrameworkCore** – the core ORM library for working with relational data.
- **Microsoft.EntityFrameworkCore.Sqlite** – the SQLite provider that allows EF Core to talk to a SQLite database.
- **Swashbuckle.AspNetCore** – generates Swagger/OpenAPI documentation and UI for the Web API.
- **Microsoft.EntityFrameworkCore.Tools** – command‑line tools for migrations and database operations.

These dependencies are listed in the `.csproj` file and restored via `dotnet restore`.

## Repository Pattern Diagram

```
+---------------------+
| StudentsController  |<------------+
+---------------------+             |
         ^                         |
         | implements              |
         |                         |
         v                         v
+---------------------+   +---------------------+
| IStudentRepository  |<--| StudentRepository   |
+---------------------+   +---------------------+
                                 |
                                 v
                         +---------------------+
                         | SqliteDbContext     |
                         +---------------------+
                                 |
                                 v
                         +---------------------+
                         | SQLite Database     |
                         +---------------------+
```

**Explanation**

- The controller depends only on the interface `IStudentRepository`.
- `StudentRepository` implements the interface and interacts with `SqliteDbContext`.
- `SqliteDbContext` communicates with the SQLite database.
- When you switch to a different data source, you replace `StudentRepository` with a new implementation (e.g., `InMemoryStudentRepository`, `PostgresRepository`) without touching the controller.

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Apply EF Core migrations (creates the SQLite database)
dotnet ef database update

# Run the app
dotnet run

# Swagger UI opens at: https://localhost:<port>/swagger
```