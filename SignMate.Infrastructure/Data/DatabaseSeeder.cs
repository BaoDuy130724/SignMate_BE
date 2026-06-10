using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SignMate.Domain.Entities;

namespace SignMate.Infrastructure.Data;

/// <summary>
/// Nạp bộ dữ liệu chuẩn cho SignMate (gói cước, trung tâm, người dùng theo từng vai trò, lớp,
/// khóa học/bài học, ghi danh + tiến độ, đăng ký gói, thông báo, thành tích, ký hiệu...).
/// Dữ liệu được thiết kế nhất quán để chạy thử toàn bộ luồng app mà không cần tạo tay.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>Mật khẩu mặc định cho mọi tài khoản seed (đăng nhập thử nhanh).</summary>
    public const string DefaultPassword = "123456";

    /// <summary>
    /// Nạp dữ liệu chuẩn. Khi <paramref name="reset"/> = true sẽ <b>xóa toàn bộ DB hiện tại</b>
    /// rồi tạo lại từ migration (identity reset về 1 → dữ liệu tất định), bảo đảm trạng thái sạch.
    /// Khi false: chỉ áp migration và seed những phần còn thiếu (idempotent).
    /// </summary>
    public static async Task SeedAsync(SignMateDbContext context, bool reset = false)
    {
        if (reset)
            await context.Database.EnsureDeletedAsync();

        // Tạo DB + áp toàn bộ migration (tạo mới nếu chưa có).
        await context.Database.MigrateAsync();

        await SeedPlansAsync(context);
        await SeedAchievementsAsync(context);

        // Khối dữ liệu nghiệp vụ chỉ seed khi chưa có người dùng (sau reset thì DB rỗng nên luôn chạy).
        if (!await context.Users.AnyAsync())
        {
            var centers = await SeedCentersAsync(context);
            var users = await SeedUsersAsync(context, centers[0].Id);

            var teacher = users.First(u => u.Role == UserRole.Teacher);
            var students = users.Where(u => u.Role == UserRole.Student).ToList();
            var admin = users.First(u => u.Role == UserRole.SuperAdmin);

            await SeedSubscriptionsAsync(context, students);
            var classes = await SeedClassesAsync(context, centers[0].Id, teacher.Id, students);
            var (courses, lessons) = await SeedCoursesAndLessonsAsync(context, admin.Id);
            await SeedEnrollmentsAndProgressAsync(context, students, courses, lessons);
            await SeedLessonAssignmentsAsync(context, classes[0].Id, lessons[0].Id, teacher.Id);
            await SeedNotificationsAsync(context, students[0].Id);
            await SeedUserAchievementsAsync(context, students[0].Id);
            await SeedTeacherCommentsAsync(context, teacher.Id, students[0].Id);
        }

        await SeedSignsAsync(context);
        await SeedPracticeSessionsAndAttemptsAsync(context);
    }

    // ── Gói cước ─────────────────────────────────────────────────────────────
    private static async Task SeedPlansAsync(SignMateDbContext context)
    {
        var plans = new List<SubscriptionPlan>
        {
            new() { Name = "Gói Miễn phí", PriceVnd = 0, DurationDays = 30, Type = PlanType.Free,
                FeaturesJson = JsonSerializer.Serialize(new[]
                { "5 bài học mỗi ngày", "Phản hồi đúng sai cơ bản", "Không phân tích thống kê", "Không sửa lỗi chi tiết" }) },
            new() { Name = "Gói Cơ bản", PriceVnd = 49000, DurationDays = 30, Type = PlanType.Basic,
                FeaturesJson = JsonSerializer.Serialize(new[]
                { "Bài học không giới hạn", "Phản hồi lỗi cơ bản", "Theo dõi tiến độ học tập", "Duy trì chuỗi ngày học (Streak)" }) },
            new() { Name = "Gói Nâng cao (Pro)", PriceVnd = 99000, DurationDays = 30, Type = PlanType.Pro,
                FeaturesJson = JsonSerializer.Serialize(new[]
                { "AI sửa lỗi chi tiết", "Phân tích góc độ, hình dáng tay", "Phát hiện điểm yếu cần cải thiện", "Phân tích chuyên sâu (Advanced analytics)" }) },
            new() { Name = "Gói Trung tâm (B2B)", PriceVnd = 79000, DurationDays = 30, Type = PlanType.B2B,
                FeaturesJson = JsonSerializer.Serialize(new[]
                { "Trang quản trị cho giáo viên", "Quản lý danh sách lớp học", "Theo dõi tiến độ học viên", "Báo cáo kết quả học tập", "Yêu cầu tối thiểu 20 học viên" }) }
        };

        if (!await context.SubscriptionPlans.AnyAsync())
        {
            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
            return;
        }

        // Upsert theo loại gói để giữ nội dung gói luôn mới nhất.
        foreach (var newPlan in plans)
        {
            var existing = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Type == newPlan.Type);
            if (existing != null)
            {
                existing.Name = newPlan.Name;
                existing.PriceVnd = newPlan.PriceVnd;
                existing.FeaturesJson = newPlan.FeaturesJson;
            }
            else
            {
                context.SubscriptionPlans.Add(newPlan);
            }
        }
        await context.SaveChangesAsync();
    }

    // ── Thành tích (global) ──────────────────────────────────────────────────
    private static async Task SeedAchievementsAsync(SignMateDbContext context)
    {
        if (await context.Achievements.AnyAsync()) return;

        await context.Achievements.AddRangeAsync(
            new Achievement { Name = "Bước đầu tiên", Description = "Hoàn thành bài học đầu tiên.", ConditionType = "LessonsCompleted", ConditionValue = 1 },
            new Achievement { Name = "Chăm chỉ", Description = "Duy trì chuỗi học 3 ngày liên tiếp.", ConditionType = "StreakDays", ConditionValue = 3 },
            new Achievement { Name = "Kiên trì", Description = "Duy trì chuỗi học 7 ngày liên tiếp.", ConditionType = "StreakDays", ConditionValue = 7 },
            new Achievement { Name = "Nhà sưu tầm", Description = "Đạt 500 điểm kinh nghiệm (XP).", ConditionType = "XpPoints", ConditionValue = 500 });

        await context.SaveChangesAsync();
    }

    // ── Trung tâm ────────────────────────────────────────────────────────────
    private static async Task<List<Center>> SeedCentersAsync(SignMateDbContext context)
    {
        var centers = new List<Center>
        {
            new() { Name = "Trung tâm VSL Hà Nội", ContactPerson = "Nguyễn Văn A", Email = "contact@vslhanoi.edu.vn", Phone = "0123456789", MaxSeats = 50, IsActive = true },
            new() { Name = "Trung tâm VSL TP.HCM", ContactPerson = "Trần Thị B", Email = "hcm@vsl.edu.vn", Phone = "0987654321", MaxSeats = 100, IsActive = true },
            new() { Name = "Trung tâm VSL Đà Nẵng", ContactPerson = "Lê Văn C", Email = "danang@vsl.edu.vn", Phone = "0112233445", MaxSeats = 30, IsActive = true }
        };
        context.Centers.AddRange(centers);
        await context.SaveChangesAsync();
        return centers;
    }

    // ── Người dùng theo vai trò + streak ─────────────────────────────────────
    private static async Task<List<User>> SeedUsersAsync(SignMateDbContext context, int centerId)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);
        var now = DateTime.UtcNow;

        var users = new List<User>
        {
            new() { Email = "admin@signmate.vn", PasswordHash = hash, FullName = "Hệ thống Quản trị", Role = UserRole.SuperAdmin, IsOnboarded = true, CreatedAt = now, UpdatedAt = now },
            new() { Email = "centeradmin@vslhanoi.edu.vn", PasswordHash = hash, FullName = "Admin Trung tâm", Role = UserRole.CenterAdmin, CenterId = centerId, IsOnboarded = true, CreatedAt = now, UpdatedAt = now },
            new() { Email = "teacher@vslhanoi.edu.vn", PasswordHash = hash, FullName = "Giáo viên Trần B", Role = UserRole.Teacher, CenterId = centerId, IsOnboarded = true, CreatedAt = now, UpdatedAt = now },
            // student[0] — học viên Pro, đã onboard, có tiến độ
            new() { Email = "student@gmail.com", PasswordHash = hash, FullName = "Học viên Nguyễn C", Role = UserRole.Student, CenterId = centerId, IsOnboarded = true, XpPoints = 150, Goal = "Học giao tiếp", Level = "Người mới bắt đầu", CreatedAt = now, UpdatedAt = now },
            // student[1] — học viên Basic
            new() { Email = "student2@gmail.com", PasswordHash = hash, FullName = "Học viên Phạm D", Role = UserRole.Student, CenterId = centerId, IsOnboarded = true, XpPoints = 80, Goal = "Phát triển sự nghiệp", Level = "Biết cơ bản", CreatedAt = now, UpdatedAt = now },
            // student[2] — học viên Free, mới (chưa onboard)
            new() { Email = "student3@gmail.com", PasswordHash = hash, FullName = "Học viên Võ E", Role = UserRole.Student, IsOnboarded = false, XpPoints = 0, CreatedAt = now, UpdatedAt = now }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync(); // sinh Id cho User trước khi tạo các bản ghi phụ thuộc

        var students = users.Where(u => u.Role == UserRole.Student).ToList();
        context.Streaks.AddRange(
            new Streak { UserId = students[0].Id, CurrentStreak = 3, LongestStreak = 5, LastActiveDate = DateOnly.FromDateTime(now) },
            new Streak { UserId = students[1].Id, CurrentStreak = 1, LongestStreak = 4, LastActiveDate = DateOnly.FromDateTime(now.AddDays(-1)) });
        await context.SaveChangesAsync();

        return users;
    }

    // ── Đăng ký gói (1 gói active / học viên) ─────────────────────────────────
    private static async Task SeedSubscriptionsAsync(SignMateDbContext context, List<User> students)
    {
        var plans = await context.SubscriptionPlans.ToListAsync();
        var pro = plans.First(p => p.Type == PlanType.Pro);
        var basic = plans.First(p => p.Type == PlanType.Basic);
        var free = plans.First(p => p.Type == PlanType.Free);
        var now = DateTime.UtcNow;

        UserSubscription Make(int userId, SubscriptionPlan plan, string? payRef) => new()
        {
            UserId = userId,
            PlanId = plan.Id,
            StartDate = now,
            EndDate = now.AddDays(plan.DurationDays),
            IsActive = true,
            PaymentReference = payRef
        };

        context.UserSubscriptions.AddRange(
            Make(students[0].Id, pro, "SEED-PRO-0001"),
            Make(students[1].Id, basic, "SEED-BASIC-0001"),
            Make(students[2].Id, free, null));
        await context.SaveChangesAsync();
    }

    // ── Lớp học + ghi danh học viên vào lớp ──────────────────────────────────
    private static async Task<List<Class>> SeedClassesAsync(SignMateDbContext context, int centerId, int teacherId, List<User> students)
    {
        var classes = new List<Class>
        {
            new() { CenterId = centerId, TeacherId = teacherId, Name = "Lớp Cơ bản 01" },
            new() { CenterId = centerId, TeacherId = teacherId, Name = "Lớp Giao tiếp 02" },
            new() { CenterId = centerId, TeacherId = teacherId, Name = "Lớp Nâng cao 01" }
        };
        context.Classes.AddRange(classes);
        await context.SaveChangesAsync();

        context.ClassStudents.AddRange(
            new ClassStudent { ClassId = classes[0].Id, StudentId = students[0].Id },
            new ClassStudent { ClassId = classes[1].Id, StudentId = students[0].Id },
            new ClassStudent { ClassId = classes[0].Id, StudentId = students[1].Id });
        await context.SaveChangesAsync();

        return classes;
    }

    // ── Khóa học + bài học ───────────────────────────────────────────────────
    private static async Task<(List<Course> Courses, List<Lesson> Lessons)> SeedCoursesAndLessonsAsync(
        SignMateDbContext context, int adminId)
    {
        var now = DateTime.UtcNow;
        var courses = new List<Course>
        {
            new() { Title = "Giao tiếp Cơ bản", Description = "Học các từ vựng và mẫu câu giao tiếp phổ biến hàng ngày.", Level = CourseLevel.Beginner, IsPublished = true, CreatedBy = adminId, CreatedAt = now },
            new() { Title = "Bảng chữ cái & Số đếm", Description = "Nền tảng quan trọng nhất để ghép vần tên riêng.", Level = CourseLevel.Beginner, IsPublished = true, CreatedBy = adminId, CreatedAt = now },
            new() { Title = "Giao tiếp Nâng cao", Description = "Mẫu câu phức tạp, hội thoại theo tình huống.", Level = CourseLevel.Intermediate, IsPublished = true, CreatedBy = adminId, CreatedAt = now }
        };
        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        // LƯU Ý: lesson đầu tiên (Id=1 sau reset) là mốc tham chiếu của file SeedData_Signs.sql.
        var lessons = new List<Lesson>
        {
            new() { CourseId = courses[0].Id, Title = "Bài 1: Chào hỏi", Topic = "Chào hỏi", Description = "Cách chào, tạm biệt, cảm ơn.", OrderIndex = 1, DurationSeconds = 300, IsPublished = true },
            new() { CourseId = courses[0].Id, Title = "Bài 2: Tự giới thiệu", Topic = "Giới thiệu", Description = "Hỏi tên, giới thiệu bản thân.", OrderIndex = 2, DurationSeconds = 400, IsPublished = true },
            new() { CourseId = courses[1].Id, Title = "Bài 1: Bảng chữ cái", Topic = "Bảng chữ cái", Description = "Ký hiệu bảng chữ cái tiếng Việt.", OrderIndex = 1, DurationSeconds = 360, IsPublished = true },
            new() { CourseId = courses[1].Id, Title = "Bài 2: Số đếm", Topic = "Số đếm", Description = "Ký hiệu số đếm cơ bản.", OrderIndex = 2, DurationSeconds = 320, IsPublished = true }
        };
        context.Lessons.AddRange(lessons);
        await context.SaveChangesAsync();

        return (courses, lessons);
    }

    // ── Ghi danh khóa học + tiến độ bài học ──────────────────────────────────
    private static async Task SeedEnrollmentsAndProgressAsync(
        SignMateDbContext context, List<User> students, List<Course> courses, List<Lesson> lessons)
    {
        var now = DateTime.UtcNow;

        // student[0] học cả course1 & course2; student[1] học course1.
        var enr1 = new Enrollment { UserId = students[0].Id, CourseId = courses[0].Id, EnrolledAt = now.AddDays(-5) };
        var enr2 = new Enrollment { UserId = students[0].Id, CourseId = courses[1].Id, EnrolledAt = now.AddDays(-2) };
        var enr3 = new Enrollment { UserId = students[1].Id, CourseId = courses[0].Id, EnrolledAt = now.AddDays(-3) };
        context.Enrollments.AddRange(enr1, enr2, enr3);
        await context.SaveChangesAsync();

        var course1Lessons = lessons.Where(l => l.CourseId == courses[0].Id).OrderBy(l => l.OrderIndex).ToList();

        // student[0]: bài 1 đã hoàn thành, bài 2 đang học.
        context.LessonProgresses.AddRange(
            new LessonProgress { EnrollmentId = enr1.Id, UserId = students[0].Id, LessonId = course1Lessons[0].Id, Status = LessonStatus.Completed, WatchDurationSeconds = course1Lessons[0].DurationSeconds, LastWatchedAt = now.AddDays(-4) },
            new LessonProgress { EnrollmentId = enr1.Id, UserId = students[0].Id, LessonId = course1Lessons[1].Id, Status = LessonStatus.InProgress, WatchDurationSeconds = 120, LastWatchedAt = now.AddDays(-1) });
        await context.SaveChangesAsync();
    }

    // ── Bài tập giao cho lớp (deadline) ──────────────────────────────────────
    private static async Task SeedLessonAssignmentsAsync(SignMateDbContext context, int classId, int lessonId, int teacherId)
    {
        var now = DateTime.UtcNow;
        context.LessonAssignments.Add(new LessonAssignment
        {
            ClassId = classId,
            LessonId = lessonId,
            AssignedBy = teacherId,
            AssignedAt = now,
            DueDate = now.AddDays(7)
        });
        await context.SaveChangesAsync();
    }

    // ── Thông báo ────────────────────────────────────────────────────────────
    private static async Task SeedNotificationsAsync(SignMateDbContext context, int studentId)
    {
        var now = DateTime.UtcNow;
        context.Notifications.AddRange(
            new Notification { UserId = studentId, Title = "Chào mừng đến với SignMate!", Body = "Bắt đầu hành trình học Ngôn ngữ Ký hiệu của bạn ngay hôm nay.", Type = "system", IsRead = false, CreatedAt = now.AddDays(-5) },
            new Notification { UserId = studentId, Title = "Giữ vững chuỗi học!", Body = "Bạn đang có chuỗi 3 ngày. Học tiếp để không bị mất nhé!", Type = "streak", IsRead = false, CreatedAt = now.AddHours(-2) });
        await context.SaveChangesAsync();
    }

    // ── Thành tích đã đạt của học viên ───────────────────────────────────────
    private static async Task SeedUserAchievementsAsync(SignMateDbContext context, int studentId)
    {
        var firstStep = await context.Achievements.FirstOrDefaultAsync(a => a.ConditionType == "LessonsCompleted");
        var streak3 = await context.Achievements.FirstOrDefaultAsync(a => a.ConditionType == "StreakDays" && a.ConditionValue == 3);
        var now = DateTime.UtcNow;

        if (firstStep != null)
            context.UserAchievements.Add(new UserAchievement { UserId = studentId, AchievementId = firstStep.Id, EarnedAt = now.AddDays(-4) });
        if (streak3 != null)
            context.UserAchievements.Add(new UserAchievement { UserId = studentId, AchievementId = streak3.Id, EarnedAt = now.AddHours(-2) });

        await context.SaveChangesAsync();
    }

    // ── Nhận xét của giáo viên ───────────────────────────────────────────────
    private static async Task SeedTeacherCommentsAsync(SignMateDbContext context, int teacherId, int studentId)
    {
        context.TeacherComments.Add(new TeacherComment
        {
            TeacherId = teacherId,
            StudentId = studentId,
            Content = "Em tiến bộ tốt ở phần chào hỏi. Cố gắng luyện thêm phần tự giới thiệu nhé!",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();
    }

    // ── Ký hiệu (chèn từ file SQL có sẵn keypoint) ───────────────────────────
    private static async Task SeedSignsAsync(SignMateDbContext context)
    {
        if (await context.Signs.AnyAsync()) return;

        var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData_Signs.sql");
        if (!File.Exists(sqlFile)) return;

        var sql = await File.ReadAllTextAsync(sqlFile);
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task SeedPracticeSessionsAndAttemptsAsync(SignMateDbContext context)
    {
        if (await context.PracticeSessions.AnyAsync()) return;

        var students = await context.Users.Where(u => u.Role == UserRole.Student).ToListAsync();
        var sign = await context.Signs.FirstOrDefaultAsync();
        if (students.Count == 0 || sign == null) return;

        var now = DateTime.UtcNow;

        // Session 1: Nguyễn C (student@gmail.com)
        var session1 = new PracticeSession
        {
            UserId = students[0].Id,
            SignId = sign.Id,
            StartedAt = now.AddMinutes(-45),
            EndedAt = now.AddMinutes(-15),
            TotalAttempts = 2
        };

        // Session 2: Phạm D (student2@gmail.com)
        var session2 = new PracticeSession
        {
            UserId = students[1].Id,
            SignId = sign.Id,
            StartedAt = now.AddMinutes(-20),
            EndedAt = now.AddMinutes(-5),
            TotalAttempts = 1
        };

        await context.PracticeSessions.AddRangeAsync(session1, session2);
        await context.SaveChangesAsync();

        // Attempts
        var attempts = new List<PracticeAttempt>
        {
            new() { SessionId = session1.Id, VideoClipUrl = "https://signmate.vn/clips/c1.mp4", RecordedAt = now.AddMinutes(-40), OverallScore = 0.85f },
            new() { SessionId = session1.Id, VideoClipUrl = "https://signmate.vn/clips/c2.mp4", RecordedAt = now.AddMinutes(-30), OverallScore = 0.90f },
            new() { SessionId = session2.Id, VideoClipUrl = "https://signmate.vn/clips/d1.mp4", RecordedAt = now.AddMinutes(-10), OverallScore = 0.75f }
        };

        await context.PracticeAttempts.AddRangeAsync(attempts);
        await context.SaveChangesAsync();
    }
}
