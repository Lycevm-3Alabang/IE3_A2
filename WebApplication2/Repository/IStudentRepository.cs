using IE3_P1.DTO.Students;

namespace IE3_P1.Repository
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
