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
            { 90m, 6.00m }, // Отличен
            { 80m, 5.00m }, // Много добър
            { 70m, 4.00m }, // Добър
            { 60m, 3.50m }, // Среден +
            { 50m, 3.00m }, // Среден
            { 0m, 2.00m }   // Слаб
        };

        public TestResult EvaluateTest(Test test, List<(int QuestionId, string SubmittedAnswer)> submittedAnswers, DateTime startTime)
        {
            var testResult = new TestResult
            {
                TestId = test.Id,
                StartTime = startTime,
                EndTime = DateTime.Now,
                MaxScore = test.TotalPoints
            };

            var answerResults = new List<TestAnswerResult>();
            int earnedPoints = 0;

            foreach (var testQuestion in test.TestQuestions)
            {
                var question = testQuestion.Question;
                var submission = submittedAnswers.FirstOrDefault(sa => sa.QuestionId == question.Id);
                var submittedAnswer = submission.SubmittedAnswer ?? "";
                var isCorrect = false;
                var points = 0;
                decimal keywordMatchPercentage = 0;

                if (question.Type == QuestionType.MultipleChoice)
                {
                    var correctAnswer = question.PossibleAnswers?.FirstOrDefault(a => a.IsCorrect);
                    var selectedAnswer = question.PossibleAnswers?.FirstOrDefault(a => a.Id.ToString() == submittedAnswer);
                    isCorrect = correctAnswer != null && selectedAnswer != null && correctAnswer.Id == selectedAnswer.Id;
                    submittedAnswer = selectedAnswer?.Content ?? submittedAnswer;
                    points = isCorrect ? question.Points : 0;
                }
                else // OpenEnded
                {
                    // Проверка за точно съвпадение
                    if (!string.IsNullOrEmpty(question.CorrectAnswer))
                    {
                        isCorrect = submittedAnswer.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                    }

                    // Ако няма точно съвпадение, проверяваме ключовите думи
                    if (!isCorrect && !string.IsNullOrEmpty(question.Keywords))
                    {
                        var keywords = question.Keywords.Split(',').Select(k => k.Trim().ToLower()).ToList();
                        var submittedWords = submittedAnswer.ToLower().Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                        
                        int matchedKeywords = keywords.Count(keyword => 
                            submittedWords.Any(word => word.Contains(keyword) || keyword.Contains(word)));
                        
                        keywordMatchPercentage = Math.Round((decimal)matchedKeywords / keywords.Count * 100, 2);

                        // Определяне на точките според процента съвпадение
                        if (keywordMatchPercentage >= 90)
                        {
                            points = question.Points;
                            isCorrect = true;
                        }
                        else if (keywordMatchPercentage >= 75)
                        {
                            points = (int)(question.Points * 0.75m);
                        }
                        else if (keywordMatchPercentage >= 50)
                        {
                            points = (int)(question.Points * 0.50m);
                        }
                        else if (keywordMatchPercentage >= 25)
                        {
                            points = (int)(question.Points * 0.25m);
                        }
                    }
                    else if (isCorrect)
                    {
                        points = question.Points;
                    }
                }

                earnedPoints += points;

                answerResults.Add(new TestAnswerResult
                {
                    QuestionId = question.Id,
                    SubmittedAnswer = submittedAnswer,
                    IsCorrect = isCorrect,
                    Points = points,
                    KeywordMatchPercentage = keywordMatchPercentage,
                    FeedbackNotes = GenerateFeedbackNotes(isCorrect, points, question.Points, keywordMatchPercentage)
                });
            }

            testResult.AnswerResults = answerResults;
            
            decimal percentageScore = test.TotalPoints > 0 
                ? Math.Round((decimal)earnedPoints / test.TotalPoints * 100, 2)
                : 0;

            testResult.Score = percentageScore;
            testResult.PercentageScore = percentageScore;
            testResult.Grade = CalculateGrade(percentageScore);
            testResult.Feedback = GenerateFeedback(testResult);

            return testResult;
        }

        private string GenerateFeedbackNotes(bool isCorrect, int earnedPoints, int totalPoints, decimal keywordMatchPercentage)
        {
            if (isCorrect)
            {
                return "Правилен отговор!";
            }
            else if (keywordMatchPercentage > 0)
            {
                return $"Частично верен отговор. Съвпадение с ключови думи: {keywordMatchPercentage}%. Получени точки: {earnedPoints} от {totalPoints}.";
            }
            else
            {
                return "Грешен отговор.";
            }
        }

        private decimal CalculateGrade(decimal percentageScore)
        {
            // Проверяваме дали процентът е в съответния диапазон и връщаме съответната оценка
            if (percentageScore >= 90) return 6.00m;
            if (percentageScore >= 80) return 5.00m;
            if (percentageScore >= 70) return 4.00m;
            if (percentageScore >= 60) return 3.50m;
            if (percentageScore >= 50) return 3.00m;
            return 2.00m;
        }

        private string GenerateFeedback(TestResult result)
        {
            var gradeText = result.Grade >= 5.50m ? "Отличен" :
                           result.Grade >= 4.50m ? "Много добър" :
                           result.Grade >= 3.50m ? "Добър" :
                           result.Grade >= 3.00m ? "Среден" : "Слаб";

            return $"{(result.PercentageScore >= PASS_THRESHOLD ? "Поздравления! Вие преминахте теста успешно" : "За съжаление не успяхте да преминете теста")} " +
                   $"с оценка {result.Grade:F2} ({gradeText}).\n" +
                   $"Верни отговори: {result.CorrectAnswers} от {result.TotalQuestions}\n" +
                   $"Процент успеваемост: {result.PercentageScore:F2}%\n" +
                   $"Получени точки: {result.AnswerResults.Sum(ar => ar.Points)} от {result.MaxScore}";
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