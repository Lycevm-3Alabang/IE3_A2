using System.ComponentModel.DataAnnotations;

namespace IE3_P1.DTO.Students
{
    public class CreateStudentRequest
    {
        [Required]
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public int? CourseId { get; set; }
    }

  
}
