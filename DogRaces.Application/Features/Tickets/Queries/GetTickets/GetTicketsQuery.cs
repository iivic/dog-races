using MediatR;

namespace DogRaces.Application.Features.Tickets.Queries.GetTickets;

/// <summary>
/// Query to get all tickets with optional filtering
/// </summary>
public record GetTicketsQuery(
    int? Limit = null,
    int? Offset = null,
    string? Status = null
) : IRequest<GetTicketsResponse>;
