using System.ComponentModel.DataAnnotations;

namespace TestGenerator.ViewModels
{
    public class TestEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително")]
        [StringLength(200, ErrorMessage = "Заглавието не може да бъде по-дълго от 200 символа")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Описанието не може да бъде по-дълго от 1000 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Времето за решаване е задължително")]
        [Range(1, 180, ErrorMessage = "Времето за решаване трябва да бъде между 1 и 180 минути")]
        [Display(Name = "Време за решаване (минути)")]
        public int TimeLimit { get; set; }

        [Required(ErrorMessage = "Нивото на трудност е задължително")]
        [Range(1, 5, ErrorMessage = "Нивото на трудност трябва да бъде между 1 и 5")]
        [Display(Name = "Ниво на трудност")]
        public int DifficultyLevel { get; set; }
    }
} 