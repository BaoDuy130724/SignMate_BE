using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignMate.Application.Interfaces;
using SignMate.Infrastructure.Data;
using SignMate.Infrastructure.ExternalServices;

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
            opt.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ISignMateDbContext>(sp => sp.GetRequiredService<SignMateDbContext>());

        // Memory Cache for OTPs
        services.AddMemoryCache();

        // External services
        services.AddHttpClient<IAIClientService, AIClientService>();
        services.AddHttpClient<IGeminiService, GeminiService>();
        services.AddScoped<IBlobService, BlobService>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddSingleton<IVnPayService, VnPayService>();

        return services;
    }
}
