namespace DogRaces.Application.Features.Tickets.Queries.GetTickets;

/// <summary>
/// Response containing list of tickets
/// </summary>
public record GetTicketsResponse(
    IEnumerable<TicketDto> Tickets,
    int TotalCount
);

/// <summary>
/// DTO representing a ticket with its bets
/// </summary>
public record TicketDto(
    Guid Id,
    string Status,
    decimal TotalStake,
    decimal? TotalPayout,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    IEnumerable<BetDto> Bets
);

/// <summary>
/// DTO representing a bet within a ticket
/// </summary>
public record BetDto(
    long Id,
    long RaceId,
    int Selection,
    string BetType,
    decimal Odds,
    bool? IsWinning,
    DateTimeOffset CreatedAt
);
