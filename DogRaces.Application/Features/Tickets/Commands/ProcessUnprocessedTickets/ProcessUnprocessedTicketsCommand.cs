using MediatR;

namespace DogRaces.Application.Features.Tickets.Commands.ProcessUnprocessedTickets;

/// <summary>
/// Command to process all unprocessed tickets where all associated races are finished
/// </summary>
public record ProcessUnprocessedTicketsCommand() : IRequest<ProcessUnprocessedTicketsResponse>;

/// <summary>
/// Response for unprocessed ticket processing
/// </summary>
public record ProcessUnprocessedTicketsResponse(
    int ProcessedTickets,
    int WinningTickets,
    int LosingTickets,
    decimal TotalPayouts
);
