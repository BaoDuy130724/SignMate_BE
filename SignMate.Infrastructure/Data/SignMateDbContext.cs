using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Infrastructure.Data;

public class SignMateDbContext : DbContext, ISignMateDbContext
{
    public SignMateDbContext(DbContextOptions<SignMateDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Sign> Signs => Set<Sign>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<SignProgress> SignProgresses => Set<SignProgress>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<PracticeAttempt> PracticeAttempts => Set<PracticeAttempt>();
    public DbSet<AIFeedback> AIFeedbacks => Set<AIFeedback>();
    public DbSet<Streak> Streaks => Set<Streak>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<Center> Centers => Set<Center>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassStudent> ClassStudents => Set<ClassStudent>();
    public DbSet<LessonAssignment> LessonAssignments => Set<LessonAssignment>();
    public DbSet<TeacherComment> TeacherComments => Set<TeacherComment>();
    public DbSet<B2BContactLead> B2BContactLeads => Set<B2BContactLead>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<SignReferenceRequest> SignReferenceRequests => Set<SignReferenceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.FullName).HasMaxLength(200);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Lesson>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300);
            e.HasOne(x => x.Course).WithMany(c => c.Lessons)
             .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sign>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Word).HasMaxLength(200);
            e.HasOne(x => x.Lesson).WithMany(l => l.Signs)
             .HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.Enrollments)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Course).WithMany(c => c.Enrollments)
             .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LessonProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.LessonId }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Enrollment).WithMany(en => en.LessonProgresses)
             .HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Lesson).WithMany()
             .HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SignProgress>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.SignId }).IsUnique();
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sign).WithMany(s => s.SignProgresses)
             .HasForeignKey(x => x.SignId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PracticeSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.PracticeSessions)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Sign).WithMany()
             .HasForeignKey(x => x.SignId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PracticeAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Session).WithMany(s => s.Attempts)
             .HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AIFeedback>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Attempt).WithMany(a => a.Feedbacks)
             .HasForeignKey(x => x.AttemptId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.FeedbackType).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<Streak>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasOne(x => x.User).WithOne(u => u.Streak)
             .HasForeignKey<Streak>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Achievement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.ConditionType).HasMaxLength(100);
        });

        modelBuilder.Entity<UserAchievement>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.AchievementId }).IsUnique();
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Achievement).WithMany(a => a.UserAchievements)
             .HasForeignKey(x => x.AchievementId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Type).HasMaxLength(50);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubscriptionPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.PriceVnd).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<UserSubscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.PlanId, x.IsActive });
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Plan).WithMany(p => p.UserSubscriptions)
             .HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Center>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(300);
            e.Property(x => x.ContactPerson).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<Class>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasOne(x => x.Center).WithMany(c => c.Classes)
             .HasForeignKey(x => x.CenterId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Teacher).WithMany()
             .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClassStudent>(e =>
        {
            e.HasKey(x => new { x.ClassId, x.StudentId });
            e.HasOne(x => x.Class).WithMany(c => c.ClassStudents)
             .HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Student).WithMany()
             .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LessonAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Class).WithMany(c => c.LessonAssignments)
             .HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Lesson).WithMany()
             .HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Teacher).WithMany()
             .HasForeignKey(x => x.AssignedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeacherComment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Teacher).WithMany()
             .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Student).WithMany()
             .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<B2BContactLead>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CenterName).HasMaxLength(300);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
        });

        modelBuilder.Entity<GameSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.GameType).HasMaxLength(100);
            e.HasOne(x => x.User).WithMany()
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
