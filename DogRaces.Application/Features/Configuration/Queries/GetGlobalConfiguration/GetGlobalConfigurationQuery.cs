using MediatR;

namespace DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;

public record GetGlobalConfigurationQuery : IRequest<GetGlobalConfigurationResponse>;

public record GetGlobalConfigurationResponse(
    decimal MinTicketStake,
    decimal MaxTicketWin,
    int MinNumberOfActiveRounds
);