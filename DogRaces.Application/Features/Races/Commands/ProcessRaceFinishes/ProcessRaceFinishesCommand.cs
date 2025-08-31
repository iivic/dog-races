using MediatR;

namespace DogRaces.Application.Features.Races.Commands.ProcessRaceFinishes;

public record ProcessRaceFinishesCommand : IRequest<ProcessRaceFinishesResponse>;

public record ProcessRaceFinishesResponse(
    int RacesProcessed,
    int RacesFinished
);