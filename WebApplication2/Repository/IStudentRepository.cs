using WebApplication2.DTO.Students;

namespace WebApplication2.Repository
{
    public interface IStudentRepository
    {
        Task<CreateStudentResponse> CreateStudentAsync(CreateStudentRequest request);

    }


    public class StudentMongoDbRepository : IStudentRepository
    {
        public Task<CreateStudentResponse> CreateStudentAsync(CreateStudentRequest request)
        {
            throw new NotImplementedException();
        }
    }

}
