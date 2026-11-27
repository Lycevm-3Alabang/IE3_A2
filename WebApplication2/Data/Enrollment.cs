using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Data
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }

        public List<Subject> Subjects { get; set; } = new List<Subject>();
        public Student? Student { get; set; }
    }
}
