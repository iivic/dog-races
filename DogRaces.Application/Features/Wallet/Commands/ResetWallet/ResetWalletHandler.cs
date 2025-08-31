using DogRaces.Application.Interfaces;
using MediatR;

namespace DogRaces.Application.Features.Wallet.Commands.ResetWallet;

public class ResetWalletHandler : IRequestHandler<ResetWalletCommand, ResetWalletResponse>
{
    private readonly IWalletService _walletService;

    public ResetWalletHandler(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public Task<ResetWalletResponse> Handle(ResetWalletCommand request, CancellationToken cancellationToken)
    {
        var balance = request.StartingBalance ?? 100m;
        _walletService.ResetWallet(balance);

        var response = new ResetWalletResponse(
            $"Wallet reset with balance: {balance}",
            _walletService.GetWalletStatus()
        );

        return Task.FromResult(response);
    }
}