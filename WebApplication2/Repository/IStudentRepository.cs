using IER_P1.Data;

namespace IER_P1.Repository
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
