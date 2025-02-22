using Microsoft.AspNetCore.Http;

namespace TestGenerator.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string oldImagePath = null);
        void DeleteImage(string imagePath);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageService> _logger;
        private const string ImageFolder = "question-images";

        public ImageService(IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile image, string oldImagePath = null)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, ImageFolder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    DeleteImage(oldImagePath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return $"~/{ImageFolder}/" + uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return null;
            }
        }

        public void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            try
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('~', '/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
            }
        }
    }
} 