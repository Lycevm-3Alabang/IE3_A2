# IE3 – P1: Subject Feature – Exercise Instructions

> **Mirror notice:** This document is a mirror of `CourseEndpoint_Instructions.md`, adapted from the **Course** feature to the **Subject** feature. All structure, user stories, task numbering, conventions, and rules are kept identical; only the model name (`Course` → `Subject`), controller name (`CoursesController` → `SubjectsController`), DTO names (`*Course*` → `*Subject*`), folder names (`DTO/Courses/` → `DTO/Subjects/`), route (`/api/courses` → `/api/subjects`), hard-wired page (`wwwroot/courses.html` → `wwwroot/subjects.html`), and data fields are changed.

This exercise will extend the current School Management web application with a full **Subject** feature.  
You will add the following:

* **Subject entity** (EF model)  
* **DTOs** for the CRUD API – named using the `[verb][model][request|response]` convention  
  * *CreateSubjectRequest*, *CreateSubjectResponse*, *UpdateSubjectRequest*, *UpdateSubjectResponse*, *SubjectResponse*  
* **SubjectsController** (API controller)  
* **DbContext** registration  

Use the provided **subjects.html** as a reference for the data fields.

---

## User Story 1 – Implement the `Subject` Entity

> **What is an Entity?**  
> An *Entity* is a C# class that maps to a database table via Entity Framework Core. It represents a persistent domain object that the application stores and retrieves.

> **Entity vs DTO**  
> * **Entity** – Used by EF to read/write data in the database.  
> * **DTO (Data Transfer Object)** – A lightweight object that is sent over the network (API). It typically contains only the data that the client needs and may omit navigation properties or sensitive fields.

### Task 1.1 – Add required properties to `Data/Subject.cs`

Open `Data/Subject.cs` and add the following properties:

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Data
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Code { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
```

> **Tip:** The form in *subjects.html* uses the IDs `subjectCode`, `subjectName`, and `subjectDesc`. Mirror these names (or a suitable C# property name) in your `Subject` entity.
>
> **Note:** The `StringLength` values above are suggested starting values (mirroring the Course feature); adjust them to match your project's requirements. `Description` is optional in *subjects.html*, so it is nullable and has no `[Required]`.

### Task 1.2 – Verify validation attributes

| Property | Attribute(s) | Reason |
|----------|-------------|--------|
| `Code` | `[Required]`, `[StringLength(10, MinimumLength = 3)]` | Ensures a non‑empty subject code (e.g., "CS101"). |
| `Name` | `[Required]`, `[StringLength(200)]` | Ensures the name is present and not excessively long. |
| `Description` | `[StringLength(500)]` | Optional; ensures the description is not excessively long. |

---

## User Story 2 – Create the DTOs for CRUD Operations

> **Why DTOs?**  
> DTOs decouple the API surface from the database schema. They allow you to shape the data returned to the client and validate incoming payloads.

Create a new folder: `DTO/Subjects/` and add the following files using the `[verb][model][request|response]` naming convention:

### Task 2.1 – Create `CreateSubjectRequest.cs`

**File:** `DTO/Subjects/CreateSubjectRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.DTO.Subjects
{
    public class CreateSubjectRequest
    {
        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Code { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
```

> **Note:** A `CreateSubjectRequest.cs` skeleton already exists in `DTO/Subjects/`; fill in its properties as shown.

### Task 2.2 – Create `CreateSubjectResponse.cs`

**File:** `DTO/Subjects/CreateSubjectResponse.cs`

```csharp
namespace IE3_P1.DTO.Subjects
{
    public class CreateSubjectResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}
```

### Task 2.3 – Create `UpdateSubjectRequest.cs`

**File:** `DTO/Subjects/UpdateSubjectRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.DTO.Subjects
{
    public class UpdateSubjectRequest
    {
        public int Id { get; set; }

        [StringLength(10, MinimumLength = 3)]
        public string? Code { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}
```

> **Why nullable fields in `UpdateSubjectRequest`?**  
> It allows partial updates — if a field is not supplied by the client, the repository will leave it unchanged.

### Task 2.4 – Create `UpdateSubjectResponse.cs`

**File:** `DTO/Subjects/UpdateSubjectResponse.cs`

```csharp
namespace IE3_P1.DTO.Subjects
{
    public class UpdateSubjectResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}
```

### Task 2.5 – Create `SubjectResponse.cs`

**File:** `DTO/Subjects/SubjectResponse.cs`

```csharp
namespace IE3_P1.DTO.Subjects
{
    public class SubjectResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}
```

> **Note:** DTOs do not need validation attributes — they are used only for output (response) purposes. Validation is handled by the request DTOs.

---

## User Story 3 – Implement the Repository Layer

> **Notice:** The following design uses a repository interface to keep the controller thin. The repository will expose CRUD operations that accept/return DTOs. This pattern is a step toward a scalable, maintainable architecture.

### Task 3.1 – Create `ISubjectsRepository.cs`

**File:** `Repositories/ISubjectsRepository.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using IE3_P1.DTO.Subjects;

namespace IE3_P1.Repositories
{
    public interface ISubjectsRepository
    {
        Task<IEnumerable<SubjectResponse>> GetAllAsync();
        Task<SubjectResponse?> GetByIdAsync(int id);
        Task<CreateSubjectResponse> CreateAsync(CreateSubjectRequest dto);
        Task UpdateAsync(int id, UpdateSubjectRequest dto);
        Task DeleteAsync(int id);
    }
}
```

### Task 3.2 – Implement `SubjectsRepository.cs`

**File:** `Repositories/SubjectsRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using IE3_P1.DTO.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IE3_P1.Repositories
{
    public class SubjectsRepository : ISubjectsRepository
    {
        private readonly SqliteDbContext _context;

        public SubjectsRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubjectResponse>> GetAllAsync()
        {
            return await _context.Subjects
                .Select(s => new SubjectResponse
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Description = s.Description
                })
                .ToListAsync();
        }

        public async Task<SubjectResponse?> GetByIdAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return null;

            return new SubjectResponse
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Description = subject.Description
            };
        }

        public async Task<CreateSubjectResponse> CreateAsync(CreateSubjectRequest dto)
        {
            var entity = new Subject
            {
                Code = dto.Code!,
                Name = dto.Name!,
                Description = dto.Description
            };

            await _context.Subjects.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new CreateSubjectResponse
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description
            };
        }

        public async Task UpdateAsync(int id, UpdateSubjectRequest dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) throw new KeyNotFoundException($"Subject with Id {id} not found.");

            if (dto.Code != null) subject.Code = dto.Code;
            if (dto.Name != null) subject.Name = dto.Name;
            if (dto.Description != null) subject.Description = dto.Description;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) throw new KeyNotFoundException($"Subject with Id {id} not found.");

            _context.Subjects.Remove(subject);
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

## User Story 4 – Implement the SubjectsController

### Task 4.1 – Create `SubjectsController.cs`

**File:** `Controllers/SubjectsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using IE3_P1.DTO.Subjects;
using IE3_P1.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IE3_P1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectsRepository _repository;

        public SubjectsController(ISubjectsRepository repository)
        {
            _repository = repository;
        }

        // GET: api/subjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectResponse>>> GetSubjects()
        {
            var subjects = await _repository.GetAllAsync();
            return Ok(subjects);
        }

        // POST: api/subjects
        [HttpPost]
        public async Task<ActionResult<CreateSubjectResponse>> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repository.CreateAsync(request);
            var response = new CreateSubjectResponse
            {
                Id = created.Id,
                Code = created.Code,
                Name = created.Name,
                Description = created.Description
            };
            return CreatedAtAction(nameof(GetSubjectById), new { id = created.Id }, response);
        }

        // GET: api/subjects/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectResponse>> GetSubjectById(int id)
        {
            var subject = await _repository.GetByIdAsync(id);
            if (subject == null) return NotFound();
            return Ok(subject);
        }

        // PUT: api/subjects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequest request)
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

        // DELETE: api/subjects/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
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
builder.Services.AddScoped<ISubjectsRepository, SubjectsRepository>();
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

### Task 6.1 – Add the `Subjects` DbSet to `SqliteDbContext.cs`

Open `Data/SqliteDbContext.cs` and add:

```csharp
public DbSet<Subject> Subjects { get; set; }
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

        public DbSet<Course> Courses { get; set; }
        public DbSet<Subject> Subjects { get; set; }   // <-- new line
    }
}
```

> **Important:** *Do not edit this file automatically.* Students should copy this line and then run the migration commands in Task 6.2.

### Task 6.2 – Run EF Core migrations

After adding the DbSet, run:

```bash
dotnet ef migrations add AddSubjectEntity
dotnet ef database update
```

> **Tip:** If you want a specific table name, override it in `OnModelCreating`:
> ```csharp
> modelBuilder.Entity<Subject>().ToTable("Subjects");
> ```

---

## User Story 7 – Test Against the Hard‑Wired Front‑End (`subjects.html`)

> **Critical Rule:** The file `wwwroot/subjects.html` is **hard-wired** and **must not be changed**.  
> It is provided as a fixed, non-modifiable front-end that students must test their implementation against.  
> (It ships with its page‑specific JavaScript hard-wired inline; no separate page script file is required.)

### Task 7.1 – Understand the front-end's expected API contract

| Operation | Method | Endpoint | Request Body Fields | Expected Response Field(s) |
|-----------|--------|----------|---------------------|----------------------------|
| **GET all** | `GET` | `/api/subjects` | *(none)* | `[ { "id": number, "code": string, "name": string, "description": string } ]` |
| **GET by ID** | `GET` | `/api/subjects/{id}` | *(none)* | `{ "id": number, "code": string, "name": string, "description": string }` |
| **POST create** | `POST` | `/api/subjects` | `{ "code": string, "name": string, "description"?: string }` | `{ "id": number, "code": string, "name": string, "description": string }` (Location header on 201) |
| **PUT update** | `PUT` | `/api/subjects/{id}` | `{ "id": number, "code"?: string, "name"?: string, "description"?: string }` | 204 No Content |
| **DELETE** | `DELETE` | `/api/subjects/{id}` | *(none)* | 204 No Content |

> **Key constraints imposed by the front-end:**  
> * The form field IDs (`subjectCode`, `subjectName`, `subjectDesc`) map to JSON keys using **camelCase** (e.g., `code`, `name`, `description`). If your API returns PascalCase properties (`Code`, `Name`), the front-end will not bind them.  
> * The response object **must** include an `id` field — the front-end uses it to populate the hidden `#subjectId` field during edit operations.  
> * All requests must be sent with `Content-Type: application/json`.  
> * The page invokes `saveSubject()` on **Save Subject** and `resetForm()` on **Cancel**; `description` is optional.

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
> Without this, ASP.NET Core serializes properties in PascalCase (`Code`, `Name`), which the hard-wired front-end cannot match. This is a common source of "working API but broken UI" bugs.  
> (ASP.NET Core's default JSON settings already use camelCase; this makes it explicit.)

### Task 7.3 – Verify CORS or static file serving is enabled

Ensure that:
* Static files (including `wwwroot/subjects.html`) are served by confirming the following exists in `Program.cs`:

```csharp
app.UseStaticFiles();

> **API base URL & port 8000:** The hard-wired page hardcodes **`http://localhost:8000/api/<endpoint>`**; it does **not** use a relative `/api/...`. **Set `applicationUrl` to `http://localhost:8000` in `WebApplication2/Properties/launchSettings.json`** (profile `http`; `dotnet run --launch-profile http`, or `dotnet run --urls http://localhost:8000`; avoid `https://localhost:...`/other ports so `app.UseHttpsRedirection()` doesn't redirect). Once the app runs on **8000** and your endpoints are created, **nothing needs to be changed on the HTML page.**
```

* This must be placed **before** `app.UseHttpsRedirection()` or `app.MapControllers()`.

### Task 7.4 – Test the implementation end-to-end

1. Start the application (`dotnet run` or F5).  
2. Open `http://localhost:<port>/subjects.html` in a browser.  
3. Verify each operation:
   * **Add** — Fill in code and name (and optional description) and click "Save Subject". Confirm the subject appears in the table below.  
   * **Edit** — Click a row's edit button, modify fields, and save. Confirm the update persists.  
   * **Delete** — Click delete on a row. Confirm it is removed from the table.  
   * **List** — Reload the page. Confirm all previously saved subjects are displayed.  
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
* Missing the `Subject` DbSet will cause EF migration failures.  
* DTO and entity mismatches can lead to runtime mapping errors.  
* Not using `[FromBody]` on POST/PUT may cause model binding to fail silently.

## Checklist

* [ ] Subject entity fields implemented with validation attributes.  
* [ ] All five DTOs created with correct naming convention.  
* [ ] Repository interface and implementation completed.  
* [ ] Repository registered in DI (`Program.cs`).  
* [ ] `DbSet<Subject>` added to `SqliteDbContext`.  
* [ ] EF migrations applied.  
* [ ] CRUD endpoints functional (tested via Swagger or Postman).  
* [ ] Front-end `subjects.html` tested end-to-end — add, edit, delete, and list all work without modifying the HTML file.

---

**End of Instructions**  
Good luck! Feel free to ask if you need clarification on any step.
