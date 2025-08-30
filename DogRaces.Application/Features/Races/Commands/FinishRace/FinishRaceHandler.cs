using DogRaces.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Commands.FinishRace;

internal sealed class FinishRaceHandler : IRequestHandler<FinishRaceCommand, FinishRaceResponse>
{
    private readonly IDogRacesContext _context;

    public FinishRaceHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<FinishRaceResponse> Handle(FinishRaceCommand request, CancellationToken cancellationToken)
    {
        var race = await _context.Races
            .FirstOrDefaultAsync(r => r.Id == request.RaceId, cancellationToken);

        if (race == null)
        {
            return new FinishRaceResponse(
                request.RaceId,
                "Unknown",
                "NotFound",
                [],
                false
            );
        }

        race.FinishRace();
        await _context.SaveChangesAsync(cancellationToken);

            return new FinishRaceResponse(
            race.Id,
            race.RaceName,
            race.Status.ToString(),
            race.Result!,
            true
        );
    }
}
