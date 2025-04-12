using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.Services.Interfaces;

namespace TestGenerator.Services.Implementations
{
    public class TestService : ITestService
    {
        private readonly ApplicationDbContext _context;

        public TestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Test> GetTestByIdAsync(int id)
        {
            return await _context.Tests
                .Include(t => t.Creator)
                .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Test>> GetAllTestsAsync()
        {
            return await _context.Tests
                .Include(t => t.Creator)
                .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                .ToListAsync();
        }

        public async Task<Test> CreateTestAsync(Test test)
        {
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();
            return test;
        }

        public async Task<Test> UpdateTestAsync(Test test)
        {
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
            return test;
        }

        public async Task DeleteTestAsync(int id)
        {
            var test = await GetTestByIdAsync(id);
            if (test != null)
            {
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasAccessToTestAsync(string userId, int testId)
        {
            var test = await GetTestByIdAsync(testId);
            if (test == null) return false;

            return test.CreatorId == userId;
        }
    }
} 