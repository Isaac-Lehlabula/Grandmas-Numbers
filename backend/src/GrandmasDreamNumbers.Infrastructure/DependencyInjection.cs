using GrandmasDreamNumbers.Application.Common.Interfaces;
using GrandmasDreamNumbers.Application.Common.Settings;
using GrandmasDreamNumbers.Infrastructure.Ai;
using GrandmasDreamNumbers.Infrastructure.Identity;
using GrandmasDreamNumbers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrandmasDreamNumbers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Kept in sync with RegisterRequestValidator (8-128 chars)
                // and the Flutter register screen's "At least 8 characters"
                // hint - Identity's stricter complexity defaults (upper/
                // lower/digit/symbol) were rejecting passwords that passed
                // both of those, which is a confusing mismatch, not extra
                // security (found via live end-to-end testing).
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<DreamAnalysisSettings>(configuration.GetSection(DreamAnalysisSettings.SectionName));

        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();

        AddAiProvider(services, configuration);

        return services;
    }

    private static void AddAiProvider(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Ai:Provider"] ?? "Mock";

        switch (provider)
        {
            case "Mock":
                services.AddSingleton<IDreamInterpretationService, MockDreamInterpretationService>();
                break;
            default:
                throw new NotSupportedException(
                    $"AI provider '{provider}' is not supported. Only 'Mock' is implemented so far - " +
                    "add a new case here when wiring up a real provider.");
        }
    }
}
