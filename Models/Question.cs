using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestGenerator.Models
{
    /// Модел за въпрос в системата
    public class Question
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Съдържанието на въпроса е задължително")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Съдържанието трябва да е между 3 и 1000 символа")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "Типът на въпроса е задължителен")]
        public QuestionType Type { get; set; }

        [Range(1, 5, ErrorMessage = "Нивото на трудност трябва да е между 1 и 5")]
        public int DifficultyLevel { get; set; }

        public string? CorrectAnswer { get; set; }

        public List<Answer>? PossibleAnswers { get; set; }

        public string? Keywords { get; set; }

        public string? ImagePath { get; set; }
        [Range(1, 100, ErrorMessage = "Точките трябва да са между 1 и 100")]
        public int Points { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public string CreatorId { get; set; } = null!;

        public ApplicationUser Creator { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedAt { get; set; }
    }

    /// Типове въпроси в системата
    public enum QuestionType
    {
        MultipleChoice,
        OpenEnded
    }
} 