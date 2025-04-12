using TestGenerator.Models;

namespace TestGenerator.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category> GetCategoryByIdAsync(int id);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
    }
} 