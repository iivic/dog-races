using MediatR;

namespace DogRaces.Application.Features.Races.Commands.ProcessRaceStarts;

public record ProcessRaceStartsCommand : IRequest<ProcessRaceStartsResponse>;

public record ProcessRaceStartsResponse(
    int RacesProcessed,
    int RacesStarted
);