using DogRaces.Application.Interfaces;
using MediatR;

namespace DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;

public class GetWalletStatusHandler : IRequestHandler<GetWalletStatusQuery, GetWalletStatusResponse>
{
    private readonly IWalletService _walletService;

    public GetWalletStatusHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public Task<GetWalletStatusResponse> Handle(GetWalletStatusQuery request, CancellationToken cancellationToken)
    {
        var response = new GetWalletStatusResponse(
            _walletService.GetWalletStatus(),
            _walletService.GetAvailableBalance(),
            _walletService.GetReservedAmount(),
            _walletService.GetTotalFunds()
        );

        return Task.FromResult(response);
    }
}