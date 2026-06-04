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
        // Các service còn lại sẽ được chuyển dần sang CQRS ở Phase 8 (Admin & Analytics).
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        // MediatR & FluentValidation CQRS Pipeline
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
