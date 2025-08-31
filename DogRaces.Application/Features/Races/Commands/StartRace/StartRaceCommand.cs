using MediatR;

namespace DogRaces.Application.Features.Races.Commands.StartRace;

public record StartRaceCommand(long RaceId) : IRequest<StartRaceResponse>;

public record StartRaceResponse(
    long RaceId,
    string RaceName,
    string NewStatus,
    bool Success
);