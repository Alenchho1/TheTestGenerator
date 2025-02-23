using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    /// <summary>
    /// Модел за отговор на въпрос с избираем отговор
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// Уникален идентификатор на отговора
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Съдържание на отговора
        /// </summary>
        [Required(ErrorMessage = "Съдържанието на отговора е задължително")]
        [MinLength(1, ErrorMessage = "Съдържанието не може да бъде празно")]
        public string Content { get; set; }

        /// <summary>
        /// Флаг, указващ дали това е верният отговор
        /// </summary>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Идентификатор на въпроса, към който принадлежи отговорът
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Връзка към въпроса
        /// </summary>
        public Question Question { get; set; }
    }
} 