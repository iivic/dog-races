using MediatR;

namespace DogRaces.Application.Features.Wallet.Commands.ReleaseFunds;

public record ReleaseFundsCommand(decimal Amount, Guid TicketId) : IRequest<ReleaseFundsResponse>;

public record ReleaseFundsResponse(
    string Message,
    string Status
);