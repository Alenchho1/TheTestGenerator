using TestGenerator.Models;

namespace TestGenerator.Services.Interfaces
{
    public interface ITestResultService
    {
        Task<TestResult> GetTestResultByIdAsync(int id);
        Task<List<TestResult>> GetTestResultsByUserIdAsync(string userId);
        Task<List<TestResult>> GetTestResultsByTestIdAsync(int testId);
        Task<TestResult> CreateTestResultAsync(TestResult testResult);
        Task DeleteTestResultAsync(int id);
    }
} 