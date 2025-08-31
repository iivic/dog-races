using DogRaces.Application.Interfaces;
using MediatR;

namespace DogRaces.Application.Features.Wallet.Commands.ReleaseFunds;

public class ReleaseFundsHandler : IRequestHandler<ReleaseFundsCommand, ReleaseFundsResponse>
{
    private readonly IWalletService _walletService;

    public ReleaseFundsHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public Task<ReleaseFundsResponse> Handle(ReleaseFundsCommand request, CancellationToken cancellationToken)
    {
        _walletService.ReleaseForTicket(request.Amount, request.TicketId);

        var response = new ReleaseFundsResponse(
            $"Released {request.Amount} for ticket {request.TicketId}",
            _walletService.GetWalletStatus()
        );

        return Task.FromResult(response);
    }
}