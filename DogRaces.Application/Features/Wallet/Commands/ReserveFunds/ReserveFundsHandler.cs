using DogRaces.Application.Interfaces;
using MediatR;

namespace DogRaces.Application.Features.Wallet.Commands.ReserveFunds;

public class ReserveFundsHandler : IRequestHandler<ReserveFundsCommand, ReserveFundsResponse>
{
    private readonly IWalletService _walletService;

    public ReserveFundsHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public Task<ReserveFundsResponse> Handle(ReserveFundsCommand request, CancellationToken cancellationToken)
    {
        var ticketId = Guid.NewGuid();
        var success = _walletService.TryReserveForTicket(request.Amount, ticketId);
        
        var response = new ReserveFundsResponse(
            success,
            ticketId,
            request.Amount,
            _walletService.GetWalletStatus()
        );

        return Task.FromResult(response);
    }
}
