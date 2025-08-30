using DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;
using DogRaces.Application.Features.Races.Commands.CreateScheduledRace;
using DogRaces.Application.Features.Races.Queries.GetActiveRaceCount;
using MediatR;

namespace DogRaces.Application.Features.Races.Commands.EnsureMinimumRaces;

public class EnsureMinimumRacesHandler : IRequestHandler<EnsureMinimumRacesCommand, EnsureMinimumRacesResponse>
{
    private readonly ISender _sender;

    public EnsureMinimumRacesHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<EnsureMinimumRacesResponse> Handle(EnsureMinimumRacesCommand request, CancellationToken cancellationToken)
    {
        var activeRacesBefore = await _sender.Send(new GetActiveRaceCountQuery(), cancellationToken);
        var config = await _sender.Send(new GetGlobalConfigurationQuery(), cancellationToken);
        var minActiveRounds = config.MinNumberOfActiveRounds;
        
        var racesCreated = 0;
        
        while (activeRacesBefore + racesCreated < minActiveRounds)
        {
            await _sender.Send(new CreateScheduledRaceCommand(), cancellationToken);
            racesCreated++;
        }
        
        var activeRacesAfter = activeRacesBefore + racesCreated;
        
        return new EnsureMinimumRacesResponse(
            activeRacesBefore,
            racesCreated,
            activeRacesAfter
        );
    }
}
