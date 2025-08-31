using MediatR;

namespace DogRaces.Application.Features.Races.Queries.GetActiveRaceCount;

public record GetActiveRaceCountQuery : IRequest<int>;