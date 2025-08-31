using MediatR;

namespace DogRaces.Application.Features.Tickets.Commands.ProcessTicketResults;

/// <summary>
/// Command to process all tickets for a finished race
/// </summary>
public record ProcessTicketResultsCommand(
    long RaceId
) : IRequest<ProcessTicketResultsResponse>;

/// <summary>
/// Response for ticket processing
/// </summary>
public record ProcessTicketResultsResponse(
    int ProcessedTickets,
    int WinningTickets,
    int LosingTickets,
    decimal TotalPayouts
);