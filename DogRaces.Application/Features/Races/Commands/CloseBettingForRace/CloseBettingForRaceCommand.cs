using MediatR;

namespace DogRaces.Application.Features.Races.Commands.CloseBettingForRace;

public record CloseBettingForRaceCommand(long RaceId) : IRequest<CloseBettingForRaceResponse>;

public record CloseBettingForRaceResponse(
    long RaceId,
    string RaceName,
    string NewStatus,
    bool Success
);