using MediatR;

namespace DogRaces.Application.Features.Races.Commands.EnsureMinimumRaces;

public record EnsureMinimumRacesCommand : IRequest<EnsureMinimumRacesResponse>;

public record EnsureMinimumRacesResponse(
    int ActiveRacesBefore,
    int RacesCreated,
    int ActiveRacesAfter
);