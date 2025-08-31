using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DogRaces.Infrastructure.Database;

/// <summary>
/// Design-time factory for DogRacesContext to support EF migrations
/// </summary>
public class DogRacesContextFactory : IDesignTimeDbContextFactory<DogRacesContext>
{
    public DogRacesContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.Development.json");

        var optionsBuilder = new DbContextOptionsBuilder<DogRacesContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DogRacesContext(optionsBuilder.Options);
    }
}