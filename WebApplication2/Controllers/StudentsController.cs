using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.DTO.Students;
using WebApplication2.Repository;

[Route("api/[controller]")]
[ApiController]
public class StudentsController : ControllerBase
{
    private readonly SqliteDbContext _context;
    private readonly IStudentRepository _studentRepository;

    public StudentsController(SqliteDbContext context, IStudentRepository studentRepository)
    {
        _context = context;
        _studentRepository = studentRepository;
    }

    // GET: api/Student
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudent()
    {
        return await _context.Students.ToListAsync();
    }

    // GET: api/Student/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> GetStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
        {
            return NotFound();
        }

        return student;
    }

    // PUT: api/Student/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutStudent(int? id, Student student)
    {
        if (id != student.Id)
        {
            return BadRequest();
        }

        _context.Entry(student).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StudentExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Student
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<IActionResult> PostStudent(CreateStudentRequest request)
    {
        return Ok(await _studentRepository.CreateStudentAsync(request));
    }

    // DELETE: api/Student/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int? id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool StudentExists(int? id)
    {
        return _context.Students.Any(e => e.Id == id);
    }
}
