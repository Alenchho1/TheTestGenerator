using Microsoft.AspNetCore.Http;

namespace TestGenerator.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string? oldImagePath = null);
        string DeleteImage(string imagePath);
    }
} 