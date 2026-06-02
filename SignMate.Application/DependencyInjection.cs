using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignMate.Application.Common.Behaviors;
using SignMate.Application.Interfaces;
using SignMate.Application.Services;

namespace SignMate.Application;

/// <summary>
/// DI registration for Application layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IPracticeService, PracticeService>();
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<INotificationService, NotificationService>();

        // New services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IB2BContactService, B2BContactService>();
        services.AddScoped<ICenterService, CenterService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IStudentTrackingService, StudentTrackingService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // MediatR & FluentValidation CQRS Pipeline
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
