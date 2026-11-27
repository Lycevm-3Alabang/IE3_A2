using IE3_P1.Data;
using IE3_P1.DTO.Students;

namespace IE3_P1.Repository
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
