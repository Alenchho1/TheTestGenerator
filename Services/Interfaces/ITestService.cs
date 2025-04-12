using TestGenerator.Models;

namespace TestGenerator.Services.Interfaces
{
    public interface ITestService
    {
        Task<Test> GetTestByIdAsync(int id);
        Task<List<Test>> GetAllTestsAsync();
        Task<Test> CreateTestAsync(Test test);
        Task<Test> UpdateTestAsync(Test test);
        Task DeleteTestAsync(int id);
        Task<bool> HasAccessToTestAsync(string userId, int testId);
    }
} 