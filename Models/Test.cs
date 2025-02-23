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
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Заглавието трябва да бъде между 5 и 200 символа")]
        public string Title { get; set; }

        /// <summary>
        /// Описание на теста
        /// </summary>
        [StringLength(1000, ErrorMessage = "Описанието не може да бъде по-дълго от 1000 символа")]
        public string? Description { get; set; }

        /// <summary>
        /// Дата и час на създаване на теста
        /// </summary>
        [Required(ErrorMessage = "Датата на създаване е задължителна")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Времево ограничение за решаване на теста в минути
        /// </summary>
        [Range(5, 180, ErrorMessage = "Времевото ограничение трябва да бъде между 5 и 180 минути")]
        public int TimeLimit { get; set; }

        /// <summary>
        /// Ниво на трудност на теста от 1 до 5
        /// </summary>
        [Range(1, 5, ErrorMessage = "Нивото на трудност трябва да бъде между 1 и 5")]
        public int DifficultyLevel { get; set; }

        /// <summary>
        /// Общ брой точки за теста
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// Идентификатор на създателя на теста
        /// </summary>
        [Required(ErrorMessage = "Създателят е задължителен")]
        public string CreatorId { get; set; }

        /// <summary>
        /// Създател на теста
        /// </summary>
        public ApplicationUser Creator { get; set; }

        /// <summary>
        /// Колекция от въпроси в теста
        /// </summary>
        public ICollection<TestQuestion> TestQuestions { get; set; }
    }

    /// <summary>
    /// Модел за връзка между тест и въпрос
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