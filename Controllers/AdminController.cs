using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Data;
using TestGenerator.Models;
using TestGenerator.ViewModels;

namespace TestGenerator.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger) : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var statistics = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalTests = await _context.Tests.CountAsync(),
                TotalQuestions = await _context.Questions.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                RecentUsers = await _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .Select(u => new UserViewModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName
                    })
                    .ToListAsync()
            };

            return View(statistics);
        }

        public async Task<IActionResult> Users()
        {
            try
            {
                var users = new List<UserViewModel>();
                var allUsers = await _userManager.Users.ToListAsync();
                
                foreach (var user in allUsers)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    var isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
                    
                    users.Add(new UserViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsAdmin = isAdmin,
                        IsTeacher = isTeacher
                    });
                }

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                SetAlert("Възникна грешка при зареждане на потребителите", "danger");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            
            try
            {
                if (isAdmin)
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    _logger.LogInformation($"Removed admin role from user {user.Email}");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation($"Added admin role to user {user.Email}");
                }

                SetAlert("Правата на потребителя бяха успешно променени", "success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling admin role");
                SetAlert("Възникна грешка при промяна на правата", "danger");
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTeacher(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isTeacher = await _userManager.IsInRoleAsync(user, "Teacher");
            
            try
            {
                if (isTeacher)
                {
                    await _userManager.RemoveFromRoleAsync(user, "Teacher");
                    _logger.LogInformation($"Removed teacher role from user {user.Email}");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");
                    _logger.LogInformation($"Added teacher role to user {user.Email}");
                }

                SetAlert("Правата на потребителя бяха успешно променени", "success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling teacher role");
                SetAlert("Възникна грешка при промяна на правата", "danger");
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // Delete user's tests
                var tests = await _context.Tests.Where(t => t.CreatorId == userId).ToListAsync();
                _context.Tests.RemoveRange(tests);

                // Delete user's questions
                var questions = await _context.Questions.Where(q => q.CreatorId == userId).ToListAsync();
                _context.Questions.RemoveRange(questions);

                // Delete user
                await _userManager.DeleteAsync(user);
                
                await _context.SaveChangesAsync();
                
                SetAlert("Потребителят беше успешно изтрит", "success");
                _logger.LogInformation($"User {user.Email} was deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                SetAlert("Възникна грешка при изтриване на потребителя", "danger");
            }

            return RedirectToAction(nameof(Users));
        }
    }
} 