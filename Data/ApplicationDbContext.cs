using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestGenerator.Models;
using Microsoft.AspNetCore.Identity;

namespace TestGenerator.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestQuestion> TestQuestions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestAnswerResult> TestAnswerResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TestQuestion>()
                .HasKey(tq => new { tq.TestId, tq.QuestionId });

            builder.Entity<TestQuestion>()
                .HasOne(tq => tq.Test)
                .WithMany(t => t.TestQuestions)
                .HasForeignKey(tq => tq.TestId);

            builder.Entity<TestQuestion>()
                .HasOne(tq => tq.Question)
                .WithMany()
                .HasForeignKey(tq => tq.QuestionId);

            builder.Entity<TestResult>()
                .HasOne(tr => tr.Test)
                .WithMany(t => t.TestResults)
                .HasForeignKey(tr => tr.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TestResult>()
                .HasOne(tr => tr.User)
                .WithMany()
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TestAnswerResult>()
                .HasOne(tar => tar.TestResult)
                .WithMany(tr => tr.AnswerResults)
                .HasForeignKey(tar => tar.TestResultId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TestAnswerResult>()
                .HasOne(tar => tar.Question)
                .WithMany()
                .HasForeignKey(tar => tar.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Question>()
                .HasOne(q => q.Creator)
                .WithMany()
                .HasForeignKey(q => q.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for PercentageScore and Grade
            builder.Entity<TestResult>()
                .Property(tr => tr.PercentageScore)
                .HasPrecision(5, 2);

            builder.Entity<TestResult>()
                .Property(tr => tr.Grade)
                .HasPrecision(3, 2);

            builder.Entity<TestAnswerResult>()
                .Property(tar => tar.KeywordMatchPercentage)
                .HasPrecision(5, 2);
        }
    }
}
