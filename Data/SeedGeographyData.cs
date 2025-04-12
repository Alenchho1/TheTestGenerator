using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Models;

namespace TestGenerator.Data
{
    public class SeedGeographyData
    {
        public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Добавяне на ролите, ако не съществуват
            if (!await roleManager.RoleExistsAsync("Teacher"))
            {
                await roleManager.CreateAsync(new IdentityRole("Teacher"));
            }
            
            if (!await roleManager.RoleExistsAsync("Student"))
            {
                await roleManager.CreateAsync(new IdentityRole("Student"));
            }
            
            // Създаване на учителски акаунт, ако не съществува
            string teacherEmail = "teacher@testgenerator.com";
            var teacher = await userManager.FindByEmailAsync(teacherEmail);
            
            if (teacher == null)
            {
                teacher = new ApplicationUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    FirstName = "Default",
                    LastName = "Teacher",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(teacher, "Teacher123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher, "Teacher");
                }
            }
            
            string teacherId = teacher.Id;
            
            // Проверка за съществуващата категория География
            var geographyCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "География");
            
            if (geographyCategory == null)
            {
                geographyCategory = new Category
                {
                    Name = "География",
                    Description = "Въпроси за географията на България и света"
                };
                
                await context.Categories.AddAsync(geographyCategory);
                await context.SaveChangesAsync();
            }
            
            int categoryId = geographyCategory.Id;
            
            // Проверка за съществуващи въпроси
            if (!await context.Questions.AnyAsync(q => q.CategoryId == categoryId))
            {
                // Създаване на въпроси
                var questions = new List<Question>
                {
                    new Question
                    {
                        Content = "Коя е столицата на България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 1,
                        Points = 5,
                        CategoryId = categoryId,
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
                        CategoryId = categoryId,
                        CreatorId = teacherId,
                        CreatedAt = DateTime.UtcNow,
                        PossibleAnswers = new List<Answer>
                        {
                            new Answer { Content = "Мусала", IsCorrect = true },
                            new Answer { Content = "Ботев", IsCorrect = false },
                            new Answer { Content = "Вихрен", IsCorrect = false },
                            new Answer { Content = "Черни връх", IsCorrect = false }
                        }
                    },
                    new Question
                    {
                        Content = "Коя е най-дългата река в България?",
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = 2,
                        Points = 10,
                        CategoryId = categoryId,
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
                        CategoryId = categoryId,
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
                        CategoryId = categoryId,
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
                        CategoryId = categoryId,
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
                        CategoryId = categoryId,
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
                
                await context.Questions.AddRangeAsync(questions);
                await context.SaveChangesAsync();
                
                // Създаване на тест с въпроси
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
                
                test.TestQuestions = new List<TestQuestion>();
                for (int i = 0; i < questions.Count; i++)
                {
                    test.TestQuestions.Add(new TestQuestion
                    {
                        Question = questions[i],
                        OrderNumber = i + 1
                    });
                }
                
                await context.Tests.AddAsync(test);
                await context.SaveChangesAsync();
            }
        }
    }
} 