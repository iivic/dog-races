using MediatR;

namespace DogRaces.Application.Features.Wallet.Queries.GetTransactionHistory;

public record GetTransactionHistoryQuery : IRequest<GetTransactionHistoryResponse>;

public record GetTransactionHistoryResponse(
    IEnumerable<TransactionDto> Transactions
);

public record TransactionDto(
    Guid Id,
    Guid TicketId,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    decimal ReservedAfter,
    string Description,
    DateTimeOffset CreatedAt
);
