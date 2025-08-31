using DogRaces.Domain.Enums;
using MediatR;

namespace DogRaces.Application.Features.Tickets.Commands.PlaceBet;

public record PlaceBetCommand(
    decimal TotalStake,
    IReadOnlyList<BetRequest> Bets
) : IRequest<PlaceBetResponse>;

public record BetRequest(
    long RaceOddsId
);

public record PlaceBetResponse(
    bool Success,
    Guid? TicketId,
    string Message,
    List<string> ValidationErrors
);