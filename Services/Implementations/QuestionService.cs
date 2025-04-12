using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.Services.Interfaces;

namespace TestGenerator.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _context;

        public QuestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.Creator)
                .Include(q => q.Category)
                .Include(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            return await _context.Questions
                .Include(q => q.Creator)
                .Include(q => q.Category)
                .Include(q => q.PossibleAnswers)
                .ToListAsync();
        }

        public async Task<Question> CreateQuestionAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<Question> UpdateQuestionAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await GetQuestionByIdAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasAccessToQuestionAsync(string userId, int questionId)
        {
            var question = await GetQuestionByIdAsync(questionId);
            if (question == null) return false;

            return question.CreatorId == userId;
        }
    }
} 