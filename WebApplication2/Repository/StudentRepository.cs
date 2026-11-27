using IE3_P1.Data;
using IE3_P1.DTO.Students;

namespace IE3_P1.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SqliteDbContext _context;
        public StudentRepository(SqliteDbContext context)
        {
            _context = context;
        }
        public async Task<CreateStudentResponse> CreateStudentAsync(CreateStudentRequest request)
        {
            var student = new Student
            {
                Name = request.StudentName,
                Email = request.StudentEmail,
                Gender = request.StudentGender
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return new CreateStudentResponse
            {
                Id = student.Id,
                Student = student.Name
            };
        }
    }


}
