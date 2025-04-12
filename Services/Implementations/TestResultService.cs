using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.Services.Interfaces;

namespace TestGenerator.Services.Implementations
{
    public class TestResultService : ITestResultService
    {
        private readonly ApplicationDbContext _context;

        public TestResultService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestResult> GetTestResultByIdAsync(int id)
        {
            return await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.User)
                .Include(tr => tr.AnswerResults)
                .ThenInclude(ar => ar.Question)
                .FirstOrDefaultAsync(tr => tr.Id == id);
        }

        public async Task<List<TestResult>> GetTestResultsByUserIdAsync(string userId)
        {
            return await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.User)
                .Include(tr => tr.AnswerResults)
                .ThenInclude(ar => ar.Question)
                .Where(tr => tr.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<TestResult>> GetTestResultsByTestIdAsync(int testId)
        {
            return await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.User)
                .Include(tr => tr.AnswerResults)
                .ThenInclude(ar => ar.Question)
                .Where(tr => tr.TestId == testId)
                .ToListAsync();
        }

        public async Task<TestResult> CreateTestResultAsync(TestResult testResult)
        {
            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();
            return testResult;
        }

        public async Task DeleteTestResultAsync(int id)
        {
            var testResult = await GetTestResultByIdAsync(id);
            if (testResult != null)
            {
                _context.TestResults.Remove(testResult);
                await _context.SaveChangesAsync();
            }
        }
    }
} 