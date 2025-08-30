using MediatR;

namespace DogRaces.Application.Features.Wallet.Commands.ReserveFunds;

public record ReserveFundsCommand(decimal Amount) : IRequest<ReserveFundsResponse>;

public record ReserveFundsResponse(
    bool Success,
    Guid TicketId,
    decimal Amount,
    string Status
);
