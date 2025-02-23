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
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Съдържанието трябва да бъде между 1 и 500 символа")]
        public string Content { get; set; }

        /// <summary>
        /// Флаг, указващ дали това е правилният отговор
        /// </summary>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Идентификатор на въпроса, към който принадлежи отговорът
        /// </summary>
        [Required(ErrorMessage = "Въпросът е задължителен")]
        public int QuestionId { get; set; }

        /// <summary>
        /// Връзка към въпроса
        /// </summary>
        public Question Question { get; set; }

        /// <summary>
        /// Пореден номер на отговора в списъка с отговори
        /// </summary>
        public int OrderNumber { get; set; }
    }
} 