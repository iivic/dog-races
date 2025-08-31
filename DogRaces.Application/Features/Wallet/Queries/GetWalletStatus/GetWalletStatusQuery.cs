using MediatR;

namespace DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;

public record GetWalletStatusQuery : IRequest<GetWalletStatusResponse>;

public record GetWalletStatusResponse(
    string Status,
    decimal AvailableBalance,
    decimal ReservedAmount,
    decimal TotalFunds
);