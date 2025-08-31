using DogRaces.Application.Features.Tickets.Commands.PlaceBet;
using MediatR;

namespace DogRaces.Api.Endpoints;

/// <summary>
/// Ticket API endpoints for betting operations
/// </summary>
public static class TicketEndpoints
{
    /// <summary>
    /// Map ticket endpoints to the application
    /// </summary>
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var tickets = app.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .WithOpenApi();

        tickets.MapPost("/place-bet", PlaceBet)
            .WithName("PlaceBet")
            .WithSummary("Place a betting ticket with multiple bets");

        return app;
    }

    /// <summary>
    /// Place a betting ticket
    /// </summary>
    private static async Task<IResult> PlaceBet(PlaceBetCommand request, ISender sender)
    {
        var response = await sender.Send(request);

        if (response.Success)
        {
            return Results.Ok(response);
        }

        return Results.BadRequest(response);
    }
}