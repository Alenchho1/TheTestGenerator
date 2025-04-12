using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.Services;
using TestGenerator.ViewModels;
using Microsoft.Extensions.Logging;

namespace TestGenerator.Controllers
{
    [Authorize]
    public class TestController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TestEvaluationService _evaluationService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            TestEvaluationService evaluationService,
            ILogger<TestController> logger) 
            : base(context)
        {
            _userManager = userManager;
            _evaluationService = evaluationService;
            _logger = logger;
        }

        private async Task<bool> UserCanAccessTest(int testId, string userId)
        {
            var test = await _context.Tests.FindAsync(testId);
            if (test == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Учител има достъп до всички тестове
            if (User.IsInRole("Teacher"))
            {
                return true;
            }

            // Ученик има достъп само до тестове, които не са създадени от него
            return test.CreatorId != userId;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Търсене на тестове за потребител: {currentUser.Id}");

                var isTeacher = await _userManager.IsInRoleAsync(currentUser, "Teacher");

                IQueryable<Test> testsQuery = _context.Tests
                    .Include(t => t.TestQuestions)
                    .Include(t => t.Creator);

                // Учител вижда всички тестове
                if (isTeacher)
                {
                    testsQuery = testsQuery.OrderByDescending(t => t.CreatedAt);
                }
                // Ученик вижда тестове, които не са създадени от него
                else
                {
                    testsQuery = testsQuery
                        .Where(t => t.CreatorId != currentUser.Id)
                        .OrderByDescending(t => t.CreatedAt);
                }

                var tests = await testsQuery.ToListAsync();
                _logger.LogInformation($"Намерени {tests.Count} теста за потребителя");

                return View(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при търсене на тестове");
                SetAlert("Възникна грешка при зареждането на тестовете.", "danger");
                return View(new List<Test>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.Category)
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (!await UserCanAccessTest(test.Id, currentUser.Id))
            {
                return Forbid();
            }

            return View(test);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Questions)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    QuestionCount = c.Questions.Count()
                })
                .ToListAsync();

            var model = new TestGenerationViewModel
            {
                TimeLimit = 60,
                NumberOfQuestions = 10
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(TestGenerationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareCreateViewData();
                return View(model);
            }

            try
            {
                var questions = await GetQuestionsForTest(model);

                if (!questions.Any())
                {
                    ModelState.AddModelError("", "Няма намерени въпроси в избраната категория.");
                    await PrepareCreateViewData();
                    return View(model);
                }

                if (questions.Count < model.NumberOfQuestions)
                {
                    ModelState.AddModelError("", $"Недостатъчен брой въпроси в избраната категория. Налични са само {questions.Count} въпроса.");
                    await PrepareCreateViewData();
                    return View(model);
                }

                var averageDifficulty = (int)Math.Round(questions.Average(q => q.DifficultyLevel));

                var test = new Test
                {
                    Title = model.Title,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    TimeLimit = model.TimeLimit,
                    DifficultyLevel = averageDifficulty,
                    CreatorId = _userManager.GetUserId(User),
                    TotalPoints = questions.Sum(q => q.Points),
                    TestQuestions = questions.Select((q, index) => new TestQuestion
                    {
                        Question = q,
                        OrderNumber = index + 1
                    }).ToList()
                };

                _context.Tests.Add(test);
                await _context.SaveChangesAsync();

                SetAlert("Тестът беше създаден успешно", "success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при създаването на теста");
                SetAlert("Възникна грешка при създаването на теста", "danger");
                await PrepareCreateViewData();
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var test = await _context.Tests
                    .Include(t => t.TestQuestions)
                    .Include(t => t.TestResults)
                        .ThenInclude(tr => tr.AnswerResults)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (test == null)
                {
                    return NotFound();
                }

                // Изтриване на свързаните данни
                if (test.TestResults != null && test.TestResults.Any())
                {
                    foreach (var result in test.TestResults)
                    {
                        if (result.AnswerResults != null)
                        {
                            _context.TestAnswerResults.RemoveRange(result.AnswerResults);
                        }
                    }
                    _context.TestResults.RemoveRange(test.TestResults);
                }

                // Изтриване на тестовите въпроси
                if (test.TestQuestions != null)
                {
                    _context.TestQuestions.RemoveRange(test.TestQuestions);
                }

                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();

                SetAlert("Тестът беше изтрит успешно", "success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при изтриване на теста");
                SetAlert("Грешка при изтриване на теста", "danger");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Question>> GetQuestionsForTest(TestGenerationViewModel model)
        {
            var questions = await _context.Questions
                .Where(q => q.CategoryId == model.CategoryId)
                .OrderBy(r => Guid.NewGuid())
                .Take(model.NumberOfQuestions)
                .ToListAsync();

            _logger.LogInformation($"Намерени {questions.Count} въпроса за категория {model.CategoryId}");
            return questions;
        }

        public async Task<IActionResult> Take(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (!await UserCanAccessTest(id.Value, currentUser.Id))
            {
                SetAlert("Нямате достъп до този тест.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            var viewModel = new TestSubmissionViewModel
            {
                TestId = test.Id,
                Title = test.Title,
                TimeLimit = test.TimeLimit,
                StartTime = DateTime.Now,
                Questions = test.TestQuestions
                    .OrderBy(tq => tq.OrderNumber)
                    .Select(tq => new QuestionSubmissionViewModel
                    {
                        QuestionId = tq.Question.Id,
                        Content = tq.Question.Content,
                        Type = tq.Question.Type.ToString(),
                        ImagePath = tq.Question.ImagePath,
                        PossibleAnswers = tq.Question.Type == QuestionType.MultipleChoice
                            ? tq.Question.PossibleAnswers.Select(a => new AnswerSubmissionViewModel
                            {
                                Id = a.Id,
                                Content = a.Content
                            }).ToList()
                            : null
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TestSubmissionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload test data for the view
                    var testData = await _context.Tests
                        .Include(t => t.TestQuestions)
                            .ThenInclude(tq => tq.Question)
                                .ThenInclude(q => q.PossibleAnswers)
                        .FirstOrDefaultAsync(t => t.Id == model.TestId);

                    if (testData != null)
                    {
                        // Update the model with the reloaded data while preserving submitted answers
                        foreach (var question in model.Questions)
                        {
                            var testQuestion = testData.TestQuestions
                                .FirstOrDefault(tq => tq.Question.Id == question.QuestionId);
                            if (testQuestion != null)
                            {
                                question.Content = testQuestion.Question.Content;
                                question.Type = testQuestion.Question.Type.ToString();
                                question.ImagePath = testQuestion.Question.ImagePath;
                                question.PossibleAnswers = testQuestion.Question.Type == QuestionType.MultipleChoice
                                    ? testQuestion.Question.PossibleAnswers.Select(a => new AnswerSubmissionViewModel
                                    {
                                        Id = a.Id,
                                        Content = a.Content
                                    }).ToList()
                                    : null;
                            }
                        }
                    }

                    SetAlert("Моля, попълнете всички задължителни полета.", "danger");
                    return View("Take", model);
                }

                var test = await _context.Tests
                    .Include(t => t.TestQuestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.PossibleAnswers)
                    .Include(t => t.TestQuestions)
                        .ThenInclude(tq => tq.Question)
                            .ThenInclude(q => q.Category)
                    .FirstOrDefaultAsync(t => t.Id == model.TestId);

                if (test == null)
                {
                    SetAlert("Тестът не беше намерен.", "danger");
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var submittedAnswers = model.Questions
                    .Select(q => (q.QuestionId, q.SubmittedAnswer))
                    .ToList();

                var testResult = _evaluationService.EvaluateTest(test, submittedAnswers, model.StartTime);
                testResult.UserId = currentUser.Id;
                testResult.EndTime = DateTime.Now;

                _context.TestResults.Add(testResult);
                await _context.SaveChangesAsync();

                // Презареждане на данните за теста
                var reloadedTest = await _context.Tests
                    .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                    .FirstOrDefaultAsync(t => t.Id == testResult.TestId);

                // Обновяване на модела с презаредените данни
                foreach (var answer in testResult.AnswerResults)
                {
                    var question = reloadedTest.TestQuestions
                        .Select(tq => tq.Question)
                        .FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question != null)
                    {
                        answer.Question = question;
                    }
                }

                SetAlert("Тестът беше успешно предаден.", "success");
                return RedirectToAction(nameof(Result), new { id = testResult.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при предаване на теста");
                SetAlert($"Възникна грешка при предаването на теста: {ex.Message}", "danger");
                return View("Take", model);
            }
        }

        public async Task<IActionResult> Result(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testResult = await _context.TestResults
                .Include(tr => tr.Test)
                    .ThenInclude(t => t.TestQuestions)
                .Include(tr => tr.AnswerResults)
                    .ThenInclude(ar => ar.Question)
                        .ThenInclude(q => q.Category)
                .Include(tr => tr.AnswerResults)
                    .ThenInclude(ar => ar.Question)
                        .ThenInclude(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(tr => tr.Id == id);

            if (testResult == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            // Позволяваме достъп на учители или на потребителя, който е направил теста
            if (testResult.UserId != currentUser.Id && !User.IsInRole("Teacher"))
            {
                return Forbid();
            }

            // Подреждаме отговорите според OrderNumber на въпросите в теста
            var orderedAnswers = testResult.AnswerResults
                .Join(testResult.Test.TestQuestions,
                    ar => ar.QuestionId,
                    tq => tq.QuestionId,
                    (ar, tq) => new { Answer = ar, OrderNumber = tq.OrderNumber })
                .OrderBy(x => x.OrderNumber)
                .Select(x => x.Answer)
                .ToList();

            testResult.AnswerResults = orderedAnswers;

            return View(testResult);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ViewResults(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.TestResults)
                    .ThenInclude(tr => tr.User)
                .Include(t => t.TestResults)
                    .ThenInclude(tr => tr.AnswerResults)
                        .ThenInclude(ar => ar.Question)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ClearAllData()
        {
            try
            {
                // Изтриване на всички тестови резултати
                var testResults = await _context.TestResults.ToListAsync();
                _context.TestResults.RemoveRange(testResults);

                // Изтриване на всички тестови въпроси
                var testQuestions = await _context.TestQuestions.ToListAsync();
                _context.TestQuestions.RemoveRange(testQuestions);

                // Изтриване на всички тестове
                var tests = await _context.Tests.ToListAsync();
                _context.Tests.RemoveRange(tests);

                // Изтриване на всички отговори
                var answers = await _context.Answers.ToListAsync();
                _context.Answers.RemoveRange(answers);

                // Изтриване на всички въпроси
                var questions = await _context.Questions.ToListAsync();
                _context.Questions.RemoveRange(questions);

                // Изтриване на всички категории
                var categories = await _context.Categories.ToListAsync();
                _context.Categories.RemoveRange(categories);

                await _context.SaveChangesAsync();

                SetAlert("Всички данни бяха успешно изтрити", "success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при изтриване на данните");
                SetAlert("Възникна грешка при изтриване на данните", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task PrepareCreateViewData()
        {
            var categories = await _context.Categories
                .Include(c => c.Questions)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    QuestionCount = c.Questions.Count()
                })
                .ToListAsync();

            ViewBag.Categories = categories;
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            // Проверяваме дали текущият потребител е създател на теста
            var currentUser = await _userManager.GetUserAsync(User);
            if (test.CreatorId != currentUser.Id)
            {
                return Forbid();
            }

            var model = new TestEditViewModel
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                TimeLimit = test.TimeLimit,
                DifficultyLevel = test.DifficultyLevel
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, TestEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var test = await _context.Tests.FindAsync(id);
                    if (test == null)
                    {
                        return NotFound();
                    }

                    // Проверяваме дали текущият потребител е създател на теста
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (test.CreatorId != currentUser.Id)
                    {
                        return Forbid();
                    }

                    // Обновяваме само позволените полета
                    test.Title = model.Title;
                    test.Description = model.Description;
                    test.TimeLimit = model.TimeLimit;
                    test.DifficultyLevel = model.DifficultyLevel;

                    _context.Update(test);
                    await _context.SaveChangesAsync();

                    SetAlert("Тестът беше успешно редактиран", "success");
                    return RedirectToAction(nameof(Details), new { id = test.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(t => t.Id == id);
        }
    }
} 