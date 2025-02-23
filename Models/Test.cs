using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    /// <summary>
    /// Модел за тест в системата
    /// </summary>
    public class Test
    {
        /// <summary>
        /// Уникален идентификатор на теста
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Заглавие на теста
        /// </summary>
        [Required(ErrorMessage = "Заглавието на теста е задължително")]
        [MinLength(5, ErrorMessage = "Заглавието трябва да бъде поне 5 символа")]
        [MaxLength(100, ErrorMessage = "Заглавието не може да бъде повече от 100 символа")]
        public string Title { get; set; }

        /// <summary>
        /// Описание на теста
        /// </summary>
        [MaxLength(500, ErrorMessage = "Описанието не може да бъде повече от 500 символа")]
        public string? Description { get; set; }

        /// <summary>
        /// Дата на създаване на теста
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Времево ограничение за решаване в минути
        /// </summary>
        [Range(5, 180, ErrorMessage = "Времето трябва да бъде между 5 и 180 минути")]
        public int TimeLimit { get; set; }

        /// <summary>
        /// Ниво на трудност на теста (1-5)
        /// </summary>
        [Range(1, 5, ErrorMessage = "Трудността трябва да бъде между 1 и 5")]
        public int DifficultyLevel { get; set; }

        /// <summary>
        /// Общ брой точки за теста
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// Идентификатор на създателя
        /// </summary>
        [Required(ErrorMessage = "Създателят е задължителен")]
        public string CreatorId { get; set; }

        /// <summary>
        /// Създател на теста
        /// </summary>
        public ApplicationUser Creator { get; set; }

        /// <summary>
        /// Въпроси в теста
        /// </summary>
        public List<TestQuestion> TestQuestions { get; set; }

        /// <summary>
        /// Резултати от решаването на теста
        /// </summary>
        public List<TestResult> TestResults { get; set; }
    }

    /// <summary>
    /// Връзка между тест и въпрос
    /// </summary>
    public class TestQuestion
    {
        /// <summary>
        /// Идентификатор на теста
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Тест
        /// </summary>
        public Test Test { get; set; }

        /// <summary>
        /// Идентификатор на въпроса
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Въпрос
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// Пореден номер на въпроса в теста
        /// </summary>
        public int OrderNumber { get; set; }
    }
} 