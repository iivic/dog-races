using DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;
using DogRaces.Application.Features.Races.Queries.GetActiveRaceCount;
using MediatR;
using DogRaces.Application.Data;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Commands.EnsureMinimumRaces;

public class EnsureMinimumRacesHandler : IRequestHandler<EnsureMinimumRacesCommand, EnsureMinimumRacesResponse>
{
    private readonly IDogRacesContext _context;
    private readonly ISender _sender;

    public EnsureMinimumRacesHandler(IDogRacesContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<EnsureMinimumRacesResponse> Handle(EnsureMinimumRacesCommand request, CancellationToken cancellationToken)
    {
        var activeRacesBefore = await _sender.Send(new GetActiveRaceCountQuery(), cancellationToken);
        var config = await _sender.Send(new GetGlobalConfigurationQuery(), cancellationToken);
        var minActiveRounds = config.MinNumberOfActiveRounds;

        var races = await CreateRaces(minActiveRounds - activeRacesBefore, cancellationToken);
        var racesCreated = races.Count;
        
        var activeRacesAfter = activeRacesBefore + racesCreated;

        _context.Races.AddRange(races);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new EnsureMinimumRacesResponse(
            activeRacesBefore,
            racesCreated,
            activeRacesAfter
        );
    }

    private async Task<List<Race>> CreateRaces(int numberOfRaces, CancellationToken cancellationToken)
    {
        // Use hardcoded values for now - can be made configurable later if needed
        var intervalSeconds = 5;  // 5 seconds between race end and next start
        var durationInSeconds = 20;   // 20 second race duration
        
        // Find the latest scheduled race to determine next start time
        var latestRace = await _context.Races
            .Where(r => r.Status == RaceStatus.Scheduled)
            .OrderByDescending(r => r.EndTime)
            .FirstOrDefaultAsync(cancellationToken);
        
        // Calculate start time: either now interval, or latest race + interval
        var nextStartTime = latestRace?.EndTime.AddSeconds(intervalSeconds) 
                          ?? DateTimeOffset.UtcNow.AddSeconds(intervalSeconds);
        
        // Ensure we don't schedule in the past
        if (nextStartTime <= DateTimeOffset.UtcNow.AddSeconds(intervalSeconds))
        {
            nextStartTime = DateTimeOffset.UtcNow.AddSeconds(intervalSeconds);
        }
        
        // Create the race with random sequence and name
        var races = new List<Race>();
        for (var i = 0; i < numberOfRaces; i++)
        {
            var race = new Race(
            id: 0,
            startTime: nextStartTime,
            raceDurationInSeconds: durationInSeconds
            );
            
            race.CreateRaceOdds();
            races.Add(race);
            nextStartTime = race.EndTime.AddSeconds(intervalSeconds);
        }
    
        return races;
    }
}
