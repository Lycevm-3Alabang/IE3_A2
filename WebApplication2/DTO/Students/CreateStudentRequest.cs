using WebApplication2.Data;

namespace WebApplication2.DTO.Students
{
    public class CreateStudentRequest
    {
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public Gender StudentGender { get; set; }
    }

    public class CreateStudentResponse
    {
        public int Id { get; set; }
        public string? Student { get; set; }
       
    }
}
