using DogRaces.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Commands.StartRace;

public class StartRaceHandler : IRequestHandler<StartRaceCommand, StartRaceResponse>
{
    private readonly IDogRacesContext _context;

    public StartRaceHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<StartRaceResponse> Handle(StartRaceCommand request, CancellationToken cancellationToken)
    {
        var race = await _context.Races
            .FirstOrDefaultAsync(r => r.Id == request.RaceId, cancellationToken);

        if (race == null)
        {
            return new StartRaceResponse(
                request.RaceId,
                "Unknown",
                "NotFound",
                false
            );
        }

        race.StartRace();
        await _context.SaveChangesAsync(cancellationToken);

        return new StartRaceResponse(
            race.Id,
            race.RaceName,
            race.Status.ToString(),
            true
        );
    }
}