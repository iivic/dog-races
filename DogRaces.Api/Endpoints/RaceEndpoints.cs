using DogRaces.Application.Features.Races.Queries.GetActiveRaceCount;
using DogRaces.Application.Features.Races.Queries.GetActiveRaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DogRaces.Api.Endpoints;

public static class RaceEndpoints
{
    public static void MapRaceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/races")
                      .WithTags("Races")
                      .WithDescription("Race management and monitoring endpoints");

        group.MapGet("/active", GetActiveRaces)
             .WithName("GetActiveRaces")
             .WithSummary("Get all currently active races")
             .Produces<GetActiveRacesResponse>();

        group.MapGet("/count", GetActiveRaceCount)
             .WithName("GetActiveRaceCount")
             .WithSummary("Get count of currently active races")
             .Produces<int>();
    }

    private static async Task<IResult> GetActiveRaces(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetActiveRacesQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetActiveRaceCount(ISender sender, CancellationToken cancellationToken)
    {
        var count = await sender.Send(new GetActiveRaceCountQuery(), cancellationToken);
        return Results.Ok(count);
    }
}
