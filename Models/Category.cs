using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    
    /// Модел за категория на въпросите
    public class Category
    {
        
        /// Конструктор, инициализиращ колекцията с въпроси
        public Category()
        {
            Questions = new List<Question>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Името на категорията е задължително")]
        [MinLength(3, ErrorMessage = "Името трябва да бъде поне 3 символа")]
        [MaxLength(50, ErrorMessage = "Името не може да бъде повече от 50 символа")]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "Описанието не може да бъде повече от 200 символа")]
        public string? Description { get; set; }

        /// Колекция от въпроси в категорията
        public ICollection<Question>? Questions { get; set; }
    }
} 