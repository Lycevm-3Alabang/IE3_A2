using System.ComponentModel.DataAnnotations;

namespace IE3_P1.Data
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public Gender Gender { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        DoNotSpecify
    }
}
