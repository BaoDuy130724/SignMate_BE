using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using SignMate.API.Middleware;
using SignMate.Application;
using SignMate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//// Load local secrets file (gitignored) — overrides appsettings.Development.json
//builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ── Clean Architecture DI ──────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Authentication (JWT) ───────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
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

var app = builder.Build();


// ── Middleware Pipeline ────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseResponseCompression();
// Serving static dataset files
app.UseStaticFiles();
// app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
