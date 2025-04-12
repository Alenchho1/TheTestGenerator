using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;

namespace TestGenerator.Services
{
    public class DataInitializationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DataInitializationService> _logger;

        public DataInitializationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DataInitializationService> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Създаване на роли в системата
                await CreateRolesAsync();

                // Създаване на примерен учителски акаунт
                var teacherId = await CreateDefaultTeacherAsync();

                if (!string.IsNullOrEmpty(teacherId))
                {
                    // Добавяне на примерни въпроси по география
                    await AddGeographyQuestionsAsync(teacherId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during data initialization");
                throw;
            }
        }

        private async Task CreateRolesAsync()
        {
            var roles = new[] { "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    _logger.LogInformation($"Created role: {role}");
                }
            }
        }

        private async Task<string> CreateDefaultTeacherAsync()
        {
            var teacherEmail = "teacher@testgenerator.com";
            var teacherUser = await _userManager.FindByEmailAsync(teacherEmail);

            if (teacherUser == null)
            {
                teacherUser = new ApplicationUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FirstName = "Default",
                    LastName = "Teacher",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(teacherUser, "Teacher123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(teacherUser, "Teacher");
                    _logger.LogInformation("Created default teacher account");
                    return teacherUser.Id;
                }
                else
                {
                    _logger.LogError("Failed to create default teacher: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return null;
                }
            }

            return teacherUser.Id;
        }

        private async Task AddGeographyQuestionsAsync(string teacherId)
        {
            if (!await _context.Questions.AnyAsync(q => q.CategoryId == 1))
            {
                var questions = new List<Question>
                {
                    new Question
                    {
                        Content = "Коя е столицата на България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 1,
                        Points = 5,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "София", IsCorrect = true },
                            new Answer { Content = "Пловдив", IsCorrect = false },
                            new Answer { Content = "Варна", IsCorrect = false },
                            new Answer { Content = "Бургас", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Content = "Кой е най-високият връх в България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 1,
                        Points = 5,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "Вихрен", IsCorrect = false },
                            new Answer { Content = "Мусала", IsCorrect = true },
                            new Answer { Content = "Ботев", IsCorrect = false },
                            new Answer { Content = "Черни връх", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Content = "Коя е най-дългата река в България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 2,
                        Points = 10,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "Дунав", IsCorrect = true },
                            new Answer { Content = "Марица", IsCorrect = false },
                            new Answer { Content = "Искър", IsCorrect = false },
                            new Answer { Content = "Струма", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Content = "Кое море мие бреговете на България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 1,
                        Points = 5,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "Черно море", IsCorrect = true },
                            new Answer { Content = "Егейско море", IsCorrect = false },
                            new Answer { Content = "Средиземно море", IsCorrect = false },
                            new Answer { Content = "Адриатическо море", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Content = "Колко области има в България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 3,
                        Points = 15,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "28", IsCorrect = true },
                            new Answer { Content = "25", IsCorrect = false },
                            new Answer { Content = "30", IsCorrect = false },
                            new Answer { Content = "27", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Content = "Разгледайте снимката и изброете имената на трите най-големи езера от Седемте рилски езера:",
                        Type = QuestionType.OpenEnded,
                        DifficultyLevel = 2,
                        Points = 10,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        ImagePath = "/question-images/seven-rila-lakes.jpg",
                        CorrectAnswer = "Окото, Бъбрека, Близнака",
                        Keywords = "Окото,Бъбрека,Близнака,езеро,Рила,Седемте рилски езера",
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "Окото, Бъбрека, Близнака", IsCorrect = true }
                        }
                    },
                    new Question
                    {
                        Content = "Разгледайте снимката и опишете къде се намира и с какво е известен язовир Искър:",
                        Type = QuestionType.OpenEnded,
                        DifficultyLevel = 2,
                        Points = 10,
                        CategoryId = 1,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        ImagePath = "/question-images/iskar-dam.jpg",
                        CorrectAnswer = "Язовир Искър се намира в Софийска област и е най-старият и най-големият изкуствен водоем в България. Той е основен източник на питейна вода за София.",
                        Keywords = "Софийска област,най-стар,най-голям,изкуствен водоем,питейна вода,София",
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "Язовир Искър се намира в Софийска област и е най-старият и най-големият изкуствен водоем в България. Той е основен източник на питейна вода за София.", IsCorrect = true }
                        }
                    }
                };

                await _context.Questions.AddRangeAsync(questions);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added geography questions");

                // Създаване на примерен тест с въпросите
                var test = new Test
                {
                    Title = "Тест по география на България",
                    Description = "Основни въпроси за географията на България, включващ въпроси с избираем отговор и въпроси със свободен отговор и снимки",
                    CreatedAt = DateTime.UtcNow,
                    TimeLimit = 30,  
                    DifficultyLevel = 2,
                    CreatorId = teacherId,
                    TotalPoints = questions.Sum(q => q.Points)
                };

                // Добавяме всички въпроси към теста
                test.TestQuestions = new List<TestQuestion>();
                for (int i = 0; i < questions.Count; i++)
                {
                    test.TestQuestions.Add(new TestQuestion
                    {
                        Question = questions[i],
                        OrderNumber = i + 1
                    });
                }

                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added default geography test with all questions");
            }
        }
    }
} 