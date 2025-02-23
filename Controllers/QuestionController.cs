using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TestGenerator.Services;

namespace TestGenerator.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class QuestionController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<QuestionController> _logger;
        private readonly IImageService _imageService;

        public QuestionController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<QuestionController> logger,
            IImageService imageService) 
            : base(context)
        {
            _userManager = userManager;
            _logger = logger;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.PossibleAnswers)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(q => q.CategoryId == categoryId);
            }

            var questions = await query.ToListAsync();
            ViewBag.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            return View(questions);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        public async Task<IActionResult> CreateMultipleChoice()
        {
            ViewBag.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> CreateOpenEnded()
        {
            ViewBag.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultipleChoice(QuestionViewModel model, int correctAnswerIndex)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get current user
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        _logger.LogError("User not found while creating multiple choice question");
                        SetAlert("Грешка: Потребителят не е намерен", "danger");
                        return RedirectToAction("Login", "Account");
                    }

                    var question = new Question
                    {
                        Content = model.Content,
                        Type = QuestionType.MultipleChoice,
                        DifficultyLevel = model.DifficultyLevel,
                        CategoryId = model.CategoryId,
                        Points = model.Points,
                        CreatorId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Handle image upload
                    if (model.Image != null)
                    {
                        question.ImagePath = await _imageService.SaveImageAsync(model.Image);
                    }

                    // Set correct answer for multiple choice
                    for (int i = 0; i < model.Answers.Count; i++)
                    {
                        model.Answers[i].IsCorrect = (i == correctAnswerIndex);
                    }

                    question.PossibleAnswers = model.Answers.Select(a => new Answer
                    {
                        Content = a.Content,
                        IsCorrect = a.IsCorrect
                    }).ToList();

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();

                    SetAlert("Въпросът беше създаден успешно", "success");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating multiple choice question");
                    SetAlert("Възникна грешка при създаването на въпроса", "danger");
                }
            }
            else
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Model validation error: {modelError.ErrorMessage}");
                }
            }

            ViewBag.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOpenEnded(QuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get current user
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        _logger.LogError("User not found while creating open ended question");
                        SetAlert("Грешка: Потребителят не е намерен", "danger");
                        return RedirectToAction("Login", "Account");
                    }

                    _logger.LogInformation($"Creating open ended question for user {user.Id}");

                    var question = new Question
                    {
                        Content = model.Content,
                        Type = QuestionType.OpenEnded,
                        DifficultyLevel = model.DifficultyLevel,
                        CategoryId = model.CategoryId,
                        Points = model.Points,
                        CreatorId = user.Id,
                        CorrectAnswer = model.CorrectAnswer,
                        Keywords = model.Keywords,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Handle image upload
                    if (model.Image != null)
                    {
                        question.ImagePath = await _imageService.SaveImageAsync(model.Image);
                    }

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();

                    SetAlert("Въпросът беше създаден успешно", "success");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating open ended question");
                    SetAlert("Възникна грешка при създаването на въпроса", "danger");
                }
            }
            else
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Model validation error: {modelError.ErrorMessage}");
                }
            }

            ViewBag.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            var viewModel = new QuestionViewModel
            {
                Id = question.Id,
                Content = question.Content,
                Type = question.Type,
                DifficultyLevel = question.DifficultyLevel,
                CategoryId = question.CategoryId,
                Points = question.Points,
                Keywords = question.Keywords,
                CorrectAnswer = question.CorrectAnswer,
                ExistingImagePath = question.ImagePath,
                Answers = question.PossibleAnswers?.Select(a => new AnswerViewModel
                {
                    Content = a.Content,
                    IsCorrect = a.IsCorrect
                }).ToList() ?? new List<AnswerViewModel>()
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", question.CategoryId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var question = await _context.Questions
                        .Include(q => q.PossibleAnswers)
                        .FirstOrDefaultAsync(q => q.Id == id);

                    if (question == null)
                    {
                        return NotFound();
                    }

                    question.Content = model.Content;
                    question.Type = model.Type;
                    question.DifficultyLevel = model.DifficultyLevel;
                    question.CategoryId = model.CategoryId;
                    question.Points = model.Points;
                    question.Keywords = model.Keywords;

                    if (model.Image != null)
                    {
                        question.ImagePath = await _imageService.SaveImageAsync(model.Image, question.ImagePath);
                    }

                    if (model.Type == QuestionType.MultipleChoice)
                    {
                        _context.RemoveRange(question.PossibleAnswers);
                        question.PossibleAnswers = model.Answers
                            .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                            .Select(a => new Answer
                            {
                                Content = a.Content,
                                IsCorrect = a.IsCorrect,
                                QuestionId = question.Id
                            }).ToList();
                        
                        question.CorrectAnswer = null;
                    }
                    else if (model.Type == QuestionType.OpenEnded)
                    {
                        question.CorrectAnswer = model.CorrectAnswer;
                        question.PossibleAnswers = null;
                    }

                    await _context.SaveChangesAsync();
                    SetAlert("Question updated successfully", "success");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Category)
                .Include(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions
                .Include(q => q.PossibleAnswers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(question.ImagePath))
                {
                    _imageService.DeleteImage(question.ImagePath);
                }

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                SetAlert("Въпросът беше изтрит успешно", "success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question");
                SetAlert("Грешка при изтриване на въпроса", "danger");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] QuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    Content = model.Content,
                    Type = model.Type,
                    DifficultyLevel = model.DifficultyLevel,
                    CategoryId = model.CategoryId,
                    Points = model.Points,
                    Keywords = model.Keywords,
                    CorrectAnswer = model.CorrectAnswer,
                    CreatorId = _userManager.GetUserId(User),
                    CreatedAt = DateTime.UtcNow
                };

                if (model.Type == QuestionType.MultipleChoice && model.Answers != null)
                {
                    question.PossibleAnswers = model.Answers
                        .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                        .Select(a => new Answer
                        {
                            Content = a.Content,
                            IsCorrect = a.IsCorrect
                        }).ToList();
                }

                _context.Add(question);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    id = question.Id,
                    content = question.Content,
                    type = question.Type.ToString(),
                    points = question.Points
                });
            }

            return BadRequest(ModelState);
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
} 