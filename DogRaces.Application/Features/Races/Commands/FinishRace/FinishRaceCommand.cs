using MediatR;

namespace DogRaces.Application.Features.Races.Commands.FinishRace;

public record FinishRaceCommand(long RaceId) : IRequest<FinishRaceResponse>;

public record FinishRaceResponse(
    long RaceId,
    string RaceName,
    string NewStatus,
    int[] Result,
    bool Success
);
