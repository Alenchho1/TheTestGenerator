using TestGenerator.Models;

namespace TestGenerator.Services
{
    public class TestEvaluationService
    {
        // Минимален процент за преминаване на теста
        private const decimal PASS_THRESHOLD = 50.0m;
        
        // Скала за оценяване
        private static readonly Dictionary<decimal, decimal> GradeScale = new()
        {
            { 90, 6.00m }, // Отличен
            { 80, 5.00m }, // Много добър
            { 70, 4.00m }, // Добър
            { 60, 3.50m }, // Среден +
            { 50, 3.00m }, // Среден
            { 0, 2.00m }   // Слаб
        };

        public TestResult EvaluateTest(Test test, List<(int QuestionId, string Answer)> submittedAnswers, DateTime startTime)
        {
            var result = new TestResult
            {
                TestId = test.Id,
                StartTime = startTime,
                EndTime = DateTime.Now,
                IsCompleted = true,
                TotalQuestionsCount = test.TestQuestions.Count,
                AnswerResults = new List<TestAnswerResult>()
            };

            foreach (var testQuestion in test.TestQuestions)
            {
                var question = testQuestion.Question;
                var submittedAnswer = submittedAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
                var answerResult = EvaluateQuestion(question, submittedAnswer.Answer);
                result.AnswerResults.Add(answerResult);
            }

            // Изчисляване на общите резултати
            result.Score = result.AnswerResults.Sum(ar => ar.Points);
            result.MaxScore = test.TotalPoints;
            result.CorrectAnswersCount = result.AnswerResults.Count(ar => ar.IsCorrect);
            result.PercentageScore = (decimal)result.Score / result.MaxScore * 100;
            result.Grade = CalculateGrade(result.PercentageScore);

            // Генериране на обратна връзка
            result.Feedback = GenerateFeedback(result);

            return result;
        }

        private TestAnswerResult EvaluateQuestion(Question question, string submittedAnswer)
        {
            var result = new TestAnswerResult
            {
                QuestionId = question.Id,
                SubmittedAnswer = submittedAnswer,
                Points = 0,
                FeedbackNotes = string.Empty // Инициализиране с празен стринг
            };

            if (question.Type == QuestionType.MultipleChoice)
            {
                result = EvaluateMultipleChoiceQuestion(question, submittedAnswer, result);
            }
            else
            {
                result = EvaluateOpenEndedQuestion(question, submittedAnswer, result);
            }

            return result;
        }

        private TestAnswerResult EvaluateMultipleChoiceQuestion(Question question, string submittedAnswer, TestAnswerResult result)
        {
            if (int.TryParse(submittedAnswer, out int selectedAnswerId))
            {
                result.SelectedAnswerId = selectedAnswerId;
                var correctAnswer = question.PossibleAnswers.FirstOrDefault(a => a.IsCorrect);
                result.IsCorrect = correctAnswer?.Id == selectedAnswerId;
                result.Points = result.IsCorrect ? question.Points : 0;
                
                // Добавяне на обратна връзка за въпроси с множествен избор
                result.FeedbackNotes = result.IsCorrect 
                    ? "Правилен отговор!" 
                    : "Грешен отговор. Моля, прегледайте материала отново.";
            }
            else
            {
                result.FeedbackNotes = "Невалиден отговор.";
            }

            return result;
        }

        private TestAnswerResult EvaluateOpenEndedQuestion(Question question, string submittedAnswer, TestAnswerResult result)
        {
            if (string.IsNullOrEmpty(submittedAnswer))
            {
                result.FeedbackNotes = "Не е предоставен отговор.";
                return result;
            }

            // Нормализиране на отговорите (премахване на излишни интервали, малки букви)
            var normalizedSubmittedAnswer = submittedAnswer.Trim().ToLower();
            var normalizedCorrectAnswer = question.CorrectAnswer?.Trim().ToLower() ?? "";
            
            // Разделяне на ключовите думи и премахване на празните
            var keywords = (question.Keywords?.Split(',')
                .Select(k => k.Trim().ToLower())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList()) ?? new List<string>();

            // Проверка за точно съвпадение
            if (normalizedSubmittedAnswer == normalizedCorrectAnswer)
            {
                result.Points = question.Points;
                result.IsCorrect = true;
                result.KeywordMatchPercentage = 100;
                result.FeedbackNotes = "Перфектен отговор! Напълно съвпада с очаквания отговор.";
                return result;
            }

            // Изчисляване на съвпадение на ключови думи
            int matchedKeywords = 0;
            foreach (var keyword in keywords)
            {
                if (normalizedSubmittedAnswer.Contains(keyword))
                {
                    matchedKeywords++;
                }
            }

            // Изчисляване на процента съвпадение
            result.KeywordMatchPercentage = keywords.Any() 
                ? (decimal)matchedKeywords / keywords.Count * 100 
                : 0;

            // Определяне на точките базирано на процента съвпадение
            if (result.KeywordMatchPercentage >= 90)
            {
                result.Points = question.Points;
                result.IsCorrect = true;
            }
            else if (result.KeywordMatchPercentage >= 75)
            {
                result.Points = (int)(question.Points * 0.75);
                result.IsCorrect = true;
            }
            else if (result.KeywordMatchPercentage >= 50)
            {
                result.Points = (int)(question.Points * 0.5);
                result.IsCorrect = false;
            }
            else if (result.KeywordMatchPercentage >= 25)
            {
                result.Points = (int)(question.Points * 0.25);
                result.IsCorrect = false;
            }

            // Добавяне на подробна обратна връзка
            result.FeedbackNotes = GenerateDetailedOpenEndedFeedback(
                result.KeywordMatchPercentage,
                matchedKeywords,
                keywords.Count,
                question.Points,
                result.Points
            );

            return result;
        }

        private decimal CalculateGrade(decimal percentageScore)
        {
            foreach (var grade in GradeScale.OrderByDescending(g => g.Key))
            {
                if (percentageScore >= grade.Key)
                {
                    return grade.Value;
                }
            }
            return 2.00m; // Минимална оценка
        }

        private string GenerateFeedback(TestResult result)
        {
            var feedback = new System.Text.StringBuilder();

            if (result.PercentageScore >= PASS_THRESHOLD)
            {
                feedback.AppendLine($"Поздравления! Вие преминахте теста успешно с оценка {result.Grade:F2}.");
            }
            else
            {
                feedback.AppendLine($"За съжаление не успяхте да преминете теста. Вашата оценка е {result.Grade:F2}.");
            }

            feedback.AppendLine($"Верни отговори: {result.CorrectAnswersCount} от {result.TotalQuestionsCount}");
            feedback.AppendLine($"Общ брой точки: {result.Score} от възможни {result.MaxScore}");
            feedback.AppendLine($"Процент успеваемост: {result.PercentageScore:F2}%");

            return feedback.ToString();
        }

        private string GenerateDetailedOpenEndedFeedback(decimal matchPercentage, int matchedCount, int totalKeywords, int maxPoints, int earnedPoints)
        {
            var feedback = new System.Text.StringBuilder();

            if (matchPercentage >= 90)
            {
                feedback.AppendLine("Отличен отговор! Включени са почти всички ключови концепции.");
            }
            else if (matchPercentage >= 75)
            {
                feedback.AppendLine("Много добър отговор! Включени са повечето ключови концепции.");
            }
            else if (matchPercentage >= 50)
            {
                feedback.AppendLine("Добър отговор, но има какво да се подобри. Включени са някои от ключовите концепции.");
            }
            else if (matchPercentage >= 25)
            {
                feedback.AppendLine("Частичен отговор. Открити са малко от ключовите концепции.");
            }
            else
            {
                feedback.AppendLine("Отговорът се нуждае от подобрение. Не са открити достатъчно ключови концепции.");
            }

            feedback.AppendLine($"Намерени ключови концепции: {matchedCount} от {totalKeywords}");
            feedback.AppendLine($"Получени точки: {earnedPoints} от {maxPoints}");

            return feedback.ToString();
        }
    }
} 