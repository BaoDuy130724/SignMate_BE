using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using SignMate.API.Middleware;
using SignMate.Application;
using SignMate.Application.DTOs.Common;
using SignMate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Load local secrets file (gitignored) — overrides appsettings.{Environment}.json
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ── Clean Architecture DI ──────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Context & User Accessor ────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SignMate.Application.Interfaces.ICurrentUser, SignMate.API.Services.CurrentUser>();

// ── Forwarded Headers (chạy sau reverse proxy: runasp / Render / Azure) ──
// Để client IP (rate limit + IP thanh toán) và scheme https được nhận đúng từ proxy.
// KnownProxies/Networks clear vì PaaS dùng proxy IP động không biết trước.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ── Authentication (JWT) ───────────────────────────────────────
// Fail-fast nếu thiếu secret → tránh NullReferenceException khó hiểu lúc khởi động trên server.
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException(
        "Thiếu cấu hình 'Jwt:Secret'. Đặt qua biến môi trường Jwt__Secret " +
        "hoặc appsettings.Local.json trước khi chạy.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Controllers ────────────────────────────────────────────────
builder.Services.AddControllers();

// Chuẩn hóa lỗi model-binding/validation tự động của [ApiController] về ApiResponse,
// tránh trả ValidationProblemDetails mặc định không đồng nhất với phần còn lại của hệ thống.
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
            .Where(msg => !string.IsNullOrWhiteSpace(msg))
            .ToList();

        Console.WriteLine("MODEL STATE ERRORS: " + string.Join(" | ", errors));

        var apiResponse = SignMate.Application.DTOs.Common.ApiResponse
            .FailureResult("Dữ liệu không hợp lệ.", errors);

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(apiResponse);
    };
});

// ── Response Compression ───────────────────────────────────────
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/json"]);
});

// ── Health Checks ──────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Background Services ────────────────────────────────────────
builder.Services.AddHostedService<SignMate.API.Services.PendingSubscriptionCleanupService>();

// ── Swagger ────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SignMate API",
        Version = "v1",
        Description = "Vietnamese Sign Language Learning Platform API"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ───────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── Rate Limiting (chống spam) ─────────────────────────────────
// Phân vùng theo IP. Khi vượt hạn mức trả 429 kèm ApiResponse chuẩn để mobile hiển thị.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // "otp": ngặt nhất — cho endpoint gửi email OTP (đăng ký / quên mật khẩu).
    options.AddPolicy("otp", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientIp(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));

    // "auth": cho các endpoint xác thực nhạy cảm (đăng nhập / đăng ký / đặt lại mật khẩu)
    // và biểu mẫu công khai — chống brute-force & spam.
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientIp(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        var response = context.HttpContext.Response;
        response.ContentType = "application/json";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

        var apiResponse = ApiResponse.FailureResult(
            "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.");
        var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await response.WriteAsync(json, token);
    };
});

var app = builder.Build();


// ── Middleware Pipeline ────────────────────────────────────────
// Đặt đầu tiên: dịch X-Forwarded-* từ proxy thành scheme/IP thật cho các middleware sau.
app.UseForwardedHeaders();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseResponseCompression();
// Serving static dataset files
app.UseStaticFiles();
// app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Lấy IP client để phân vùng rate limit (ưu tiên IP kết nối; fallback "unknown").
static string GetClientIp(HttpContext ctx) =>
    ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
