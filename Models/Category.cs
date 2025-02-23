using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    /// <summary>
    /// Модел за категория на въпросите
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Конструктор, инициализиращ колекцията с въпроси
        /// </summary>
        public Category()
        {
            Questions = new List<Question>();
        }

        /// <summary>
        /// Уникален идентификатор на категорията
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на категорията
        /// </summary>
        [Required(ErrorMessage = "Името на категорията е задължително")]
        [MinLength(3, ErrorMessage = "Името трябва да бъде поне 3 символа")]
        [MaxLength(50, ErrorMessage = "Името не може да бъде повече от 50 символа")]
        public string Name { get; set; }

        /// <summary>
        /// Описание на категорията
        /// </summary>
        [MaxLength(200, ErrorMessage = "Описанието не може да бъде повече от 200 символа")]
        public string? Description { get; set; }

        /// <summary>
        /// Колекция от въпроси в категорията
        /// </summary>
        public ICollection<Question>? Questions { get; set; }
    }
} 