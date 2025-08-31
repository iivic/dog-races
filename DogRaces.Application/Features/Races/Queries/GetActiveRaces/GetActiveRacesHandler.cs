using DogRaces.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Queries.GetActiveRaces;

public class GetActiveRacesHandler : IRequestHandler<GetActiveRacesQuery, GetActiveRacesResponse>
{
    private readonly IDogRacesContext _context;

    public GetActiveRacesHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<GetActiveRacesResponse> Handle(GetActiveRacesQuery request, CancellationToken cancellationToken)
    {
        var races = await _context.Races
            .Include(r => r.RaceOdds)
            .Where(r => r.IsActive)
            .OrderBy(r => r.StartTime)
            .ToListAsync(cancellationToken);

        var activeRaceDtos = races.Select(race => new ActiveRaceDto(
            race.Id,
            race.RaceName,
            race.Status.ToString(),
            race.StartTime,
            race.EndTime,
            race.HasStarted,
            race.HasEnded,
            race.RaceOdds.Select(ro => new RaceOddsDto(
                ro.Id,
                ro.Selection,
                ro.Odds,
                ro.BetType.ToString()
            )).OrderBy(ro => ro.BetType).ThenBy(ro => ro.Selection).ToList(),
            race.Result
        )).ToList();

        return new GetActiveRacesResponse(
            activeRaceDtos,
            activeRaceDtos.Count
        );
    }
}