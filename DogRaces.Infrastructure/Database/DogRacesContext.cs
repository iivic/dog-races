using DogRaces.Application.Data;
using DogRaces.Domain.Entities;
using DogRaces.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Infrastructure.Database;

/// <summary>
/// Entity Framework DbContext for Dog Races betting system
/// </summary>
public class DogRacesContext : DbContext, IDogRacesContext
{
    public DogRacesContext(DbContextOptions<DogRacesContext> options) : base(options) {}
    
    // DbSets for entities
    public DbSet<Race> Races => Set<Race>();
    public DbSet<RaceOdds> RaceOdds => Set<RaceOdds>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Bet> Bets => Set<Bet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply Infrastructure entity configurations
        modelBuilder.ApplyConfiguration(new RaceConfiguration());
        modelBuilder.ApplyConfiguration(new RaceOddsConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new BetConfiguration());
    }
}