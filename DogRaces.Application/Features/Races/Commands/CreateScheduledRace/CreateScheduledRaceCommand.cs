using MediatR;

namespace DogRaces.Application.Features.Races.Commands.CreateScheduledRace;

public record CreateScheduledRaceCommand : IRequest<CreateScheduledRaceResponse>;

public record CreateScheduledRaceResponse(
    long RaceId,
    string RaceName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);