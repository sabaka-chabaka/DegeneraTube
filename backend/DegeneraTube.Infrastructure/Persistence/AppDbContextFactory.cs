using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DegeneraTube.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "DegeneraTube.API");

        var config = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(apiPath, "appsettings.json"))
            .Build();

        var connectionString = config.GetConnectionString("Postgres")!;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}