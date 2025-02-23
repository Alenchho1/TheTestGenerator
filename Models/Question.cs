using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    /// <summary>
    /// Модел за въпрос в системата
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Уникален идентификатор на въпроса
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Съдържание на въпроса
        /// </summary>
        [Required(ErrorMessage = "Съдържанието на въпроса е задължително")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Съдържанието трябва да бъде между 10 и 1000 символа")]
        public string Content { get; set; }

        /// <summary>
        /// Тип на въпроса (с избираем отговор или със свободен текст)
        /// </summary>
        [Required(ErrorMessage = "Типът на въпроса е задължителен")]
        public QuestionType Type { get; set; }

        /// <summary>
        /// Ниво на трудност от 1 до 5
        /// </summary>
        [Range(1, 5, ErrorMessage = "Нивото на трудност трябва да бъде между 1 и 5")]
        public int DifficultyLevel { get; set; }

        /// <summary>
        /// Верен отговор за въпроси със свободен текст
        /// </summary>
        public string? CorrectAnswer { get; set; }

        /// <summary>
        /// Възможни отговори за въпроси с избираем отговор
        /// </summary>
        public List<Answer>? PossibleAnswers { get; set; }

        /// <summary>
        /// Ключови думи за оценяване на въпроси със свободен текст
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Път до изображението, свързано с въпроса (ако има такова)
        /// </summary>
        public string? ImagePath { get; set; }

        /// <summary>
        /// Точки, които носи въпросът
        /// </summary>
        [Range(1, 100, ErrorMessage = "Точките трябва да бъдат между 1 и 100")]
        public int Points { get; set; }

        /// <summary>
        /// Идентификатор на категорията
        /// </summary>
        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Категория на въпроса
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Идентификатор на създателя
        /// </summary>
        [Required(ErrorMessage = "Създателят е задължителен")]
        public string CreatorId { get; set; }

        /// <summary>
        /// Създател на въпроса
        /// </summary>
        public ApplicationUser Creator { get; set; }

        /// <summary>
        /// Дата на създаване
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата на последна модификация
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }
    }

    public enum QuestionType
    {
        MultipleChoice,
        OpenEnded
    }
} 