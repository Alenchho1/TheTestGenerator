using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using TestGenerator.Models;

namespace TestGenerator.ViewModels
{
    public class QuestionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Съдържанието на въпроса е задължително")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Типът на въпроса е задължителен")]
        public QuestionType Type { get; set; }

        [Range(1, 5, ErrorMessage = "Нивото на трудност трябва да бъде между 1 и 5")]
        public int DifficultyLevel { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        [Range(1, 100, ErrorMessage = "Точките трябва да бъдат между 1 и 100")]
        public int Points { get; set; }

        public string? Keywords { get; set; }

        [RequiredIf("Type", QuestionType.OpenEnded, ErrorMessage = "Верният отговор е задължителен за въпрос със свободен отговор")]
        public string? CorrectAnswer { get; set; }

        public IFormFile? Image { get; set; }

        public string? ExistingImagePath { get; set; }

        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    }

    public class AnswerViewModel
    {
        [Required(ErrorMessage = "Съдържанието на отговора е задължително")]
        public string Content { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly object _desiredValue;

        public RequiredIfAttribute(string propertyName, object desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var propertyValue = instance.GetType().GetProperty(_propertyName).GetValue(instance, null);

            if (propertyValue.Equals(_desiredValue) && (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
} 