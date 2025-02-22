namespace TestGenerator.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTests { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalCategories { get; set; }
        public List<UserViewModel> RecentUsers { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsTeacher { get; set; }
    }
} 