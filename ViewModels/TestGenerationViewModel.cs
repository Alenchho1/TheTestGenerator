using System.ComponentModel.DataAnnotations;

namespace TestGenerator.ViewModels
{
    public class TestGenerationViewModel
    {
        [Required(ErrorMessage = "Заглавието е задължително")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Заглавието трябва да бъде между 3 и 100 символа")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Описанието не може да бъде по-дълго от 500 символа")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Броят въпроси е задължителен")]
        [Range(1, 50, ErrorMessage = "Броят въпроси трябва да бъде между 1 и 50")]
        [Display(Name = "Брой въпроси")]
        public int NumberOfQuestions { get; set; }

        [Required(ErrorMessage = "Времето за решаване е задължително")]
        [Range(5, 120, ErrorMessage = "Времето за решаване трябва да бъде между 5 и 120 минути")]
        [Display(Name = "Време за решаване")]
        public int TimeLimit { get; set; }

        [Required(ErrorMessage = "Моля, изберете категория")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }
    }
} 