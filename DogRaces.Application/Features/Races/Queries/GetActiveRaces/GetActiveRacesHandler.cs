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
            race.CalculateOddsFromSequence(),
            race.Result
        )).ToList();

        return new GetActiveRacesResponse(
            activeRaceDtos,
            activeRaceDtos.Count
        );
    }
}
