using DogRaces.Application.Data;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogRaces.Application.Features.Tickets.Queries.GetTickets;

/// <summary>
/// Handler for getting tickets with optional filtering and pagination
/// </summary>
public class GetTicketsHandler : IRequestHandler<GetTicketsQuery, GetTicketsResponse>
{
    private readonly IDogRacesContext _context;

    public GetTicketsHandler(IDogRacesContext context)
    {
        _context = context;
    }

    public async Task<GetTicketsResponse> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .Include(t => t.Bets)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(request.Status) && 
            Enum.TryParse<TicketStatus>(request.Status, true, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        if (request.Offset.HasValue)
        {
            query = query.Skip(request.Offset.Value);
        }

        if (request.Limit.HasValue)
        {
            query = query.Take(request.Limit.Value);
        }

        // Order by creation date (newest first)
        query = query.OrderByDescending(t => t.CreatedAt);

        var tickets = await query.ToListAsync(cancellationToken);

        var ticketDtos = tickets.Select(t => new TicketDto(
            t.Id,
            t.Status.ToString(),
            t.TotalStake,
            t.TotalPayout,
            t.CreatedAt,
            t.ProcessedAt,
            t.Bets.Select(b => new BetDto(
                b.Id,
                b.RaceId,
                b.Selection,
                b.BetType.ToString(),
                b.Odds,
                b.IsWinning,
                b.CreatedAt
            ))
        ));

        return new GetTicketsResponse(ticketDtos, totalCount);
    }
}
