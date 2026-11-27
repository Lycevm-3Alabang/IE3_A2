using IE3_P1.Data;

namespace IE3_P1.DTO.Students
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
