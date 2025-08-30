using DogRaces.Application.Interfaces;
using MediatR;

namespace DogRaces.Application.Features.Wallet.Queries.GetTransactionHistory;

public class GetTransactionHistoryHandler : IRequestHandler<GetTransactionHistoryQuery, GetTransactionHistoryResponse>
{
    private readonly IWalletService _walletService;

    public GetTransactionHistoryHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public Task<GetTransactionHistoryResponse> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var transactions = _walletService.GetTransactionHistory();
        var transactionDtos = transactions.Select(t => new TransactionDto(
            t.Id,
            t.TicketId,
            t.Type.ToString(),
            t.Amount,
            t.BalanceAfter,
            t.ReservedAfter,
            t.Description,
            t.CreatedAt
        ));

        var response = new GetTransactionHistoryResponse(transactionDtos);
        return Task.FromResult(response);
    }
}
