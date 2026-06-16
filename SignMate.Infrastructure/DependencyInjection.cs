using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
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
        // AI service: ép IPv4 + connect-timeout (tránh treo IPv6 như PayOS từng gặp) và thêm
        // resilience — retry transient (5xx/408/network) + backoff+jitter + per-attempt timeout
        // + circuit breaker. Call /analyze (chấm điểm) idempotent nên retry an toàn.
        services.AddHttpClient<IAIClientService, AIClientService>()
            // Để resilience (TotalRequestTimeout) điều phối thời gian, tắt timeout mặc định
            // 100s của HttpClient (nếu không nó cắt ngang trước pipeline).
            .ConfigureHttpClient(c => c.Timeout = System.Threading.Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                ConnectTimeout = TimeSpan.FromSeconds(10),
                ConnectCallback = async (context, ct) =>
                {
                    var addresses = await Dns.GetHostAddressesAsync(
                        context.DnsEndPoint.Host, AddressFamily.InterNetwork, ct);
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        NoDelay = true
                    };
                    try
                    {
                        await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, ct);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            })
            .AddStandardResilienceHandler(options =>
            {
                // Video + MediaPipe có thể lâu → mỗi lần thử cho 30s; trần tổng 100s.
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(100);
                // Ràng buộc của handler: SamplingDuration >= 2 × AttemptTimeout (đặt 70 cho dư).
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(70);
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromSeconds(1);
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
            });

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
