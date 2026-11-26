using WebApplication2.Data;
using WebApplication2.DTO.Students;

namespace WebApplication2.Repository
{
    public class StudentMySqlRepo : IStudentRepository
    {
        private IList<Student> students = new List<Student>();
        public Task<CreateStudentResponse> CreateStudentAsync(CreateStudentRequest request)
        {
            //list add

            return Task.FromResult(new CreateStudentResponse());
        }
    }


}
