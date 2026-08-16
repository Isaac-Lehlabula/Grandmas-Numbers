using System.Reflection;
using FluentValidation;
using GrandmasDreamNumbers.Application.Admin.Services;
using GrandmasDreamNumbers.Application.Dreams.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GrandmasDreamNumbers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<DreamSymbolMatcher>();
        services.AddScoped<CombinationGenerator>();
        services.AddScoped<DreamAnalysisService>();
        services.AddScoped<DreamHistoryService>();
        services.AddScoped<DreamSymbolSearchService>();
        services.AddScoped<AdminDreamSymbolService>();

        return services;
    }
}
