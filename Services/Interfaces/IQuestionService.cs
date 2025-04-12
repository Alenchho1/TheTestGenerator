using TestGenerator.Models;

namespace TestGenerator.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<Question> GetQuestionByIdAsync(int id);
        Task<List<Question>> GetAllQuestionsAsync();
        Task<Question> CreateQuestionAsync(Question question);
        Task<Question> UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(int id);
        Task<bool> HasAccessToQuestionAsync(string userId, int questionId);
    }
} 