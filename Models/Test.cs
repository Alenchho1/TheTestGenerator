using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    /// Модел за тест в системата
    public class Test
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието на теста е задължително")]
        [MinLength(5, ErrorMessage = "Заглавието трябва да бъде поне 5 символа")]
        [MaxLength(100, ErrorMessage = "Заглавието не може да бъде повече от 100 символа")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Описанието не може да бъде повече от 500 символа")]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Range(5, 180, ErrorMessage = "Времето трябва да бъде между 5 и 180 минути")]
        public int TimeLimit { get; set; }

        [Range(1, 5, ErrorMessage = "Трудността трябва да бъде между 1 и 5")]
        public int DifficultyLevel { get; set; }

        public int TotalPoints { get; set; }

        [Required(ErrorMessage = "Създателят е задължителен")]
        public string CreatorId { get; set; }

        public ApplicationUser Creator { get; set; }

        public List<TestQuestion> TestQuestions { get; set; }

        public List<TestResult> TestResults { get; set; }
    }

    /// Връзка между тест и въпрос
    public class TestQuestion
    {
        public int TestId { get; set; }
        public Test Test { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int OrderNumber { get; set; }
    }
} 