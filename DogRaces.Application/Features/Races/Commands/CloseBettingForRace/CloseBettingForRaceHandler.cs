using DogRaces.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Commands.CloseBettingForRace;

internal sealed class CloseBettingForRaceHandler : IRequestHandler<CloseBettingForRaceCommand, CloseBettingForRaceResponse>
{
    private readonly IDogRacesContext _context;

    public CloseBettingForRaceHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<CloseBettingForRaceResponse> Handle(CloseBettingForRaceCommand request, CancellationToken cancellationToken)
    {
        var race = await _context.Races
            .FirstOrDefaultAsync(r => r.Id == request.RaceId, cancellationToken);

        if (race == null)
        {
            return new CloseBettingForRaceResponse(
                request.RaceId,
                "Unknown",
                "NotFound",
                false
            );
        }

        race.CloseBetting();
        await _context.SaveChangesAsync(cancellationToken);

        return new CloseBettingForRaceResponse(
            race.Id,
            race.RaceName,
            race.Status.ToString(),
            true
        );
    }
}
