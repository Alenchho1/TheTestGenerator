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
            return test.CreatorId == userId;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Fetching tests for user: {currentUser.Id}");

                var tests = await _context.Tests
                    .Include(t => t.TestQuestions)
                    .Include(t => t.Creator)
                    .Where(t => t.CreatorId == currentUser.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Found {tests.Count} tests for user");

                return View(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tests");
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

        [Authorize(Roles = "Admin,Teacher")]
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
        [Authorize(Roles = "Admin,Teacher")]
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
                    _logger.LogWarning($"No questions found for category {model.CategoryId}");
                    ModelState.AddModelError("", "Няма намерени въпроси в избраната категория.");
                    await PrepareCreateViewData();
                    return View(model);
                }

                if (questions.Count < model.NumberOfQuestions)
                {
                    _logger.LogWarning($"Not enough questions found. Requested: {model.NumberOfQuestions}, Found: {questions.Count}");
                    ModelState.AddModelError("", $"Недостатъчен брой въпроси в избраната категория. Налични са само {questions.Count} въпроса.");
                    await PrepareCreateViewData();
                    return View(model);
                }

                // Calculate average difficulty
                var averageDifficulty = (int)Math.Round(questions.Average(q => q.DifficultyLevel));
                _logger.LogInformation($"Calculated average difficulty: {averageDifficulty}");

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

                _logger.LogInformation($"Test created successfully with ID: {test.Id}");
                SetAlert("Тестът беше създаден успешно", "success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test");
                SetAlert("Възникна грешка при създаването на теста", "danger");
                await PrepareCreateViewData();
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests
                .Include(t => t.TestQuestions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (test.CreatorId != currentUser.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
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

                var currentUser = await _userManager.GetUserAsync(User);
                if (test.CreatorId != currentUser.Id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Delete all related test results first
                if (test.TestResults != null && test.TestResults.Any())
                {
                    foreach (var result in test.TestResults)
                    {
                        // Delete answer results first
                        if (result.AnswerResults != null)
                        {
                            _context.TestAnswerResults.RemoveRange(result.AnswerResults);
                        }
                    }
                    _context.TestResults.RemoveRange(test.TestResults);
                }

                // Delete test questions
                if (test.TestQuestions != null)
                {
                    _context.TestQuestions.RemoveRange(test.TestQuestions);
                }

                // Finally delete the test
                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();

                SetAlert("Тестът беше изтрит успешно", "success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting test");
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

            _logger.LogInformation($"Found {questions.Count} questions for category {model.CategoryId}");
            return questions;
        }

        public async Task<IActionResult> Take(int? id)
        {
            if (id == null)
            {
                return NotFound();
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

                SetAlert("Тестът беше успешно предаден.", "success");
                return RedirectToAction(nameof(Result), new { id = testResult.Id });
            }
            catch (Exception ex)
            {
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
            if (testResult.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View(testResult);
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
    }
} 