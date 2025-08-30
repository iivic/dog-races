using DogRaces.Application.Data;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Commands.CreateScheduledRace;

public class CreateScheduledRaceHandler : IRequestHandler<CreateScheduledRaceCommand, CreateScheduledRaceResponse>
{
    private readonly IDogRacesContext _context;

    public CreateScheduledRaceHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<CreateScheduledRaceResponse> Handle(CreateScheduledRaceCommand request, CancellationToken cancellationToken)
    {
        // Use hardcoded values for now - can be made configurable later if needed
        var intervalSeconds = 5;  // 5 seconds between race end and next start
        var durationInSeconds = 10;   // 10 second race duration
        
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
        var race = new Race(
            id: 0,
            startTime: nextStartTime,
            raceDurationInSeconds: durationInSeconds
        );
        
        race.CreateRaceOdds();
        _context.Races.Add(race);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new CreateScheduledRaceResponse(
            race.Id,
            race.RaceName,
            race.StartTime,
            race.EndTime
        );
    }
}
