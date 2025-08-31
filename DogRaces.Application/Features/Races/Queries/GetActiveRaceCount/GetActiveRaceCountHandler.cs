using DogRaces.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Races.Queries.GetActiveRaceCount;

public class GetActiveRaceCountHandler : IRequestHandler<GetActiveRaceCountQuery, int>
{
    private readonly IDogRacesContext _context;

    public GetActiveRaceCountHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetActiveRaceCountQuery request, CancellationToken cancellationToken)
    {
        return await _context.Races
            .CountAsync(r => r.IsActive, cancellationToken);
    }
}