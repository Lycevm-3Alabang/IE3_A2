# IE3 – P1: Course Feature – Exercise Instructions

This exercise will extend the current School Management web application with a full **Course** feature.  
You will add the following:

* **Course entity** (EF model)  
* **DTOs** for the CRUD API – named using the `[verb][model][request|response]` convention  
  * *CreateCourseRequest*, *CreateCourseResponse*, *UpdateCourseRequest*, *UpdateCourseResponse*, *CourseResponse*  
* **CoursesController** (API controller)  
* **DbContext** registration  

Use the provided **courses.html** as a reference for the data fields.

---

## User Story 1 – Implement the `Course` Entity

> **What is an Entity?**  
> An *Entity* is a C# class that maps to a database table via Entity Framework Core. It represents a persistent domain object that the application stores and retrieves.

> **Entity vs DTO**  
> * **Entity** – Used by EF to read/write data in the database.  
> * **DTO (Data Transfer Object)** – A lightweight object that is sent over the network (API). It typically contains only the data that the client needs and may omit navigation properties or sensitive fields.

### Task 1.1 – Add required properties to `Data/Course.cs`

Open `Data/Course.cs` and add the following properties:

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Data
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Code { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = default!;

        [Required]
        [Range(0.5, 6.0)]
        public decimal Credits { get; set; }
    }
}
```

> **Tip:** The form in *courses.html* uses the IDs `courseCode`, `courseTitle`, `courseSubject`, and `courseCredits`. Mirror these names (or a suitable C# property name) in your `Course` entity.

### Task 1.2 – Verify validation attributes

| Property | Attribute(s) | Reason |
|----------|-------------|--------|
| `Code` | `[Required]`, `[StringLength(10, MinimumLength = 3)]` | Ensures a non‑empty course code (e.g., "CS101"). |
| `Title` | `[Required]`, `[StringLength(200)]` | Ensures the title is present and not excessively long. |
| `Subject` | `[Required]`, `[StringLength(100)]` | Ensures the subject name is present and not excessively long. |
| `Credits` | `[Required]`, `[Range(0.5, 6.0)]` | Ensures credits are within a realistic range (e.g., 3 or 4.5). |

---

## User Story 2 – Create the DTOs for CRUD Operations

> **Why DTOs?**  
> DTOs decouple the API surface from the database schema. They allow you to shape the data returned to the client and validate incoming payloads.

Create a new folder: `DTO/Courses/` and add the following files using the `[verb][model][request|response]` naming convention:

### Task 2.1 – Create `CreateCourseRequest.cs`

**File:** `DTO/Courses/CreateCourseRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.DTO.Courses
{
    public class CreateCourseRequest
    {
        [Required]
        [StringLength(10, MinimumLength = 3)]
        public string Code { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = default!;

        [Required]
        [Range(0.5, 6.0)]
        public decimal Credits { get; set; }
    }
}
```

### Task 2.2 – Create `CreateCourseResponse.cs`

**File:** `DTO/Courses/CreateCourseResponse.cs`

```csharp
namespace IE3_P1.DTO.Courses
{
    public class CreateCourseResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public decimal Credits { get; set; }
    }
}
```

### Task 2.3 – Create `UpdateCourseRequest.cs`

**File:** `DTO/Courses/UpdateCourseRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.DTO.Courses
{
    public class UpdateCourseRequest
    {
        public int Id { get; set; }

        [StringLength(10, MinimumLength = 3)]
        public string? Code { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Subject { get; set; }

        [Range(0.5, 6.0)]
        public decimal? Credits { get; set; }
    }
}
```

> **Why nullable fields in `UpdateCourseRequest`?**  
> It allows partial updates — if a field is not supplied by the client, the repository will leave it unchanged.

### Task 2.4 – Create `UpdateCourseResponse.cs`

**File:** `DTO/Courses/UpdateCourseResponse.cs`

```csharp
namespace IE3_P1.DTO.Courses
{
    public class UpdateCourseResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public decimal Credits { get; set; }
    }
}
```

### Task 2.5 – Create `CourseResponse.cs`

**File:** `DTO/Courses/CourseResponse.cs`

```csharp
namespace IE3_P1.DTO.Courses
{
    public class CourseResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public decimal Credits { get; set; }
    }
}
```

> **Note:** DTOs do not need validation attributes — they are used only for output (response) purposes. Validation is handled by the request DTOs.

---

## User Story 3 – Implement the Repository Layer

> **Notice:** The following design uses a repository interface to keep the controller thin. The repository will expose CRUD operations that accept/return DTOs. This pattern is a step toward a scalable, maintainable architecture.

### Task 3.1 – Create `ICoursesRepository.cs`

**File:** `Repositories/ICoursesRepository.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using IE3_P1.DTO.Courses;

namespace IE3_P1.Repositories
{
    public interface ICoursesRepository
    {
        Task<IEnumerable<CourseResponse>> GetAllAsync();
        Task<CourseResponse?> GetByIdAsync(int id);
        Task<CreateCourseResponse> CreateAsync(CreateCourseRequest dto);
        Task UpdateAsync(int id, UpdateCourseRequest dto);
        Task DeleteAsync(int id);
    }
}
```

### Task 3.2 – Implement `CoursesRepository.cs`

**File:** `Repositories/CoursesRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using IE3_P1.DTO.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IE3_P1.Repositories
{
    public class CoursesRepository : ICoursesRepository
    {
        private readonly SqliteDbContext _context;

        public CoursesRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseResponse>> GetAllAsync()
        {
            return await _context.Courses
                .Select(c => new CourseResponse
                {
                    Id = c.Id,
                    Code = c.Code,
                    Title = c.Title,
                    Subject = c.Subject,
                    Credits = c.Credits
                })
                .ToListAsync();
        }

        public async Task<CourseResponse?> GetByIdAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return null;

            return new CourseResponse
            {
                Id = course.Id,
                Code = course.Code,
                Title = course.Title,
                Subject = course.Subject,
                Credits = course.Credits
            };
        }

        public async Task<CreateCourseResponse> CreateAsync(CreateCourseRequest dto)
        {
            var entity = new Course
            {
                Code = dto.Code!,
                Title = dto.Title!,
                Subject = dto.Subject!,
                Credits = dto.Credits!
            };

            await _context.Courses.AddAsync(entity);
            await _context.SaveChangesAsync();

            return new CreateCourseResponse
            {
                Id = entity.Id,
                Code = entity.Code,
                Title = entity.Title,
                Subject = entity.Subject,
                Credits = entity.Credits
            };
        }

        public async Task UpdateAsync(int id, UpdateCourseRequest dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) throw new KeyNotFoundException($"Course with Id {id} not found.");

            if (dto.Code != null) course.Code = dto.Code;
            if (dto.Title != null) course.Title = dto.Title;
            if (dto.Subject != null) course.Subject = dto.Subject;
            if (dto.Credits.HasValue) course.Credits = dto.Credits.Value;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) throw new KeyNotFoundException($"Course with Id {id} not found.");

            _context.Courses.Remove(course);
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

## User Story 4 – Implement the CoursesController

### Task 4.1 – Create `CoursesController.cs`

**File:** `Controllers/CoursesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using IE3_P1.DTO.Courses;
using IE3_P1.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IE3_P1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesRepository _repository;

        public CoursesController(ICoursesRepository repository)
        {
            _repository = repository;
        }

        // GET: api/courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetCourses()
        {
            var courses = await _repository.GetAllAsync();
            return Ok(courses);
        }

        // POST: api/courses
        [HttpPost]
        public async Task<ActionResult<CreateCourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repository.CreateAsync(request);
            var response = new CreateCourseResponse
            {
                Id = created.Id,
                Code = created.Code,
                Title = created.Title,
                Subject = created.Subject,
                Credits = created.Credits
            };
            return CreatedAtAction(nameof(GetCourseById), new { id = created.Id }, response);
        }

        // GET: api/courses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseResponse>> GetCourseById(int id)
        {
            var course = await _repository.GetByIdAsync(id);
            if (course == null) return NotFound();
            return Ok(course);
        }

        // PUT: api/courses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
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

        // DELETE: api/courses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
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
builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();
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

### Task 6.1 – Add the `Courses` DbSet to `SqliteDbContext.cs`

Open `Data/SqliteDbContext.cs` and add:

```csharp
public DbSet<Course> Courses { get; set; }
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

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }   // <-- new line
    }
}
```

> **Important:** *Do not edit this file automatically.* Students should copy this line and then run the migration commands in Task 6.2.

### Task 6.2 – Run EF Core migrations

After adding the DbSet, run:

```bash
dotnet ef migrations add AddCourseEntity
dotnet ef database update
```

> **Tip:** If you want a specific table name, override it in `OnModelCreating`:
> ```csharp
> modelBuilder.Entity<Course>().ToTable("Courses");
> ```

---

## User Story 7 – Test Against the Hard‑Wired Front‑End (`courses.html`)

> **Critical Rule:** The file `wwwroot/courses.html` is **hard-wired** and **must not be changed**.  
> It is provided as a fixed, non-modifiable front-end that students must test their implementation against.

The front-end expects your API to conform exactly to the contract below. Any mismatch — wrong URL path, missing response fields, or incorrect HTTP method — will cause the UI to fail silently.

### Task 7.1 – Understand the front-end's expected API contract

| Operation | Method | Endpoint | Request Body Fields | Expected Response Field(s) |
|-----------|--------|----------|---------------------|----------------------------|
| **GET all** | `GET` | `/api/courses` | *(none)* | `[ { "id": number, "code": string, "title": string, "subject": string, "credits": number } ]` |
| **GET by ID** | `GET` | `/api/courses/{id}` | *(none)* | `{ "id": number, "code": string, "title": string, "subject": string, "credits": number }` |
| **POST create** | `POST` | `/api/courses` | `{ "code": string, "title": string, "subject": string, "credits": number }` | `{ "id": number, "code": string, "title": string, "subject": string, "credits": number }` (Location header on 201) |
| **PUT update** | `PUT` | `/api/courses/{id}` | `{ "id": number, "code"?: string, "title"?: string, "subject"?: string, "credits"?: number }` | 204 No Content |
| **DELETE** | `DELETE` | `/api/courses/{id}` | *(none)* | 204 No Content |

> **Key constraints imposed by the front-end:**  
> * The form field IDs (`courseCode`, `courseTitle`, `courseSubject`, `courseCredits`) map to JSON keys using **camelCase** (e.g., `code`, `title`, `subject`, `credits`). If your API returns PascalCase properties (`Code`, `Title`), the front-end will not bind them.  
> * The response object **must** include an `id` field — the front-end uses it to populate the hidden `#courseId` field during edit operations.  
> * All requests must be sent with `Content-Type: application/json`.

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
> Without this, ASP.NET Core serializes properties in PascalCase (`Code`, `Title`), which the hard-wired front-end cannot match. This is a common source of "working API but broken UI" bugs.

### Task 7.3 – Verify CORS or static file serving is enabled

Ensure that:
* Static files (including `wwwroot/courses.html`) are served by confirming the following exists in `Program.cs`:

```csharp
app.UseStaticFiles();
```

* This must be placed **before** `app.UseHttpsRedirection()` or `app.MapControllers()`.

### Task 7.4 – Test the implementation end-to-end

1. Start the application (`dotnet run` or F5).  
2. Open `http://localhost:<port>/courses.html` in a browser.  
3. Verify each operation:
   * **Add** — Fill in all four fields and click "Save Course". Confirm the course appears in the table below.  
   * **Edit** — Click a row's edit button (if present), modify fields, and save. Confirm the update persists.  
   * **Delete** — Click delete on a row. Confirm it is removed from the table.  
   * **List** — Reload the page. Confirm all previously saved courses are displayed.  
4. If any operation fails:
   * Open the browser's DevTools **Network** tab. Check the request URL, method, and response status code.  
   * Inspect the raw JSON in the **Response** tab — verify field names match camelCase exactly.

---

## Additional Configurations (if needed)

* Ensure the connection string in `appsettings.json` points to the SQLite database file used by the project.
* Verify that the `IE3_P1.Repositories` namespace is accessible from `Program.cs` (add `using IE3_P1.Repositories;` if necessary).

---

## Hand‑Off Section – Historical Context

### What was already in the repository?

| Item | Location | Status |
|------|----------|--------|
| `Student` entity | `Data/Student.cs` | Present |
| `Students` DbSet | `Data/SqliteDbContext.cs` | Present |
| `courses.html` | `wwwroot/courses.html` | Present (UI layout) |
| No `Course` entity existed. | — | Absent |
| No DTOs for Courses. | — | Absent |

### What will be added by this exercise?

* `Course` entity class with validation attributes.  
* Five DTOs: *CreateCourseRequest*, *CreateCourseResponse*, *UpdateCourseRequest*, *UpdateCourseResponse*, *CourseResponse*.  
* Repository interface (`ICoursesRepository`) and implementation (`CoursesRepository`).  
* `CoursesController` API controller.  
* `DbSet<Course>` in `SqliteDbContext.cs`.  
* DI registration for the repository.

---

## Handoff Summary

This document now contains all the pieces needed to complete the Course feature with a repository‑pattern architecture.  
Each user story is independent and can be completed in order without overlap:

| User Story | File(s) Modified / Created |
|------------|---------------------------|
| **1 – Course Entity** | `Data/Course.cs` |
| **2 – DTOs** | `DTO/Courses/CreateCourseRequest.cs`, `CreateCourseResponse.cs`, `UpdateCourseRequest.cs`, `UpdateCourseResponse.cs`, `CourseResponse.cs` |
| **3 – Repository** | `Repositories/ICoursesRepository.cs`, `Repositories/CoursesRepository.cs` |
| **4 – Controller** | `Controllers/CoursesController.cs` |
| **5 – DI Registration** | `Program.cs` |
| **6 – DbContext & Migrations** | `Data/SqliteDbContext.cs` (add DbSet), then run migration commands |
| **7 – Front-End Testing** | `wwwroot/courses.html` (read-only), `Program.cs` (JSON options, static files) |

### Potential Pitfalls

* Forgetting to register the repository in DI results in a runtime error when the controller tries to inject it.  
* Missing the `Course` DbSet will cause EF migration failures.  
* DTO and entity mismatches can lead to runtime mapping errors.  
* Not using `[FromBody]` on POST/PUT may cause model binding to fail silently.

### Checklist

* [ ] Course entity fields implemented with validation attributes.  
* [ ] All five DTOs created with correct naming convention.  
* [ ] Repository interface and implementation completed.  
* [ ] Repository registered in DI (`Program.cs`).  
* [ ] `DbSet<Course>` added to `SqliteDbContext`.  
* [ ] EF migrations applied.  
* [ ] CRUD endpoints functional (tested via Swagger or Postman).  
* [ ] Front-end `courses.html` tested end-to-end — add, edit, delete, and list all work without modifying the HTML file.

---

**End of Instructions**  
Good luck! Feel free to ask if you need clarification on any step.
