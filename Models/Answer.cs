using System.ComponentModel.DataAnnotations;

namespace TestGenerator.Models
{
    
    /// Модел за отговор на въпрос с избираем отговор
    public class Answer
    {
        

        public int Id { get; set; }

        [Required(ErrorMessage = "Съдържанието на отговора е задължително")]
        [MinLength(1, ErrorMessage = "Съдържанието не може да бъде празно")]
        public string Content { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
} 