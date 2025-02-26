using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
      /// Модел за въпрос в системата
    public class Question
    {
        /// Уникален идентификатор на въпроса 
        public int Id { get; set; }

          /// Съдържание на въпроса
          
        [Required(ErrorMessage = "Съдържанието на въпроса е задължително")]
        [MinLength(10, ErrorMessage = "Съдържанието трябва да бъде поне 10 символа")]
        public string Content { get; set; }

        
        /// Тип на въпроса (с избираем или отворен отговор)
        
        [Required(ErrorMessage = "Типът на въпроса е задължителен")]
        public QuestionType Type { get; set; }

        
        /// Ниво на трудност от 1 до 5
        
        [Range(1, 5, ErrorMessage = "Трудността трябва да бъде между 1 и 5")]
        public int DifficultyLevel { get; set; }
        
        /// Верен отговор за въпроси със свободен текст
        public string? CorrectAnswer { get; set; }

        /// Възможни отговори за въпроси с избираем отговор
        public List<Answer>? PossibleAnswers { get; set; }

        /// Ключови думи за оценяване на отворени въпроси
        public string? Keywords { get; set; }

        /// Път до изображение, свързано с въпроса
        public string? ImagePath { get; set; }

        /// Точки за верен отговор
        [Range(1, 100, ErrorMessage = "Точките трябва да бъдат между 1 и 100")]
        public int Points { get; set; }

        /// Идентификатор на категорията
        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        /// Категория на въпроса
        public Category Category { get; set; }

        /// Идентификатор на създателя
        [Required(ErrorMessage = "Създателят е задължителен")]
        public string CreatorId { get; set; }

        /// Създател на въпроса
        public ApplicationUser Creator { get; set; }

        /// Дата на създаване
        [Required]
        public DateTime CreatedAt { get; set; }

        /// Дата на последна модификация
        public DateTime? LastModifiedAt { get; set; }
    }

    /// Типове въпроси в системата
    public enum QuestionType
    {
        /// Въпрос с избираем отговор
        MultipleChoice,

        /// Въпрос със свободен отговор
        OpenEnded
    }
} 