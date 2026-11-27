# IE3 – P1: Course Feature – Exercise Instructions

This exercise will extend the current School Management web application with a full *Course* feature. You will add the following:

1. **Course entity** (EF model)
2. **DTOs** for the CRUD API
3. **CoursesController** (API controller)
4. **DbContext** registration

Use the provided **courses.html** as a reference for the data fields.

---

## 1. Course Entity

> **What is an Entity?**
> An *Entity* is a C# class that maps to a database table via Entity Framework Core. It represents a persistent domain object that the application stores and retrieves.
>
> **Entity vs DTO**
> - **Entity** – Used by EF to read/write data in the database.
> - **DTO (Data Transfer Object)** – A lightweight object that is sent over the network (API). It typically contains only the data that the client needs and may omit navigation properties or sensitive fields.

**Create `Course.cs` in the `Entities` folder (or wherever your existing entities live):**

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Entities
{
    public class Course
    {
        // Primary Key – already present in the skeleton.
        [Key]
        public int Id { get; set; }

        // -- Hint: the following properties correspond to the form fields in courses.html
        // Course Code   (e.g. "CS101")
        // Course Title  (e.g. "Introduction to Computer Science")
        // Subject       (e.g. "Computer Science")
        // Credits       (e.g. 3 or 4.5)

        // TODO: Add the remaining properties here.
    }
}
```

> **Tip:** The form in *courses.html* uses the IDs `courseCode`, `courseTitle`, `courseSubject`, and `courseCredits`. Mirror these names (or a suitable C# property name) in your `Course` entity.

---

## 2. DTOs for CRUD Operations

> **Why DTOs?**
> DTOs decouple the API surface from the database schema. They allow you to shape the data returned to the client and validate incoming payloads.

Create a new folder: `DTO/Courses/` and add the following files:

| File | Purpose | Note |
|------|---------|------|
| `CreateCourseDTO.cs` | Payload for **POST** (create) | Must include properties needed to create a Course. |
| `UpdateCourseDTO.cs` | Payload for **PUT** (update) | Includes the `Id` plus updatable fields. |
| `CourseDTO.cs` | Payload for **GET** | Represents a Course returned to the client. |

> **Sample skeletons** – *no fields added, just placeholders.*

```csharp
// DTO/Courses/CreateCourseDTO.cs
namespace IE3_P1.DTO.Courses
{
    public class CreateCourseDTO
    {
        // TODO: Add properties that a client submits to create a Course.
    }
}
```

```csharp
// DTO/Courses/UpdateCourseDTO.cs
namespace IE3_P1.DTO.Courses
{
    public class UpdateCourseDTO
    {
        // TODO: Add Id and properties that can be updated.
    }
}
```

```csharp
// DTO/Courses/CourseDTO.cs
namespace IE3_P1.DTO.Courses
{
    public class CourseDTO
    {
        // TODO: Add properties that should be sent back to the client.
    }
}
```

---

## 3. CoursesController

> **Notice:** The following design uses a repository interface to keep the controller thin. The repository will expose CRUD operations that accept/return DTOs. This pattern is a step toward a scalable, maintainable architecture.

Create `ICoursesRepository.cs` in the `Repositories` folder:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using IE3_P1.DTO.Courses;

namespace IE3_P1.Repositories
{
    public interface ICoursesRepository
    {
        Task<IEnumerable<CourseDTO>> GetAllAsync();
        Task<CourseDTO> GetByIdAsync(int id);
        Task<CourseDTO> CreateAsync(CreateCourseDTO dto);
        Task UpdateAsync(int id, UpdateCourseDTO dto);
        Task DeleteAsync(int id);
    }
}
```

> **How to use:**
> - The controller will receive an instance of `ICoursesRepository` via dependency injection.
> - Each action delegates the logic to the repository, which handles mapping to/from the EF `Course` entity and interacts with the `SqliteDbContext`.
> - The repository keeps the controller free from data‑access concerns, ensuring a clean separation of responsibilities.
>
> **Why this matters:**
> - Avoids **fat controllers** that mix routing logic with data manipulation.
> - Enables **unit testing** of the controller by mocking the repository.
> - Allows **swapping** data storage (e.g., moving from SQLite to another database) without changing controller code.
>
> **Repository implementation skeleton (Optional):**
>
```csharp
// Repositories/CoursesRepository.cs
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using IE3_P1.DTO.Courses;
using IE3_P1.Entities;

namespace IE3_P1.Repositories
{
    public class CoursesRepository : ICoursesRepository
    {
        private readonly SqliteDbContext _context;

        public CoursesRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllAsync()
        {
            // TODO: map entities to CourseDTOs
            return Enumerable.Empty<CourseDTO>();
        }

        public async Task<CourseDTO> GetByIdAsync(int id)
        {
            // TODO: retrieve by id and map
            return null;
        }

        public async Task<CourseDTO> CreateAsync(CreateCourseDTO dto)
        {
            // TODO: map dto to entity, add, save, map back
            return null;
        }

        public async Task UpdateAsync(int id, UpdateCourseDTO dto)
        {
            // TODO: retrieve, map, update, save
        }

        public async Task DeleteAsync(int id)
        {
            // TODO: retrieve, delete, save
        }
    }
}
```

> **Updating the controller to use the repository**:
>
```csharp
// Update the constructor injection
private readonly ICoursesRepository _repository;

public CoursesController(ICoursesRepository repository)
{
    _repository = repository;
}

// Example action using the repository
[HttpGet]
public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
{
    var courses = await _repository.GetAllAsync();
    return Ok(courses);
}
```

> **Register the repository in the DI container** (e.g., in `Program.cs`):
>
```csharp
builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();
```

> By following this pattern students learn how to keep controllers lightweight and how to structure a clean, testable architecture using DTOs and the DbContext within a repository.

Create `CoursesController.cs` in the `Controllers` folder:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using IE3_P1.Entities;
using IE3_P1.DTO.Courses;

namespace IE3_P1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly SqliteDbContext _context;

        public CoursesController(SqliteDbContext context)
        {
            _context = context;
        }

        // GET: api/courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            // TODO: Map Course entities to CourseDTOs
            // Example: return await _context.Courses.Select(c => new CourseDTO { ... }).ToListAsync();
            return Ok();
        }

        // POST: api/courses
        [HttpPost]
        public async Task<ActionResult<CourseDTO>> CreateCourse(CreateCourseDTO dto)
        {
            // TODO: Validate dto, map to Course entity, save, and return CourseDTO
            return CreatedAtAction(nameof(GetCourseById), new { id = 0 }, null);
        }

        // GET: api/courses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDTO>> GetCourseById(int id)
        {
            // TODO: Retrieve and map Course by Id
            return NotFound();
        }

        // PUT: api/courses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDTO dto)
        {
            // TODO: Validate, update entity, save changes
            return NoContent();
        }

        // DELETE: api/courses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            // TODO: Find and delete entity, save changes
            return NoContent();
        }
    }
}
```

> **Next steps for students**
> 1. Fill in the DTO properties based on the HTML form fields.
> 2. Implement mapping between DTOs and the `Course` entity.
> 3. Add the controller actions’ logic.
> 4. Test the API using Postman or the built‑in Swagger UI.

---

## 4. Add Course DbSet to SqliteDbContext

> The `SqliteDbContext` already contains a `Students` DbSet.
> Add a new DbSet for the `Course` entity.

**Sample code to add to `Data/SqliteDbContext.cs`:**

```csharp
public DbSet<Course> Courses { get; set; }
```

> **Important:**
> *Do not edit the file automatically.*
> Students should copy this line into the file and run `dotnet ef migrations add AddCoursesDbSet` followed by `dotnet ef database update`.

---

## 5. Additional Configurations (if needed)

- Register `SqliteDbContext` in `Program.cs` or `Startup.cs` if not already done.
- Ensure the connection string points to the SQLite database used by the project.
- Run migrations to apply the new `Courses` table.

---

## Hand‑Off Section – Historical Context

### What was already in the repository?

| Item | Location | Status |
|------|----------|--------|
| `Student` entity | `Entities/Student.cs` | Present |
| `Students` DbSet | `Data/SqliteDbContext.cs` | Present |
| `courses.html` | `wwwroot/courses.html` | Present (UI layout) |
| No `Course` entity or controller exists. |
| No DTOs for Courses. |

### What will be added by this exercise?

- `Course` entity class.
- `CourseDTO`, `CreateCourseDTO`, `UpdateCourseDTO`.
- `CoursesController` API controller.
- `DbSet<Course>` in `SqliteDbContext.cs`.

---

## Handoff Summary

This document now contains all the pieces needed to complete the Course feature with a repository‑pattern architecture. The following steps should be taken by the incoming developer or student:

1. **Add the Course entity**  
   * File: `Entities/Course.cs`  
   * Add the properties that map to the form fields: `Code`, `Title`, `Subject`, `Credits`.

2. **Implement DTOs**  
   * Files: `DTO/Courses/CreateCourseDTO.cs`, `DTO/Courses/UpdateCourseDTO.cs`, `DTO/Courses/CourseDTO.cs`  
   * Populate each DTO with the appropriate fields.  

3. **Create the repository**  
   * Interface: `Repositories/ICoursesRepository.cs` (already added)  
   * Implementation: `Repositories/CoursesRepository.cs` (skeleton provided).  
   * Implement mapping logic between DTOs and `Course` entity.

4. **Register the repository**  
   * In `Program.cs` add: `builder.Services.AddScoped<ICoursesRepository, CoursesRepository>();`

5. **Update the controller**  
   * Replace the existing `CoursesController` constructor to inject `ICoursesRepository`.  
   * Replace action methods to call the repository methods (the skeleton in the file is ready).  

6. **Add DbSet to the DbContext**  
   * File: `Data/SqliteDbContext.cs` – add `public DbSet<Course> Courses { get; set; }`.

7. **Run migrations**  
   * Execute:  
     ```bash
     dotnet ef migrations add AddCourseEntity
     dotnet ef database update
     ```

8. **Test**  
   * Use Swagger or Postman to test the CRUD endpoints.  
   * Add unit tests for the controller and repository using mocks.

9. **Documentation**  
   * Update any README or design docs to reflect the new repository pattern.

### Potential Pitfalls

- Forgetting to register the repository in DI results in `NullReferenceException`.
- Missing the `Course` DbSet will cause EF migration failures.
- DTO and entity mismatches can lead to runtime mapping errors.

### Checklist

- [ ] Course entity fields implemented.
- [ ] DTOs populated.
- [ ] Repository interface and implementation implemented.
- [ ] Repository registered in DI.
- [ ] DbSet added to DbContext.
- [ ] Migrations applied.
- [ ] CRUD endpoints functional.
- [ ] Unit tests passing.

Feel free to adjust the code as needed, but ensure the core concepts (repository pattern, DTO usage, EF Core integration) remain intact.

> **End of Instructions** 
> Good luck! Feel free to ask if you need any clarification or code examples.

