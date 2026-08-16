using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GrandmasDreamNumbers.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations` construct the DbContext directly from
/// configuration instead of spinning up the full Api host (which pulls in
/// Identity/DataProtection services that aren't needed just to diff the
/// model).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=grandmas_dream_numbers;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
