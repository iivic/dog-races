using MediatR;

namespace DogRaces.Application.Features.Wallet.Commands.ResetWallet;

public record ResetWalletCommand(decimal? StartingBalance) : IRequest<ResetWalletResponse>;

public record ResetWalletResponse(
    string Message,
    string Status
);
