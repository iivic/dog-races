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

        wallet.MapGet("/status", GetWalletStatus)
            .WithName("GetWalletStatus")
            .WithSummary("Get wallet status and balance information");

        wallet.MapGet("/transactions", GetTransactionHistory)
            .WithName("GetTransactionHistory")
            .WithSummary("Get wallet transaction history");

        wallet.MapPost("/reset", ResetWallet)
            .WithName("ResetWallet")
            .WithSummary("Reset wallet with new starting balance");

        wallet.MapPost("/test-reserve", TestReserveFunds)
            .WithName("TestReserveFunds")
            .WithSummary("Test reserving funds (for testing purposes)");

        wallet.MapPost("/test-release", TestReleaseFunds)
            .WithName("TestReleaseFunds")
            .WithSummary("Test releasing reserved funds (for testing purposes)");

        return app;
    }

    /// <summary>
    /// Get wallet status and balance information
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
    /// Test reserving funds for a ticket
    /// </summary>
    private static async Task<IResult> TestReserveFunds(ReserveFundsCommand request, IMediator mediator)
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }

    /// <summary>
    /// Test releasing reserved funds for a ticket
    /// </summary>
    private static async Task<IResult> TestReleaseFunds(ReleaseFundsCommand request, IMediator mediator)
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
}