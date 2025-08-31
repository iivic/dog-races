using DogRaces.Application.Data;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DogRaces.Application.Features.Races.Commands.ProcessRaceFinishes;

public sealed class ProcessRaceFinishesHandler : IRequestHandler<ProcessRaceFinishesCommand, ProcessRaceFinishesResponse>
{
    private readonly IDogRacesContext _context;
    private readonly ILogger<ProcessRaceFinishesHandler> _logger;

    public ProcessRaceFinishesHandler(
        IDogRacesContext context,
        ILogger<ProcessRaceFinishesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProcessRaceFinishesResponse> Handle(ProcessRaceFinishesCommand request, CancellationToken cancellationToken)
    {
        var racesToFinish = await GetRacesReadyToFinish(cancellationToken);
        var racesFinished = ProcessRaceFinishes(racesToFinish);

        await _context.SaveChangesAsync(cancellationToken);
        return new ProcessRaceFinishesResponse(racesToFinish.Count, racesFinished);
    }

    private async Task<List<Race>> GetRacesReadyToFinish(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Races
            .Include(r => r.Bets)
            .Where(r => r.Status == RaceStatus.Running && r.EndTime <= now)
            .OrderBy(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    private int ProcessRaceFinishes(List<Race> races)
    {
        var successCount = 0;

        foreach (var race in races)
        {
            race.FinishRace();
            successCount++;
            _logger.LogInformation(
                "🏆 Finished race {RaceId}: {RaceName} - Results: [{Results}]", 
                race.Id,
                race.RaceName, 
                string.Join(", ", race.Result!));
        }

        return successCount;
    }
}