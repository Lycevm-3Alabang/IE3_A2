# WebApplication2 (IE3_P1)

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