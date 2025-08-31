using MediatR;

namespace DogRaces.Application.Features.Races.Queries.GetActiveRaces;

public record GetActiveRacesQuery : IRequest<GetActiveRacesResponse>;

public record GetActiveRacesResponse(
    List<ActiveRaceDto> Races,
    int TotalCount
);

public record ActiveRaceDto(
    long Id,
    string RaceName,
    string Status,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    bool HasStarted,
    bool HasEnded,
    List<RaceOddsDto> RaceOdds,
    int[]? Result
);

public record RaceOddsDto(
    long Id,
    int Selection,
    decimal Odds,
    string BetType
);