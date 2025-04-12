using Microsoft.AspNetCore.Http;
using TestGenerator.Services.Interfaces;

namespace TestGenerator.Services.Implementations
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService>? logger = null)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile image, string? oldImagePath = null)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return string.Empty;
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "question-images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    var oldImageFullPath = Path.Combine(_environment.WebRootPath, oldImagePath.TrimStart('/'));
                    if (File.Exists(oldImageFullPath))
                    {
                        File.Delete(oldImageFullPath);
                    }
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return $"/question-images/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Грешка при запазване на изображението");
                return string.Empty;
            }
        }

        public string DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return string.Empty;
                }

                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return "Изображението беше изтрито успешно";
                }

                return "Изображението не беше намерено";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Грешка при изтриване на изображението");
                return "Грешка при изтриване на изображението";
            }
        }
    }
} 