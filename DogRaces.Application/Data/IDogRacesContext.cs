using DogRaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Data;

/// <summary>
/// Interface for Dog Races DbContext
/// </summary>
public interface IDogRacesContext
{
    DbSet<Race> Races { get; }
    DbSet<RaceOdds> RaceOdds { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<Bet> Bets { get; }
    DbSet<GlobalConfiguration> GlobalConfigurations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
