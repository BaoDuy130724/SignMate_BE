using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignMate.Application.Interfaces;
using SignMate.Infrastructure.Data;
using SignMate.Infrastructure.ExternalServices;
using SignMate.Infrastructure.Services;

namespace SignMate.Infrastructure;

/// <summary>
/// DI registration for Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<SignMateDbContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<ISignMateDbContext>(sp => sp.GetRequiredService<SignMateDbContext>());

        // Generic Repository & Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Memory Cache for OTPs
        services.AddMemoryCache();

        // External services
        services.AddHttpClient<IAIClientService, AIClientService>();
        services.AddHttpClient<IGeminiService, GeminiService>();
        services.AddScoped<IBlobService, BlobService>();

        // Email: gửi thật qua SMTP khi đã cấu hình "Email:Host"; nếu chưa thì rơi về Mock (chỉ log).
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        var smtpHost = configuration[$"{EmailSettings.SectionName}:Host"];
        if (string.IsNullOrWhiteSpace(smtpHost))
            services.AddScoped<IEmailService, MockEmailService>();
        else
            services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        // PayOS payment gateway
        services.AddSingleton<IPayOsService, PayOsService>();

        // Background Processing
        services.AddSingleton<IVideoProcessingQueue, VideoProcessingQueue>();
        services.AddHostedService<SignMate.Infrastructure.BackgroundJobs.VideoProcessingBackgroundService>();

        return services;
    }
}
