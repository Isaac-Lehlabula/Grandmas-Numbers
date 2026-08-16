using System.Text;
using GrandmasDreamNumbers.Api.Filters;
using GrandmasDreamNumbers.Api.Middleware;
using GrandmasDreamNumbers.Application;
using GrandmasDreamNumbers.Application.Common.Settings;
using GrandmasDreamNumbers.Infrastructure;
using GrandmasDreamNumbers.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Grandma's Dream Numbers API",
        Version = "v1"
    });
});

builder.Services.AddDataProtection();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("dream-analysis", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { title = "Too many requests", detail = "Please slow down and try again shortly." },
            cancellationToken);
    };
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
        name: "postgresql");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    try
    {
        await DevelopmentSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        // Sample-data seeding is a dev convenience, not a startup requirement -
        // don't block the app (or WebApplicationFactory-based tests) just
        // because Postgres isn't reachable yet.
        app.Logger.LogWarning(ex, "Skipping development data seeding - database not reachable.");
    }
}
else
{
    // Local dev runs plain HTTP for simplicity (no dev cert setup required).
    // Everywhere else, require HTTPS.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}
