# IE3 – P1: Student Feature – Exercise Instructions

> **Mirror notice:** This document mirrors `CourseEndpoint_Instructions.md`, `SubjectEndpoint_Instructions.md`, and `EnrollmentEndpoint_Instructions.md`, adapted to the **Student** feature. All structure, user stories, task numbering, conventions, and rules are kept identical; only the model name (`Course` → `Student`), controller name (`CoursesController` → `StudentsController`), DTO names (`*Course*` → `*Student*`), folder name (`DTO/Courses/` → `DTO/Students/`), route (`/api/courses` → `/api/students`), hard-wired page (`wwwroot/courses.html` → `wwwroot/students.html`), and data fields are changed.
>
> **Order:** **Student is the first feature to be worked on.** Implement `Student` first, then Course, Subject, and finally **Enrolment last** (see `EnrollmentEndpoint_Instructions.md`, which depends on `Student`, `Course`, and `Subject`).

This exercise will extend the current School Management web application with a full **Student** feature.  
You will add the following:

* **Student entity** (EF model)  
* **DTOs** for the CRUD API – named using the `[verb][model][request|response]` convention
  * *CreateStudentRequest*, *CreateStudentResponse*, *UpdateStudentRequest*, *UpdateStudentResponse*, *StudentResponse*  
* **StudentsController** (API controller)  
* **DbContext** registration  

Use the provided **students.html** as a reference for the data fields.

---

## User Story 1 – Implement the `Student` Entity

> **What is an Entity?**  
> An *Entity* is a C# class that maps to a database table via Entity Framework Core. It represents a persistent domain object that the application stores and retrieves.

> **Entity vs DTO**  
> * **Entity** – Used by EF to read/write data in the database.  
> * **DTO (Data Transfer Object)** – A lightweight object that is sent over the network (API). It typically contains only the data that the client needs and may omit navigation properties or sensitive fields.

### Task 1.1 – Add required properties to `Data/Student.cs`

Open `Data/Student.cs` and add the following properties:

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Data
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = default!;

        [Required]
        public Gender Gender { get; set; }

        public int CourseId { get; set; }

        public Course? Course { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        DoNotSpecify
    }
}
```

> **Note:** The current `Data/Student.cs` has `string? Name`, `string? Email`, `Gender`, `int CourseId`, `Course? Course`, and `Gender` already defined. This task tightens validation (`[Required]`/`[EmailAddress]`/`[StringLength]`). **Email validation requirement:** student email must be a valid email address. `CourseId` references an existing `Course`; `Course? Course` is a **navigation property** used by EF, normally **omitted from DTOs**. `Gender` values: `Male` / `Female` / `DoNotSpecify`. **Hard-wired page note:** `wwwroot/students.html` is the reference front-end; its input ids are `studentName`/`studentEmail`/`studentGender`/`courseId` (`#studentId`, `#studentsTable`) and its JSON keys are `name`/`email`/`gender`/`courseId` (see Task 7) for this API.

### Task 1.2 – Verify validation attributes

| Property | Attribute(s) | Reason |
|----------|-------------|--------|
| `Name` | `[Required]`, `[StringLength(200)]` | Ensures the student name is present and not excessively long. |
| `Email` | `[Required]`, `[EmailAddress]`, `[StringLength(200)]` | **Email validation requirement:** ensures a valid email address. |
| `Gender` | `[Required]` | Ensures a gender is selected (`Male`/`Female`/`DoNotSpecify`). |
| `CourseId` | — (FK) | Links to an existing `Course`. |

---

## User Story 2 – Create the DTOs for CRUD Operations

> **Why DTOs?**  
> DTOs decouple the API surface from the database schema. They allow you to shape the data returned to the client and validate incoming payloads.

Create a new folder: `DTO/Students/` and add the following files using the `[verb][model][request|response]` naming convention:

### Task 2.1 – Create `CreateStudentRequest.cs`

**File:** `DTO/Students/CreateStudentRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using IE3_P1.Data;

namespace IE3_P1.DTO.Students
{
    public class CreateStudentRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = default!;

        [Required]
        public Gender Gender { get; set; }

        public int CourseId { get; set; }
    }
}
```

> **Note:** A `CreateStudentRequest.cs` skeleton already exists in `DTO/Students/`; fill in its properties as shown. **Email validation requirement** applies here (`[Required]` + `[EmailAddress]`).

### Task 2.2 – Create `CreateStudentResponse.cs`

**File:** `DTO/Students/CreateStudentResponse.cs`

```csharp
using IE3_P1.Data;

namespace IE3_P1.DTO.Students
{
    public class CreateStudentResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public Gender Gender { get; set; }
        public int CourseId { get; set; }
    }
}
```

### Task 2.3 – Create `UpdateStudentRequest.cs`

**File:** `DTO/Students/UpdateStudentRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using IE3_P1.Data;

namespace IE3_P1.DTO.Students
{
    public class UpdateStudentRequest
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        public Gender? Gender { get; set; }

        public int? CourseId { get; set; }
    }
}
```

> **Why nullable fields in `UpdateStudentRequest`?**  
> It allows partial updates — if a field is not supplied by the client, the repository will leave it unchanged. **Email validation requirement** still applies when `Email` is supplied (`[EmailAddress]`).

### Task 2.4 – Create `UpdateStudentResponse.cs`

**File:** `DTO/Students/UpdateStudentResponse.cs`

```csharp
using IE3_P1.Data;

namespace IE3_P1.DTO.Students
{
    public class UpdateStudentResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public Gender Gender { get; set; }
        public int CourseId { get; set; }
    }
}
```

### Task 2.5 – Create `StudentResponse.cs`

**File:** `DTO/Students/StudentResponse.cs`

```csharp
using IE3_P1.Data;

namespace IE3_P1.DTO.Students
{
    public class StudentResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public Gender Gender { get; set; }
        public int CourseId { get; set; }
    }
}
```

> **Note:** DTOs do not need validation attributes on output — they are used only for output (response) purposes. Validation is handled by the request DTOs. Navigation properties (`Course`) are **not** included.  
> `StudentResponse` can be reused **as a list element** (`GetAllAsync()` returns `IEnumerable<StudentResponse>`, `GET /api/students` returns `[ StudentResponse, … ]`).

---

## User Story 3 – Implement the Repository Layer

> **Notice:** The following design uses a repository interface to keep the controller thin. The repository will expose CRUD operations that accept/return DTOs. This pattern is a step toward a scalable, maintainable architecture.

### Task 3.1 – Create `IStudentsRepository.cs`

**File:** `Repositories/IStudentsRepository.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using IE3_P1.DTO.Students;

namespace IE3_P1.Repositories
{
    public interface IStudentsRepository
    {
        Task<IEnumerable<StudentResponse>> GetAllAsync();
        Task<StudentResponse?> GetByIdAsync(int id);
        Task<CreateStudentResponse> CreateAsync(CreateStudentRequest dto);
        Task UpdateAsync(int id, UpdateStudentRequest dto);
        Task DeleteAsync(int id);
    }
}
```

### Task 3.2 – Implement `StudentsRepository.cs`

**File:** `Repositories/StudentsRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using IE3_P1.DTO.Students;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IE3_P1.Repositories
{
    public class StudentsRepository : IStudentsRepository
    {
        private readonly SqliteDbContext _context;

        public StudentsRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentResponse>> GetAllAsync()
        {
            return await _context.Students
                .Select(s => new StudentResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email,
                    Gender = s.Gender,
                    CourseId = s.CourseId
                })
                .ToListAsync();
        }

        public async Task<StudentResponse?> GetByIdAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return null;

            return new StudentResponse
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Gender = student.Gender,
                CourseId = student.CourseId
            };
        }

        public async Task<CreateStudentResponse> CreateAsync(CreateStudentRequest dto)
        {
            var entity = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Gender = dto.Gender,
                CourseId = dto.CourseId
            };

            await _context.Students.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new CreateStudentResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Gender = entity.Gender,
                CourseId = entity.CourseId
            };
        }

        public async Task UpdateAsync(int id, UpdateStudentRequest dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) throw new KeyNotFoundException($"Student with Id {id} not found.");

            if (dto.Name != null) student.Name = dto.Name;
            if (dto.Email != null) student.Email = dto.Email;
            if (dto.Gender.HasValue) student.Gender = dto.Gender.Value;
            if (dto.CourseId.HasValue) student.CourseId = dto.CourseId.Value;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) throw new KeyNotFoundException($"Student with Id {id} not found.");

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}
```

> **Return conventions**  
> * `GetByIdAsync` returns `null` when the entity does not exist — the controller should translate this into a 404.  
> * `CreateAsync` returns the newly created DTO, including its `Id`.  
> * `UpdateAsync` and `DeleteAsync` throw `KeyNotFoundException` if the entity is missing — the controller catches this and converts it to a 404.

---

## User Story 4 – Implement the StudentsController

### Task 4.1 – Create `StudentsController.cs`

**File:** `Controllers/StudentsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using IE3_P1.DTO.Students;
using IE3_P1.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IE3_P1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsRepository _repository;

        public StudentsController(IStudentsRepository repository)
        {
            _repository = repository;
        }

        // GET: api/students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentResponse>>> GetStudents()
        {
            var students = await _repository.GetAllAsync();
            return Ok(students);
        }

        // POST: api/students
        [HttpPost]
        public async Task<ActionResult<CreateStudentResponse>> CreateStudent([FromBody] CreateStudentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repository.CreateAsync(request);
            return CreatedAtAction(nameof(GetStudentById), new { id = created.Id }, created);
        }

        // GET: api/students/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentResponse>> GetStudentById(int id)
        {
            var student = await _repository.GetByIdAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        // PUT: api/students/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _repository.UpdateAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/students/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
```

> **Key points:**  
> * `[FromBody]` is used explicitly on POST and PUT to avoid accidental query‑string binding.  
> * `ModelState.IsValid` validates DTO constraints before hitting the repository.  
> * `KeyNotFoundException` from the repository is caught and translated to a 404 response.

---

## User Story 5 – Register Dependencies in DI

### Task 5.1 – Register the repository in `Program.cs`

Add the following line **after** `builder.Services.AddOpenApi();`:

```csharp
builder.Services.AddScoped<IStudentsRepository, StudentsRepository>();
```

> **Why `Scoped`?**  
> The repository depends on a `DbContext`. `Scoped` aligns with the typical EF Core lifetime within an HTTP request.

### Task 5.2 – Verify `SqliteDbContext` registration

Ensure `Program.cs` already contains:

```csharp
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## User Story 6 – Add DbSet to DbContext & Run Migrations

### Task 6.1 – Add the `Students` DbSet to `SqliteDbContext.cs`

Open `Data/SqliteDbContext.cs` and add:

```csharp
public DbSet<Student> Students { get; set; }
```

The file should now look like:

```csharp
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;

namespace IE3_P1.Data
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }   // <-- new line
        public DbSet<Course> Courses { get; set; }
    }
}
```

> **Important:** *Do not edit this file automatically.* Students should copy this line and then run the migration commands in Task 6.2.

### Task 6.2 – Run EF Core migrations

After adding the DbSet, run:

```bash
dotnet ef migrations add AddStudentEntity
dotnet ef database update
```

> **Tip:** If you want a specific table name, override it in `OnModelCreating`:
> ```csharp
> modelBuilder.Entity<Student>().ToTable("Students");
> ```

---

## User Story 7 – Test Against the Hard‑Wired Front‑End (`students.html`)

> **Critical Rule:** Once `wwwroot/students.html` is **hard-wired**, it is **hard-wired** and **must not be changed**.  
> It is a fixed, non-modifiable front-end that students must test their implementation against.

### Task 7.1 – Understand the front-end's expected API contract

| Operation | Method | Endpoint | Request Body Fields | Expected Response Field(s) |
|-----------|--------|----------|---------------------|----------------------------|
| **GET all** | `GET` | `/api/students` | *(none)* | `[ { "id": number, "name": string, "email": string, "gender": string, "courseId": number } ]` |
| **GET by ID** | `GET` | `/api/students/{id}` | *(none)* | `{ "id": number, "name": string, "email": string, "gender": string, "courseId": number }` |
| **POST create** | `POST` | `/api/students` | `{ "name": string, "email": string, "gender": string, "courseId": number }` | `{ "id": number, "name": string, "email": string, "gender": string, "courseId": number }` (Location header on 201) |
| **PUT update** | `PUT` | `/api/students/{id}` | `{ "id": number, "name"?: string, "email"?: string, "gender"?: string, "courseId"?: number }` | 204 No Content |
| **DELETE** | `DELETE` | `/api/students/{id}` | *(none)* | 204 No Content |

> **Key constraints imposed by the front-end:**  
> * JSON keys use **camelCase** (e.g. `name`, `email`, `gender`, `courseId`). PascalCase (`Name`, `Email`) will not bind.  
> * The response object **must** include an `id` field — used to populate the hidden `#studentId` field during edit operations.  
> * All requests must be sent with `Content-Type: application/json`.  
> * **Email validation requirement:** `email` is required and must be a valid email address (`[Required]`/`[EmailAddress]`, `type="email"`).  
> * `gender` is `Male` / `Female` / `DoNotSpecify`.  
> * **Field-name note:** `wwwroot/students.html` is hard-wired self-contained to this API with `studentName`/`studentEmail`/`studentGender`/`courseId` inputs and `name`/`email`/`gender`/`courseId` JSON keys (`#studentId`, `saveStudent()`, `resetForm()`, `#studentsTable`); API base `http://localhost:8000/api/students`. (It no longer uses the courses-only `api.js`.)

### Task 7.2 – Ensure JSON serialization uses camelCase

Open `Program.cs` and modify the controller configuration to ensure camelCase property names:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
```

> **Why?**  
> Without this, ASP.NET Core serializes properties in PascalCase (`Name`, `Email`), which the hard-wired front-end cannot match. (ASP.NET Core's default JSON settings already use camelCase; this makes it explicit.)

### Task 7.3 – Verify CORS or static file serving is enabled

Ensure that:
* Static files (including `wwwroot/students.html`) are served by confirming the following exists in `Program.cs`:

```csharp
app.UseStaticFiles();

> **API base URL & port 8000:** The hard-wired page hardcodes **`http://localhost:8000/api/<endpoint>`**; it does **not** use a relative `/api/...`. **Set `applicationUrl` to `http://localhost:8000` in `WebApplication2/Properties/launchSettings.json`** (profile `http`; `dotnet run --launch-profile http`, or `dotnet run --urls http://localhost:8000`; avoid `https://localhost:...`/other ports so `app.UseHttpsRedirection()` doesn't redirect). Once the app runs on **8000** and your endpoints are created, **nothing needs to be changed on the HTML page.**
```

* This must be placed **before** `app.UseHttpsRedirection()` or `app.MapControllers()`.

### Task 7.4 – Test the implementation end-to-end

1. Start the application (`dotnet run` or F5).  
2. Open `http://localhost:<port>/students.html` in a browser.  
3. Verify each operation:
   * **Add** — Fill in Name, a valid Email, Gender, and Course, click "Save Student". Confirm the student appears in the table below.  
   * **Edit** — Click a row's edit button, modify fields, and save. Confirm the update persists.  
   * **Delete** — Click delete on a row. Confirm it is removed from the table.  
   * **List** — Reload the page. Confirm all previously saved students are displayed.  
4. If any operation fails:
   * Open the browser's DevTools **Network** tab. Check the request URL, method, and response status code.  
   * Inspect the raw JSON in the **Response** tab — verify field names match camelCase exactly.

---

## Additional Configurations (if needed)

* Ensure the connection string in `appsettings.json` points to the SQLite database file used by the project.
* Verify that the `IE3_P1.Repositories` namespace is accessible from `Program.cs` (add `using IE3_P1.Repositories;` if necessary).

---

## Potential Pitfalls

* Forgetting to register the repository in DI results in a runtime error when the controller tries to inject it.  
* Missing the `Student` DbSet will cause EF migration failures.  
* DTO and entity mismatches can lead to runtime mapping errors.  
* Not using `[FromBody]` on POST/PUT may cause model binding to fail silently.  
* **Email validation** not applied (`[EmailAddress]`, `type="email"`) lets invalid emails through.  
* Hard-wired `students.html` field names/ids not matching `name`/`email`/`gender`/`courseId` (and `#studentId`, `#studentsTable`) breaks the UI even when the API works.

## Checklist

* [ ] Student entity fields implemented with validation attributes (`Name`, `Email`, `Gender`, `CourseId`; `Gender` enum).  
* [ ] **Email validation requirement:** `Email` `[Required]` + `[EmailAddress]`; `CreateStudentRequest.Email` validated.  
* [ ] All five DTOs created with correct naming convention.  
* [ ] Repository interface and implementation completed.  
* [ ] Repository registered in DI (`Program.cs`).  
* [ ] `DbSet<Student>` added to `SqliteDbContext`.  
* [ ] EF migrations applied.  
* [ ] CRUD endpoints functional (tested via Swagger or Postman).  
* [ ] Front-end `students.html` tested end-to-end — add, edit, delete, and list all work without modifying the HTML file.

---

**End of Instructions**  
Good luck! Feel free to ask if you need clarification on any step.
