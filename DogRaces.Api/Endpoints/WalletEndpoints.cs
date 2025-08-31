using DogRaces.Application.Features.Wallet.Commands.ReleaseFunds;
using DogRaces.Application.Features.Wallet.Commands.ReserveFunds;
using DogRaces.Application.Features.Wallet.Commands.ResetWallet;
using DogRaces.Application.Features.Wallet.Queries.GetTransactionHistory;
using DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;
using MediatR;

namespace DogRaces.Api.Endpoints;

/// <summary>
/// Wallet API endpoints
/// </summary>
public static class WalletEndpoints
{
    /// <summary>
    /// Map wallet endpoints to the application
    /// </summary>
    public static IEndpointRouteBuilder MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var wallet = app.MapGroup("/api/wallet")
            .WithTags("Wallet")
            .WithOpenApi();

        wallet.MapGet("/balance", GetWalletStatus)
            .WithName("GetWalletBalance")
            .WithSummary("Get wallet balance and status information");

        wallet.MapGet("/transactions", GetTransactionHistory)
            .WithName("GetTransactionHistory")
            .WithSummary("Get wallet transaction history");

        wallet.MapPost("/reset", ResetWallet)
            .WithName("ResetWallet")
            .WithSummary("Reset wallet with new starting balance");

        wallet.MapPost("/reserve", ReserveFunds)
            .WithName("ReserveFunds")
            .WithSummary("Reserve funds for a ticket");

        wallet.MapPost("/release", ReleaseFunds)
            .WithName("ReleaseFunds")
            .WithSummary("Release reserved funds for a ticket");

        return app;
    }

    /// <summary>
    /// Get wallet balance and status information
    /// </summary>
    private static async Task<IResult> GetWalletStatus(IMediator mediator)
    {
        var response = await mediator.Send(new GetWalletStatusQuery());
        return Results.Ok(response);
    }

    /// <summary>
    /// Get wallet transaction history
    /// </summary>
    private static async Task<IResult> GetTransactionHistory(IMediator mediator)
    {
        var response = await mediator.Send(new GetTransactionHistoryQuery());
        return Results.Ok(response);
    }

    /// <summary>
    /// Reset wallet with new starting balance
    /// </summary>
    private static async Task<IResult> ResetWallet(ResetWalletCommand request, IMediator mediator)
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Reserve funds for a ticket
    /// </summary>
    private static async Task<IResult> ReserveFunds(ReserveFundsCommand request, IMediator mediator)
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Release reserved funds for a ticket
    /// </summary>
    private static async Task<IResult> ReleaseFunds(ReleaseFundsCommand request, IMediator mediator)
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
}