using System.Text.Json.Serialization;
using SmartInventory.API.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using Hangfire.PostgreSql;
using SmartInventory.Application.Auth.Interfaces;
using SmartInventory.Application.Auth.Services;
using SmartInventory.Application.Auth.BackgroundJobs;
using SmartInventory.Application.Location.Interfaces;
using SmartInventory.Application.Location.Mappings;
using SmartInventory.Application.Location.Services;
using SmartInventory.Application.Asset.Interfaces;
using SmartInventory.Application.Asset.Services;
using SmartInventory.Application.Asset.Mappings;
using SmartInventory.Application.Asset.BackgroundJobs;
using SmartInventory.Infrastructure.Auth.Configuration;
using SmartInventory.Infrastructure.Auth.Email;
using SmartInventory.Infrastructure.Auth.Repositories;
using SmartInventory.Infrastructure.Auth.Security;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Location.Repositories;
using SmartInventory.Infrastructure.Asset.Repositories;
using SmartInventory.Infrastructure.Asset.BackgroundJobs;
using SmartInventory.Infrastructure.Auth.BackgroundJobs;
using SmartInventory.Infrastructure.Notification.Repositories;
using SmartInventory.Application.Notification.Interfaces;
using SmartInventory.Application.Notification.Services;
using SmartInventory.Application.UserPreferences.Interfaces;
using SmartInventory.Application.UserPreferences.Services;
using SmartInventory.Infrastructure.UserPreferences.Repositories;
using SmartInventory.Api.Hubs;
using System.Net;
using System.Net.Mail;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("AZURE_POSTGRES_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Configure it as an Azure App Setting (ConnectionStrings__DefaultConnection) or AZURE_POSTGRES_CONNECTION_STRING env var.");
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(defaultConnection));

var jwtSettings = new JwtSettings
{
    IssuerSigningKey = builder.Configuration["Jwt:IssuerSigningKey"] ?? "YourSuperSecretKeyHereMustBeAtLeast32CharactersLong!",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "SmartInventory",
    Audience = builder.Configuration["Jwt:Audience"] ?? "SmartInventory",
    ExpiryMinutes = int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? "60")
};

builder.Services.AddSingleton(jwtSettings);

var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME");
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
{
    throw new InvalidOperationException("SMTP_USERNAME and SMTP_PASSWORD environment variables are required");
}

var smtpSettings = new SmtpSettings
{
    Host = builder.Configuration["Smtp:Host"] ?? "live.smtp.mailtrap.io",
    Port = int.Parse(builder.Configuration["Smtp:Port"] ?? "587"),
    Username = smtpUsername,
    Password = smtpPassword,
    EnableSsl = true,
    FromEmail = builder.Configuration["Smtp:FromEmail"] ?? "hello@demomailtrap.co",
    FromName = builder.Configuration["Smtp:FromName"] ?? "SmartInventory"
};

builder.Services.AddSingleton(smtpSettings);

builder.Services.AddScoped<ISmtpClient>(sp =>
{
    var settings = sp.GetRequiredService<SmtpSettings>();
    var smtpClient = new SmtpClient(settings.Host, settings.Port)
    {
        Credentials = new NetworkCredential(settings.Username, settings.Password),
        EnableSsl = settings.EnableSsl
    };
    return new SmtpClientWrapper(smtpClient);
});

builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IEmailJob, EmailJob>();

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SupervisorOnly", policy => policy.RequireRole("Supervisor"));
});

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ILocationService, LocationService>();

builder.Services.AddScoped<IBulkImportJob, BulkImportJob>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssetHistoryRepository, AssetHistoryRepository>();
builder.Services.AddScoped<IAssetHistoryService, AssetHistoryService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<CsvExportService>();
builder.Services.AddScoped<PdfExportService>();

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();

builder.Services.AddAutoMapper(typeof(LocationMappingProfile));
builder.Services.AddAutoMapper(typeof(AssetMappingProfile));

builder.Services.AddHealthChecks();

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(defaultConnection));
builder.Services.AddHangfireServer();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.MapGet("/", () => Results.Ok(new { service = "SmartInventory.Api", status = "Running" }));
}

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new SmartInventory.Api.HangfireDashboardAuthorizationFilter() }
});

app.Run();