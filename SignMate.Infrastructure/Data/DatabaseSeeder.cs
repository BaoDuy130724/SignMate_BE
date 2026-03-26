using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SignMate.Domain.Entities;

namespace SignMate.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(SignMateDbContext context)
    {
        await SeedPlansAsync(context);
        
        // Seed dummy data if no users exist
        if (!await context.Users.AnyAsync())
        {
            var center = await SeedCenterAsync(context);
            var users = await SeedUsersAsync(context, center.Id);
            var teacher = users.First(u => u.Role == UserRole.Teacher);
            var student = users.First(u => u.Role == UserRole.Student);
            var admin = users.First(u => u.Role == UserRole.SuperAdmin);

            var cls = await SeedClassAsync(context, center.Id, teacher.Id, student.Id);
            await SeedCoursesAndLessonsAsync(context, admin.Id, student.Id);
        }
    }

    private static async Task SeedPlansAsync(SignMateDbContext context)
    {
        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Miễn phí",
                PriceVnd = 0,
                DurationDays = 30,
                Type = PlanType.Free,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "5 bài học mỗi ngày", 
                    "Phản hồi đúng sai cơ bản", 
                    "Không phân tích thống kê", 
                    "Không sửa lỗi chi tiết" 
                })
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Cơ bản",
                PriceVnd = 49000,
                DurationDays = 30,
                Type = PlanType.Basic,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "Bài học không giới hạn", 
                    "Phản hồi lỗi cơ bản", 
                    "Theo dõi tiến độ học tập", 
                    "Duy trì chuỗi ngày học (Streak)" 
                })
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Nâng cao (Pro)",
                PriceVnd = 99000,
                DurationDays = 30,
                Type = PlanType.Pro,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "AI sửa lỗi chi tiết", 
                    "Phân tích góc độ, hình dáng tay", 
                    "Phát hiện điểm yếu cần cải thiện", 
                    "Phân tích chuyên sâu (Advanced analytics)" 
                })
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Trung tâm (B2B)",
                PriceVnd = 79000,
                DurationDays = 30,
                Type = PlanType.B2B,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "Trang quản trị cho giáo viên", 
                    "Quản lý danh sách lớp học", 
                    "Theo dõi tiến độ học viên", 
                    "Báo cáo kết quả học tập",
                    "Yêu cầu tối thiểu 20 học viên"
                })
            }
        };

        if (!await context.SubscriptionPlans.AnyAsync())
        {
            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }
        else
        {
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
    }

    private static async Task<Center> SeedCenterAsync(SignMateDbContext context)
    {
        var center1 = new Center
        {
            Id = Guid.NewGuid(),
            Name = "Trung tâm VSL Hà Nội",
            ContactPerson = "Nguyễn Văn A",
            Email = "contact@vslhanoi.edu.vn",
            Phone = "0123456789"
        };
        var center2 = new Center
        {
            Id = Guid.NewGuid(),
            Name = "Trung tâm VSL TP.HCM",
            ContactPerson = "Trần Thị B",
            Email = "hcm@vsl.edu.vn",
            Phone = "0987654321"
        };
        var center3 = new Center
        {
            Id = Guid.NewGuid(),
            Name = "Trung tâm VSL Đà Nẵng",
            ContactPerson = "Lê Văn C",
            Email = "danang@vsl.edu.vn",
            Phone = "0112233445"
        };
        
        context.Centers.AddRange(center1, center2, center3);
        await context.SaveChangesAsync();
        return center1; // Return the first one to use for mapping
    }

    private static async Task<List<User>> SeedUsersAsync(SignMateDbContext context, Guid centerId)
    {
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@signmate.vn",
                PasswordHash = defaultPasswordHash,
                FullName = "Hệ thống Quản trị",
                Role = UserRole.SuperAdmin,
                IsOnboarded = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "centeradmin@vslhanoi.edu.vn",
                PasswordHash = defaultPasswordHash,
                FullName = "Admin Trung tâm",
                Role = UserRole.CenterAdmin,
                CenterId = centerId,
                IsOnboarded = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "teacher@vslhanoi.edu.vn",
                PasswordHash = defaultPasswordHash,
                FullName = "Giáo viên Trần B",
                Role = UserRole.Teacher,
                CenterId = centerId,
                IsOnboarded = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "student@gmail.com",
                PasswordHash = defaultPasswordHash,
                FullName = "Học viên Nguyễn C",
                Role = UserRole.Student,
                CenterId = centerId,
                IsOnboarded = true,
                XpPoints = 150,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Users.AddRange(users);
        
        var student = users.First(u => u.Role == UserRole.Student);
        context.Streaks.Add(new Streak
        {
            Id = Guid.NewGuid(),
            UserId = student.Id,
            CurrentStreak = 3,
            LongestStreak = 5,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await context.SaveChangesAsync();
        return users;
    }

    private static async Task<Class> SeedClassAsync(SignMateDbContext context, Guid centerId, Guid teacherId, Guid studentId)
    {
        var cls1 = new Class
        {
            Id = Guid.NewGuid(),
            CenterId = centerId,
            TeacherId = teacherId,
            Name = "Lớp Cơ bản 01"
        };
        var cls2 = new Class
        {
            Id = Guid.NewGuid(),
            CenterId = centerId,
            TeacherId = teacherId,
            Name = "Lớp Giao tiếp 02"
        };
        var cls3 = new Class
        {
            Id = Guid.NewGuid(),
            CenterId = centerId,
            TeacherId = teacherId,
            Name = "Lớp Nâng cao 01"
        };

        context.Classes.AddRange(cls1, cls2, cls3);

        context.ClassStudents.Add(new ClassStudent
        {
            ClassId = cls1.Id,
            StudentId = studentId
        });
        
        context.ClassStudents.Add(new ClassStudent
        {
            ClassId = cls2.Id,
            StudentId = studentId
        });

        await context.SaveChangesAsync();
        return cls1;
    }

    private static async Task SeedCoursesAndLessonsAsync(SignMateDbContext context, Guid adminId, Guid studentId)
    {
        var course1 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Giao tiếp Cơ bản",
            Description = "Học các từ vựng và mẫu câu giao tiếp phổ biến hàng ngày.",
            Level = CourseLevel.Beginner,
            IsPublished = true,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };
        
        var course2 = new Course
        {
            Id = Guid.NewGuid(),
            Title = "Bảng chữ cái & Số đếm",
            Description = "Nền tảng quan trọng nhất để ghép vần tên riêng.",
            Level = CourseLevel.Beginner,
            IsPublished = true,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        context.Courses.AddRange(course1, course2);

        var lesson1 = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = course1.Id,
            Title = "Bài 1: Chào hỏi",
            Description = "Cách chào, tạm biệt, cảm ơn.",
            OrderIndex = 1,
            DurationSeconds = 300,
            IsPublished = true
        };

        var lesson2 = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = course1.Id,
            Title = "Bài 2: Tự giới thiệu",
            Description = "Hỏi tên, giới thiệu bản thân.",
            OrderIndex = 2,
            DurationSeconds = 400,
            IsPublished = true
        };

        context.Lessons.AddRange(lesson1, lesson2);



        context.Enrollments.Add(new Enrollment
        {
            Id = Guid.NewGuid(),
            UserId = studentId,
            CourseId = course1.Id,
            EnrolledAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}
