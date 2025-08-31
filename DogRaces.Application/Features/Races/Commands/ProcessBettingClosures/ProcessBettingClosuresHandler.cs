using DogRaces.Application.Data;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DogRaces.Application.Features.Races.Commands.ProcessBettingClosures;

public sealed class ProcessBettingClosuresHandler : IRequestHandler<ProcessBettingClosuresCommand, ProcessBettingClosuresResponse>
{
    private readonly IDogRacesContext _context;
    private readonly ILogger<ProcessBettingClosuresHandler> _logger;

    public ProcessBettingClosuresHandler(
        IDogRacesContext context,
        ILogger<ProcessBettingClosuresHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProcessBettingClosuresResponse> Handle(ProcessBettingClosuresCommand request, CancellationToken cancellationToken)
    {
        var racesToCloseBetting = await GetRacesReadyForBettingClosure(cancellationToken);
        var bettingClosed = ProcessBettingClosures(racesToCloseBetting);

        await _context.SaveChangesAsync(cancellationToken);
        return new ProcessBettingClosuresResponse(racesToCloseBetting.Count, bettingClosed);
    }

    private async Task<List<Race>> GetRacesReadyForBettingClosure(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Races
            .Where(r => r.Status == RaceStatus.Scheduled && r.StartTime <= now.AddSeconds(5))
            .OrderBy(r => r.StartTime)
            .ToListAsync(cancellationToken);
    }

    private int ProcessBettingClosures(List<Race> races)
    {
        var successCount = 0;

        foreach (var race in races)
        {
            race.CloseBetting();
            successCount++;
            _logger.LogInformation("🔒 Closed betting for race {RaceId}: {RaceName}",
                race.Id, race.RaceName);
        }

        return successCount;
    }
}