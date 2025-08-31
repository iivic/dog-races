using DogRaces.Application.Data;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DogRaces.Application.Features.Races.Commands.ProcessRaceStarts;

public sealed class ProcessRaceStartsHandler : IRequestHandler<ProcessRaceStartsCommand, ProcessRaceStartsResponse>
{
    private readonly IDogRacesContext _context;
    private readonly ILogger<ProcessRaceStartsHandler> _logger;

    public ProcessRaceStartsHandler(
        IDogRacesContext context,
        ILogger<ProcessRaceStartsHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProcessRaceStartsResponse> Handle(ProcessRaceStartsCommand request, CancellationToken cancellationToken)
    {
        var racesToStart = await GetRacesReadyToStart(cancellationToken);
        var racesStarted = ProcessRaceStarts(racesToStart);

        await _context.SaveChangesAsync(cancellationToken);
        return new ProcessRaceStartsResponse(racesToStart.Count, racesStarted);
    }

    private async Task<List<Race>> GetRacesReadyToStart(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Races
            .Where(r => r.Status == RaceStatus.BettingClosed && r.StartTime <= now)
            .OrderBy(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    private int ProcessRaceStarts(IReadOnlyList<Race> races)
    {
        var successCount = 0;

        foreach (var race in races)
        {
            race.StartRace();
            successCount++;
            _logger.LogInformation(
                "🚀 Started race {RaceId}: {RaceName}",
                race.Id,
                race.RaceName);
        }

        return successCount;
    }
}