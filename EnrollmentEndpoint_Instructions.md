# IE3 – P1: Enrollment Feature – Exercise Instructions

> **Mirror notice:** This document is a mirror of `CourseEndpoint_Instructions.md` (itself mirrored for Subjects in `SubjectEndpoint_Instructions.md`), adapted from the **Course** feature to the **Enrollment** feature. All structure, user stories, task numbering, conventions, and rules are kept identical; only the model name (`Course` → `Enrollment`), controller name (`CoursesController` → `EnrollmentsController`), DTO names (`*Course*` → `*Enrollment*`), folder name (`DTO/Courses/` → `DTO/Enrolments/`), route (`/api/courses` → `/api/enrollments`), hard-wired page (`wwwroot/courses.html` → `wwwroot/enrolment.html`), and data fields are changed.
>
> **⚠️ What is different for Enrollments:** **A `Student` can only be created in Enrollment.** A `Student` record cannot be created on `StudentsController`, and cannot be created anywhere else; the only way a student comes into existence is as part of creating an `Enrollment`. `StudentsController` may **edit** (update) an already‑created student, but it has **no create** path. See **"Student creation policy"** below.  
> **And on create, an enrolment can add subjects to a single enrolled student.** One `Enrollment` is for exactly **one** enrolled `Student`; `POST /api/enrollments` can, in the same operation, attach subjects to that student. See **"Adding subjects to a single enrolled student on create"** below.

This exercise will extend the current School Management web application with a full **Enrollment** feature.  
You will add the following:

* **Enrollment entity** (EF model)  
* **DTOs** for the CRUD API – named using the `[verb][model][request|response]` convention  
  * *CreateEnrollmentRequest*, *CreateEnrollmentResponse*, *UpdateEnrollmentRequest*, *UpdateEnrollmentResponse*, *GetEnrollmentDetailDTO*  
* **EnrollmentsController** (API controller)  
* **DbContext** registration  

Use the provided **enrolment.html** as a reference for the data fields.

---

## Student creation policy (the key difference)

> **Rule:** *Student can only be created in Enrollment.*

* **A `Student` cannot be created unless it's through enrolment.** There is **no** standalone `POST /api/students`; `StudentsController` does **not** expose a create operation.
* **Creating an `Enrollment` is the only student‑creation path.** `POST /api/enrollments` creates the `Enrollment` and, when the enrolment introduces a new student, creates the `Student` **as part of the same operation** (atomically — both are saved, or neither is).
* **`StudentsController` can only edit an existing student.** Reads and updates (`GET`, `PUT`/`PATCH`) are allowed; `POST` (create) is **not**. A request to create a student on `StudentsController` must be rejected (e.g. `405 Method Not Allowed` / no route) — clients must enrol the student instead.
* **No other endpoint, controller, repository method, or seeding path may construct and save a new `Student`.** `StudentsRepository` must not expose `CreateAsync`; only `EnrollmentsRepository` may add a `Student`.
* **Editing is separate from creating.** Once a student exists (because it was created in an enrolment), its fields (e.g. `Name`, `Email`, `Gender`) may be edited via `StudentsController`, and its enrolment links may be managed via `EnrollmentsController`; editing never creates a new `Student`.

> **Consequence for this exercise:** `EnrollmentsController`/`EnrollmentsRepository` own student creation. When implementing `StudentsController`, omit `CreateStudent` (and `CreateStudentRequest`/`CreateStudentResponse` creation paths); keep only `GetStudents`, `GetStudentById`, `UpdateStudent`, `DeleteStudent` as applicable, with update going through the existing student.

---

## Adding subjects to a single enrolled student on create

> **Rule:** *An enrolment should be able to add subjects to a single enrolled student on create.*

* **One enrolment, one student.** An `Enrollment` is always for a **single** `Student`; there is no multi‑student enrolment. The subjects below are attached to **that one enrolled student** (via `Enrollment.Subjects` for that student's enrolment).
* **On `POST /api/enrollments`, an optional list of subjects may be supplied.** `CreateEnrollmentRequest` accepts `subjectIds` (`List<int>`, default empty); each id references an **existing** `Subject`. When present, those subjects are **added to the single enrolled student's enrolment** in the **same operation** as enrolment creation (and, if this enrolment also creates the student per the policy above, in the **same unit of work / transaction**).
* **Adding subjects ≠ creating subjects.** `Subject` records are created independently through the Subjects feature (`SubjectsController`, `/api/subjects`). Enrolment only **links/adds** existing subjects to the enrolled student; it does not create `Subject` rows. (Only `Student` creation is restricted to enrolment.)  
* **Subjects endpoint shape.** The subjects endpoint (`GET /api/subjects`) used to populate the hard-wired multi‑select returns **`Id` and `Name`** for each subject (camelCase `id`, `name`; see `SubjectEndpoint_Instructions.md`); `name` is displayed, `id` is submitted in `subjectIds`.
* **Populate `Enrollment.Subjects`.** On create, load the matching `Subject`s and assign them to `enrollment.Subjects` before `SaveChangesAsync()`. Unknown/unsupplied ids are ignored or rejected; an empty/absent list simply creates the enrolment with no added subjects.
* **This applies on create.** `UpdateEnrollmentRequest` (PUT `/api/enrollments/{id}`) updates the enrolment's links; per this requirement subjects are added **on create**. (If you choose to also allow editing subjects on update, keep it separate and explicit; it is not required.)

---

## User Story 1 – Implement the `Enrollment` Entity

> **What is an Entity?**  
> An *Entity* is a C# class that maps to a database table via Entity Framework Core. It represents a persistent domain object that the application stores and retrieves.

> **Entity vs DTO**  
> * **Entity** – Used by EF to read/write data in the database.  
> * **DTO (Data Transfer Object)** – A lightweight object that is sent over the network (API). It typically contains only the data that the client needs and may omit navigation properties or sensitive fields.

### Task 1.1 – Add required properties to `Data/Enrollment.cs`

Open `Data/Enrollment.cs` and add the following properties:

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Data
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime EnrollDate { get; set; }

        public Student? Student { get; set; }
        public Course? Course { get; set; }
        public List<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
```

> **Tip:** The hard-wired form in *enrolment.html* uses `studentName`, `studentEmail`, `studentGender`, `courseId`, `enrollDate`, `subjectIds`. On **create** the student isn't an existing id (`studentId`); student details are supplied and a `Student` is created, after which `Enrollment.StudentId` is set to the new student's id. (`StudentId` is still the `Enrollment` → `Student` FK on the entity, and `PUT` may re‑point it to an existing student.)
>
> **Note:** `EnrollDate` is added to match the hard-wired `enrollDate` field (`<input type="date">`); it did not exist on the original `Enrollment`. The `DateTime` type binds `yyyy‑MM‑dd`. `Course? Course`, `Student? Student`, and the existing `List<Subject> Subjects` are **navigation properties** — they are used by EF but are normally **omitted from DTOs**. `CourseId` references an existing `Course`; the `StudentId` references a `Student` (which, per the policy above, can only have been created in an enrolment).
>
> **Subjects on a single enrolled student:** `Subjects` is the list of subjects added to **this one enrolled student** (this enrolment). On create, `CreateEnrollmentRequest.subjectIds` can populate it; see **"Adding subjects to a single enrolled student on create"**.

### Task 1.2 – Verify validation attributes

| Property | Attribute(s) | Reason |
|----------|-------------|--------|
| `StudentId` | `[Required]` | Ensures the enrolment is linked to a student. |
| `CourseId` | `[Required]` | Ensures the enrolment is linked to a course. |
| `EnrollDate` | `[Required]` | Ensures an enrolment date is recorded. |

---

## User Story 2 – Create the DTOs for CRUD Operations

> **Why DTOs?**  
> DTOs decouple the API surface from the database schema. They allow you to shape the data returned to the client and validate incoming payloads.

Create a new folder: `DTO/Enrolments/` and add the following files using the `[verb][model][request|response]` naming convention:

> **Spelling note:** the repo already uses the British folder `DTO/Enrolments/` and page `enrolment.html`, while the entity is `Enrollment` and the route is `/api/enrollments`. Keep these as‑is; do not rename to `Enrollments`/`enrollment.html`.

### Task 2.1 – Create `CreateEnrollmentRequest.cs`

**File:** `DTO/Enrolments/CreateEnrollmentRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using IE3_P1.Data;

namespace IE3_P1.DTO.Enrolments
{
    public class CreateEnrollmentRequest
    {
        // ⚠️ NO StudentId. On create the student does NOT yet exist;
        //    supply the new student's details — the enrolment creates the Student.

        /// <summary>Name of the not‑yet‑existing student to create in this enrolment.</summary>
        [Required]
        public string Name { get; set; } = default!;

        /// <summary>Email of the not‑yet‑existing student to create in this enrolment.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        /// <summary>Gender of the not‑yet‑existing student (Male / Female / DoNotSpecify).</summary>
        public Gender Gender { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime EnrollDate { get; set; }

        /// <summary>Existing Subject ids to add to this single enrolled student. Optional.</summary>
        public List<int> SubjectIds { get; set; } = new List<int>();
    }
}
```

> **Note:** A `CreateEnrollmentRequest.cs` skeleton already exists in `DTO/Enrolments/`; fill in its properties as shown.  
> **POST create does NOT take `studentId`.** On `POST /api/enrollments` **a student is not yet existing**, so the request carries **student details** — `name`, `email`, `gender`, etc. — **together with `courseId` and `subjectIds`** (and `enrollDate`). The enrolment creates that student. `Gender` is `IE3_P1.Data.Gender`. **This is the only request DTO permitted to carry student‑creation data.**  
> **`subjectIds`:** an optional list of **existing** `Subject` ids to add to the single enrolled student (populated in `Enrollment.Subjects`). Empty/absent ⇒ no subjects added. `Subject`s themselves are created via the Subjects feature, not here.

### Task 2.2 – Create `CreateEnrollmentResponse.cs`

**File:** `DTO/Enrolments/CreateEnrollmentResponse.cs`

```csharp
using System.Collections.Generic;
using IE3_P1.Data;

namespace IE3_P1.DTO.Enrolments
{
    public class CreateEnrollmentResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }

        /// <summary>All subjects added to this enrolled student — each with Id + Name.</summary>
        public List<SubjectSummary> Subjects { get; set; } = new List<SubjectSummary>();

        /// <summary>The single enrolled student — Id + all details.</summary>
        public StudentDetails Student { get; set; } = default!;

        /// <summary>The course — Id + Name.</summary>
        public CourseSummary Course { get; set; } = default!;

        public class SubjectSummary
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
        }

        public class StudentDetails
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public Gender Gender { get; set; }
            public int CourseId { get; set; }
            // ... any other Student details (navigation omitted)
        }

        public class CourseSummary
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
        }
    }
}
```

> **Create‑enrolment response shape (`POST /api/enrollments`, 201):** besides `id`/`studentId`/`courseId`/`enrollDate`, the response **lists**:
> * **`subjects`** — **all** subjects added to this single enrolled student, **each with `id` and `name`** (`Subjects` → `List<SubjectSummary>`; sourced from `Enrollment.Subjects`; `Subject.Name`; empty list if none added);
> * **`student`** — the single enrolled `Student` with **`id` and all details** (`name`, `email`, `gender`, `courseId`, …; `StudentDetails`; navigation props omitted; `gender` = `IE3_P1.Data.Gender`);
> * **`course`** — the `Course` with **`id` and `name`** (`CourseSummary`).
>
> camelCase JSON: `subjects`, `student`, `course`, `id`, `name`, `email`, `gender`, `courseId`, `studentId`, `enrollDate`. (`Student` fields mirror `Data/Student.cs`.)
>
> **Course `name` note:** `CourseSummary.Name` maps to the course's name field — in this codebase `Course.Title` (`DTO/Courses/CourseResponse.Title`); if `Course` has no `Name`/`Title`, populate it from whatever course name field exists, or add `Course.Name`. `Course.Id` is the `courseId`.

### Task 2.3 – Create `UpdateEnrollmentRequest.cs`

**File:** `DTO/Enrolments/UpdateEnrollmentRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace IE3_P1.DTO.Enrolments
{
    public class UpdateEnrollmentRequest
    {
        public int Id { get; set; }

        public int? StudentId { get; set; }

        public int? CourseId { get; set; }

        public DateTime? EnrollDate { get; set; }
    }
}
```

> **Why nullable fields in `UpdateEnrollmentRequest`?**  
> It allows partial updates — if a field is not supplied by the client, the repository will leave it unchanged.
>
> **Note:** `UpdateEnrollmentRequest` updates the **enrolment**; it does **not** create a `Student`. Editing a student's own fields happens through `StudentsController`. (`StudentId` may be re‑pointed to another existing student; it cannot create one.)  
> Subjects are added to the enrolled student **on create** (see `CreateEnrollmentRequest.subjectIds`); `UpdateEnrollmentRequest` does not require subject handling.

### Task 2.4 – Create `UpdateEnrollmentResponse.cs`

**File:** `DTO/Enrolments/UpdateEnrollmentResponse.cs`

```csharp
namespace IE3_P1.DTO.Enrolments
{
    public class UpdateEnrollmentResponse
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
    }
}
```

### Task 2.5 – Create `GetEnrollmentDetailDTO.cs`

**File:** `DTO/Enrolments/GetEnrollmentDetailDTO.cs`

```csharp
namespace IE3_P1.DTO.Enrolments
{
    public class GetEnrollmentDetailDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
    }
}
```

> **Note:** DTOs do not need validation attributes — they are used only for output (response) purposes. Validation is handled by the request DTOs.  
> Navigation properties (`Student`, `Course`, `Subjects`) are **not** included in this DTO.  
> **Name:** `EnrollmentResponse` is named **`GetEnrollmentDetailDTO`**; it is reusable **as a list element** — `GetAllAsync()` returns `IEnumerable<GetEnrollmentDetailDTO>`, `GET /api/enrollments` returns `[ GetEnrollmentDetailDTO, … ]`, and `GetEnrollmentById` returns a single `GetEnrollmentDetailDTO`.

---

## User Story 3 – Implement the Repository Layer

> **Notice:** The following design uses a repository interface to keep the controller thin. The repository will expose CRUD operations that accept/return DTOs. This pattern is a step toward a scalable, maintainable architecture.

### Task 3.1 – Create `IEnrollmentsRepository.cs`

**File:** `Repositories/IEnrollmentsRepository.cs`

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using IE3_P1.DTO.Enrolments;

namespace IE3_P1.Repositories
{
    public interface IEnrollmentsRepository
    {
        Task<IEnumerable<GetEnrollmentDetailDTO>> GetAllAsync();
        Task<GetEnrollmentDetailDTO?> GetByIdAsync(int id);
        Task<CreateEnrollmentResponse> CreateAsync(CreateEnrollmentRequest dto);
        Task UpdateAsync(int id, UpdateEnrollmentRequest dto);
        Task DeleteAsync(int id);
    }
}
```

### Task 3.2 – Implement `EnrollmentsRepository.cs`

**File:** `Repositories/EnrollmentsRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using IE3_P1.Data;
using IE3_P1.DTO.Enrolments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IE3_P1.Repositories
{
    public class EnrollmentsRepository : IEnrollmentsRepository
    {
        private readonly SqliteDbContext _context;

        public EnrollmentsRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GetEnrollmentDetailDTO>> GetAllAsync()
        {
            return await _context.Enrollments
                .Select(e => new GetEnrollmentDetailDTO
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    CourseId = e.CourseId,
                    EnrollDate = e.EnrollDate
                })
                .ToListAsync();
        }

        public async Task<GetEnrollmentDetailDTO?> GetByIdAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return null;

            return new GetEnrollmentDetailDTO
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                EnrollDate = enrollment.EnrollDate
            };
        }

        public async Task<CreateEnrollmentResponse> CreateAsync(CreateEnrollmentRequest dto)
        {
            // ⚠️ POST create: a student is NOT yet existing. The request carries student details
            //    (name, email, gender, ...) + courseId + subjectIds — NOT a studentId.
            // ⚠️ Student can ONLY be created here (in enrolment). No other code path may add a Student.
            //    Create ONE student for THIS single enrolled student, in the SAME unit of work.
            var student = new Student
            {
                Name = dto.Name,
                Email = dto.Email,
                Gender = dto.Gender,
                CourseId = dto.CourseId
            };
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();          // assigns student.Id

            var enrollment = new Enrollment
            {
                StudentId = student.Id,                 // link enrolment to the just-created student
                CourseId = dto.CourseId,
                EnrollDate = dto.EnrollDate
            };

            // Never create a Student from StudentsController or any other endpoint.

            // An enrolment can add subjects to a single enrolled student on create:
            //   attach existing Subjects (by id) to THIS enrolment's single student. Subjects are
            //   created via the Subjects feature; here we only link them. Unknown ids are ignored.
            if (dto.SubjectIds != null && dto.SubjectIds.Count > 0)
            {
                enrollment.Subjects = await _context.Subjects
                    .Where(s => dto.SubjectIds.Contains(s.Id))
                    .ToListAsync();
            }

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            // Create-enrolment response: enrolment + ALL subjects + single student + course.
            var course = await _context.Courses.FindAsync(enrollment.CourseId);   // Course.Id (+ name field)

            return new CreateEnrollmentResponse
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                EnrollDate = enrollment.EnrollDate,

                // all subjects added to this single enrolled student — each Id + Name
                Subjects = enrollment.Subjects
                    .Select(s => new CreateEnrollmentResponse.SubjectSummary { Id = s.Id, Name = s.Name })
                    .ToList(),

                // the single enrolled student — Id + all details
                Student = new CreateEnrollmentResponse.StudentDetails
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    Gender = student.Gender,
                    CourseId = student.CourseId
                },

                // the course — Id + Name (Course.Title / Course.Name)
                Course = new CreateEnrollmentResponse.CourseSummary
                {
                    Id = enrollment.CourseId,
                    Name = course?.Title ?? string.Empty   // Course name; use Course.Name if that field exists
                }
            };
        }

        public async Task UpdateAsync(int id, UpdateEnrollmentRequest dto)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) throw new KeyNotFoundException($"Enrollment with Id {id} not found.");

            if (dto.StudentId.HasValue) enrollment.StudentId = dto.StudentId.Value;   // re-point to an EXISTING student; never creates one
            if (dto.CourseId.HasValue) enrollment.CourseId = dto.CourseId.Value;
            if (dto.EnrollDate.HasValue) enrollment.EnrollDate = dto.EnrollDate.Value;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) throw new KeyNotFoundException($"Enrollment with Id {id} not found.");

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
        }
    }
}
```

> **Return conventions**  
> * `GetByIdAsync` returns `null` when the entity does not exist — the controller should translate this into a 404.  
> * `CreateAsync` returns the newly created DTO, including its `Id`.  
> * `UpdateAsync` and `DeleteAsync` throw `KeyNotFoundException` if the entity is missing — the controller catches this and converts it to a 404.  
> * `CreateAsync` is the **only** method permitted to add a `Student` (when the enrolment introduces one). `UpdateAsync` never creates a `Student`; there is no student‑create method on any other repository.

---

## User Story 4 – Implement the EnrollmentsController

### Task 4.1 – Create `EnrollmentsController.cs`

**File:** `Controllers/EnrollmentsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using IE3_P1.DTO.Enrolments;
using IE3_P1.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IE3_P1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentsRepository _repository;

        public EnrollmentsController(IEnrollmentsRepository repository)
        {
            _repository = repository;
        }

        // GET: api/enrollments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetEnrollmentDetailDTO>>> GetEnrollments()
        {
            var enrollments = await _repository.GetAllAsync();
            return Ok(enrollments);
        }

        // POST: api/enrollments
        [HttpPost]
        public async Task<ActionResult<CreateEnrollmentResponse>> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // repository returns the full create-enrolment response
            // (id + studentId/courseId/enrollDate + subjects + student + course)
            var created = await _repository.CreateAsync(request);
            return CreatedAtAction(nameof(GetEnrollmentById), new { id = created.Id }, created);
        }

        // GET: api/enrollments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GetEnrollmentDetailDTO>> GetEnrollmentById(int id)
        {
            var enrollment = await _repository.GetByIdAsync(id);
            if (enrollment == null) return NotFound();
            return Ok(enrollment);
        }

        // PUT: api/enrollments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentRequest request)
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

        // DELETE: api/enrollments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
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
> * There is **no** student‑create action here; `POST /api/enrollments` is the only way a `Student` gets created.

### Task 4.2 – `StudentsController`: edit allowed, create forbidden

If a `StudentsController` exists (or is added), it **must not** create students:

```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    // GET api/students, GET api/students/{id} ...

    // ✅ EDIT an existing student:
    [HttpPut("{id}")]      // (and/or [HttpPatch("{id}")])
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request) { /* update existing only */ }

    // ❌ NO POST create. A student cannot be created unless it's through enrolment.
    //    Do NOT add: [HttpPost] CreateStudent(...)
    //    Do NOT inject/use a "StudentsRepository.CreateAsync".
}
```

> A `POST /api/students` request must **not** create a student; only `POST /api/enrollments` can. Students are created inside `EnrollmentsRepository.CreateAsync`; `StudentsController`/`StudentsRepository` expose update, not create.

---

## User Story 5 – Register Dependencies in DI

### Task 5.1 – Register the repository in `Program.cs`

Add the following line **after** `builder.Services.AddOpenApi();`:

```csharp
builder.Services.AddScoped<IEnrollmentsRepository, EnrollmentsRepository>();
```

> **Why `Scoped`?**  
> The repository depends on a `DbContext`. `Scoped` aligns with the typical EF Core lifetime within an HTTP request.  
> Do **not** register any student‑creating service; student creation is reachable only through `IEnrollmentsRepository`.

### Task 5.2 – Verify `SqliteDbContext` registration

Ensure `Program.cs` already contains:

```csharp
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

---

## User Story 6 – Add DbSet to DbContext & Run Migrations

### Task 6.1 – Add the `Enrollments` DbSet to `SqliteDbContext.cs`

Open `Data/SqliteDbContext.cs` and add:

```csharp
public DbSet<Enrollment> Enrollments { get; set; }
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
        public DbSet<Enrollment> Enrollments { get; set; }   // <-- new line
    }
}
```

> **Prerequisites for this feature:** enrolment **creates a `Student`** and can **add subjects**, so `_context.Students` and `_context.Subjects` must exist — ensure `public DbSet<Student> Students { get; set; }` and `public DbSet<Subject> Subjects { get; set; }` are present (add them if missing; `Student`/`Subject` are created elsewhere, `Student` creation is only *triggered* here). Only `DbSet<Enrollment> Enrollments` is the new line for **this** feature.

> **Important:** *Do not edit this file automatically.* Students should copy this line and then run the migration commands in Task 6.2.

### Task 6.2 – Run EF Core migrations

After adding the DbSet, run:

```bash
dotnet ef migrations add AddEnrollmentEntity
dotnet ef database update
```

> **Tip:** If you want a specific table name, override it in `OnModelCreating`:
> ```csharp
> modelBuilder.Entity<Enrollment>().ToTable("Enrollments");
> ```

---

## User Story 7 – Test Against the Hard‑Wired Front‑End (`enrolment.html`)

> **Critical Rule:** The file `wwwroot/enrolment.html` is **hard-wired** and **must not be changed**.  
> It is provided as a fixed, non-modifiable front-end that students must test their implementation against.  
> (It ships with its page‑specific JavaScript hard-wired inline; no separate page script file is required.)
>
> **Note the spelling:** the page is `enrolment.html`, the table body is `enrolmentsTable`, and the title reads *Enrolment*; the entity is `Enrollment`, the DTO folder is `DTO/Enrolments`, and the route is `/api/enrollments`.

### Task 7.1 – Understand the front-end's expected API contract

| Operation | Method | Endpoint | Request Body Fields | Expected Response Field(s) |
|-----------|--------|----------|---------------------|----------------------------|
| **GET all** | `GET` | `/api/enrollments` | *(none)* | `[ { "id": number, "studentId": number, "courseId": number, "enrollDate": string } ]` |
| **GET by ID** | `GET` | `/api/enrollments/{id}` | *(none)* | `{ "id": number, "studentId": number, "courseId": number, "enrollDate": string }` |
| **POST create** | `POST` | `/api/enrollments` | `{ "name": string, "email": string, "gender": string, "courseId": number, "enrollDate": string, "subjectIds"?: number[] }` — **no `studentId`; a student is not yet existing; send student details + `courseId` + `subjectIds`** (+`enrollDate`) | `{ "id": number, "studentId": number, "courseId": number, "enrollDate": string, "subjects": [ { "id": number, "name": string } ], "student": { "id": number, "name": string, "email": string, "gender": string, "courseId": number, "...": "all student details" }, "course": { "id": number, "name": string } }` — **lists all `subjects` (id + name), `student` (id + all details), `course` (id + name)** (Location header on 201; `student.id`/`studentId` is the id of the just‑created student) |
| **PUT update** | `PUT` | `/api/enrollments/{id}` | `{ "id": number, "studentId"?: number, "courseId"?: number, "enrollDate"?: string }` | 204 No Content |
| **DELETE** | `DELETE` | `/api/enrollments/{id}` | *(none)* | 204 No Content |

> **Key constraints imposed by the front-end:**  
> * The form field IDs (`studentName`, `studentEmail`, `studentGender`, `courseId`, `enrollDate`, `subjectIds`) map to JSON keys using **camelCase**. On **create**, student details are sent as `name`, `email`, `gender` + `courseId` + `subjectIds` (+`enrollDate`); **not** `studentId`. If your API returns PascalCase properties (`StudentId`, `CourseId`, `Name`), the front-end will not bind them.  
> * **Course dropdown (single-select):** `#courseId` is a `<select>` (not `multiple`) **bound to the UI**; on load the hard-wired page fetches **`GET /api/courses`** and renders one `<option value="id">name</option>` per course (all courses; `id` submitted, `name` displayed; PascalCase `Id`/`Name`/`Title` tolerated; course `name` maps to `Course.Title`/`Course.Name`). A placeholder `Select course…` has empty value.  
> * **Email validation requirement:** `studentEmail` is `type="email"`, **required**, and validated as a proper email address (simple `local@domain.tld` check) before submit on **create**; `CreateEnrollmentRequest.Email` carries `[Required]` + `[EmailAddress]`.  
> * The response object **must** include an `id` field — the front-end uses it to populate the hidden `#enrollmentId` field during edit operations.  
> * All requests must be sent with `Content-Type: application/json`.  
> * The page invokes `saveEnrollment()` on **Save Enrolment** and `resetForm()` on **Cancel**; the table body is `#enrolmentsTable`; `enrollDate` is a date string (`yyyy‑MM‑dd`).  
> * **Student creation rule:** the hard-wired page does not create standalone students; per policy, a `Student` is only ever created through `POST /api/enrollments`.  
> * **Subjects on a single enrolled student:** `subjectIds` (`number[]`, optional) on **POST** lists existing subjects to add to that one enrolled student on create; it is camelCase and sent with `Content-Type: application/json`.  
> * **Subjects multi‑select:** `#subjectIds` is a `<select multiple>` **bound to the UI**; on page load the hard-wired page fetches **`GET /api/subjects`** and renders one `<option value="id">name</option>` per subject. **The subjects endpoint returns `Id` and `Name`** for each subject (camelCase `id`, `name`; `name` is shown, `id` is submitted; PascalCase `Id`/`Name` are tolerated). On save, `getSubjectIds()` reads `selectedOptions` → `subjectIds[]` (`number[]`) on **create**; nothing is sent on update.

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
> Without this, ASP.NET Core serializes properties in PascalCase (`StudentId`, `CourseId`), which the hard-wired front-end cannot match. This is a common source of "working API but broken UI" bugs.  
> (ASP.NET Core's default JSON settings already use camelCase; this makes it explicit.)

### Task 7.3 – Verify CORS or static file serving is enabled

Ensure that:
* Static files (including `wwwroot/enrolment.html`) are served by confirming the following exists in `Program.cs`:

```csharp
app.UseStaticFiles();

> **API base URL & port 8000:** The hard-wired page hardcodes **`http://localhost:8000/api/<endpoint>`**; it does **not** use a relative `/api/...`. **Set `applicationUrl` to `http://localhost:8000` in `WebApplication2/Properties/launchSettings.json`** (profile `http`; `dotnet run --launch-profile http`, or `dotnet run --urls http://localhost:8000`; avoid `https://localhost:...`/other ports so `app.UseHttpsRedirection()` doesn't redirect). Once the app runs on **8000** and your endpoints are created, **nothing needs to be changed on the HTML page.**
```

* This must be placed **before** `app.UseHttpsRedirection()` or `app.MapControllers()`.

### Task 7.4 – Test the implementation end-to-end

1. Start the application (`dotnet run` or F5).  
2. Open `http://localhost:<port>/enrolment.html` in a browser.  
3. Verify each operation:
   * **Add** — Enter **Student Name**, a **valid Student Email** (required + email‑format validated), and **Student Gender**; pick a **Course** from the single‑select **course dropdown** (all courses, `id` + `name`, bound to `GET /api/courses`); enter **Enrollment Date**; and **(optionally) multi‑select Subjects** from the subjects `<select multiple>` (bound to `GET /api/subjects`, `id` + `name`) to add them to this single enrolled student on create. Click "Save Enrolment". (On create **no Student ID is entered** — the student doesn't exist yet; `name`/`email`/`gender` + `courseId` + `subjectIds` + `enrollDate` are sent and the student is created.) Confirm the enrolment appears in the table below.  
   * **Edit** — Click a row's edit button, modify fields, and save. Confirm the update persists.  
   * **Delete** — Click delete on a row. Confirm it is removed from the table.  
   * **List** — Reload the page. Confirm all previously saved enrolments are displayed.  
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
* Missing the `Enrollment` DbSet will cause EF migration failures.  
* DTO and entity mismatches can lead to runtime mapping errors.  
* Not using `[FromBody]` on POST/PUT may cause model binding to fail silently.  
* **Creating a `Student` anywhere other than inside `EnrollmentsRepository.CreateAsync` violates the rule that a student can only be created in enrolment.**  
* Exposing a `POST /api/students` (create) on `StudentsController`, or a `StudentsRepository.CreateAsync`, breaks the student‑creation policy; `StudentsController` may only **edit** existing students.  
* Renaming the hard-wired page/folder spellings (`enrolment.html`, `DTO/Enrolments`, `enrolmentsTable`, `enrollmentId`) breaks the front-end.  
* `subjectIds` references **existing** `Subject`s; treating it as subject **creation**, or allowing a multi‑student enrolment, breaks "add subjects to a single enrolled student on create".

## Checklist

* [ ] Enrollment entity fields implemented with validation attributes (`StudentId`, `CourseId`, `EnrollDate`; `EnrollDate` added to match `enrollDate`).  
* [ ] All five DTOs created with correct naming convention.  
* [ ] Repository interface and implementation completed.  
* [ ] Repository registered in DI (`Program.cs`).  
* [ ] `DbSet<Enrollment>` added to `SqliteDbContext`.  
* [ ] EF migrations applied.  
* [ ] CRUD endpoints functional (tested via Swagger or Postman).  
* [ ] Front-end `enrolment.html` tested end-to-end — add, edit, delete, and list all work without modifying the HTML file.  
* [ ] **Student creation policy enforced:** a `Student` can only be created in enrolment (`POST /api/enrollments`); `StudentsController` allows editing but **not** creating; no other code path adds a `Student`.  
* [ ] **Subjects on a single enrolled student on create:** `POST /api/enrollments` accepts optional `subjectIds` and adds those existing subjects to that one enrolled student (`Enrollment.Subjects`) in the same operation.  
* [ ] **Course dropdown (single-select):** `#courseId` is a `<select>` of all courses from `GET /api/courses` (`id` + `name`); not free-text.  
* [ ] **Email validation requirement:** student email is required and validated as a proper email address (front-end + `[Required]`/`[EmailAddress]` on `CreateEnrollmentRequest.Email`).

---

**End of Instructions**  
Good luck! Feel free to ask if you need clarification on any step.
