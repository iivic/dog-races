using DogRaces.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;

public class GetGlobalConfigurationHandler : IRequestHandler<GetGlobalConfigurationQuery, GetGlobalConfigurationResponse>
{
    private readonly IDogRacesContext _context;

    public GetGlobalConfigurationHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<GetGlobalConfigurationResponse> Handle(GetGlobalConfigurationQuery request, CancellationToken cancellationToken)
    {
        var config = await _context.GlobalConfigurations.SingleAsync(cancellationToken);

        return new GetGlobalConfigurationResponse(
            config.MinTicketStake,
            config.MaxTicketWin,
            config.MinNumberOfActiveRounds
        );
    }
}