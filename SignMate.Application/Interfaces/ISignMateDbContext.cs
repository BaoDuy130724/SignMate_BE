using Microsoft.EntityFrameworkCore;
using SignMate.Domain.Entities;

namespace SignMate.Application.Interfaces;

/// <summary>
/// Abstraction over the database context. Implemented by Infrastructure layer.
/// </summary>
public interface ISignMateDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Course> Courses { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Sign> Signs { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<SignProgress> SignProgresses { get; }
    DbSet<PracticeSession> PracticeSessions { get; }
    DbSet<PracticeAttempt> PracticeAttempts { get; }
    DbSet<AIFeedback> AIFeedbacks { get; }
    DbSet<Streak> Streaks { get; }
    DbSet<Achievement> Achievements { get; }
    DbSet<UserAchievement> UserAchievements { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }
    DbSet<Center> Centers { get; }
    DbSet<Class> Classes { get; }
    DbSet<ClassStudent> ClassStudents { get; }
    DbSet<LessonAssignment> LessonAssignments { get; }
    DbSet<TeacherComment> TeacherComments { get; }
    DbSet<B2BContactLead> B2BContactLeads { get; }
    DbSet<GameSession> GameSessions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
