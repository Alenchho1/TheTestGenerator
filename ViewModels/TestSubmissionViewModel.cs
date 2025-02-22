using System.ComponentModel.DataAnnotations;

namespace TestGenerator.ViewModels
{
    public class TestSubmissionViewModel
    {
        [Required(ErrorMessage = "Идентификаторът на теста е задължителен")]
        public int TestId { get; set; }
        
        public string Title { get; set; }
        
        public List<QuestionSubmissionViewModel> Questions { get; set; } = new List<QuestionSubmissionViewModel>();
        
        public DateTime StartTime { get; set; }
        
        public int TimeLimit { get; set; }
    }

    public class QuestionSubmissionViewModel
    {
        [Required(ErrorMessage = "Идентификаторът на въпроса е задължителен")]
        public int QuestionId { get; set; }
        
        public string Content { get; set; }
        
        [Required(ErrorMessage = "Типът на въпроса е задължителен")]
        public string Type { get; set; }
        
        public string? ImagePath { get; set; }
        
        public List<AnswerSubmissionViewModel>? PossibleAnswers { get; set; }
        
        [Required(ErrorMessage = "Отговорът е задължителен")]
        public string SubmittedAnswer { get; set; }
    }

    public class AnswerSubmissionViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }
} 