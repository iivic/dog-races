using DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;
using MediatR;

namespace DogRaces.Api.Endpoints;

public static class ConfigurationEndpoints
{
    public static void MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configuration")
                      .WithTags("Configuration")
                      .WithDescription("System configuration endpoints");

        group.MapGet("/", GetGlobalConfiguration)
             .WithName("GetGlobalConfiguration")
             .WithSummary("Get current system configuration")
             .Produces<GetGlobalConfigurationResponse>();
    }

    private static async Task<IResult> GetGlobalConfiguration(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGlobalConfigurationQuery(), cancellationToken);
        return Results.Ok(result);
    }
}