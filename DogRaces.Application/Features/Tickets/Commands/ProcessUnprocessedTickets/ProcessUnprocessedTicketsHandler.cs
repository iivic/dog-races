using DogRaces.Application.Data;
using DogRaces.Application.Interfaces;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DogRaces.Application.Features.Tickets.Commands.ProcessUnprocessedTickets;

/// <summary>
/// Handler for processing unprocessed tickets where all associated races are finished
/// </summary>
public class
    ProcessUnprocessedTicketsHandler : IRequestHandler<ProcessUnprocessedTicketsCommand,
    ProcessUnprocessedTicketsResponse>
{
    private readonly IDogRacesContext _context;
    private readonly IWalletService _walletService;
    private readonly ILogger<ProcessUnprocessedTicketsHandler> _logger;

    public ProcessUnprocessedTicketsHandler(
        IDogRacesContext context,
        IWalletService walletService,
        ILogger<ProcessUnprocessedTicketsHandler> logger)
    {
        _context = context;
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<ProcessUnprocessedTicketsResponse> Handle(ProcessUnprocessedTicketsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking for unprocessed tickets with all races finished");

        // Find tickets that are in Success status (placed successfully) but not yet processed (Won/Lost)
        // and where all associated races are finished
        var unprocessedTickets = await _context.Tickets
            .Include(t => t.Bets)
            .Where(t => t.Status == TicketStatus.Success)
            .Where(t => t.Bets.All(b => b.IsWinning.HasValue))
            .ToListAsync(cancellationToken);

        if (unprocessedTickets.Count == 0)
        {
            _logger.LogDebug("No unprocessed tickets found");
            return new ProcessUnprocessedTicketsResponse(0, 0, 0, 0m);
        }

        _logger.LogInformation("Found {TicketCount} unprocessed tickets ready for processing",
            unprocessedTickets.Count);

        int winningTickets = 0;
        int losingTickets = 0;
        decimal totalPayouts = 0m;

        foreach (var ticket in unprocessedTickets)
        {
            _logger.LogDebug("Processing ticket {TicketId} with {BetCount} bets", ticket.Id, ticket.Bets.Count);
            ticket.ProcessResult();
            var isWinningTicket = ticket.IsWinning();

            if (isWinningTicket)
            {
                var payoutAmount = ticket.TotalPayout!.Value;
                _walletService.AddPayout(payoutAmount, ticket.Id);

                winningTickets++;
                totalPayouts += payoutAmount;
                _logger.LogInformation("Ticket {TicketId} won with payout {Payout}", ticket.Id, payoutAmount);
            }
            else
            {
                losingTickets++;
                _logger.LogDebug("Ticket {TicketId} lost", ticket.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed {ProcessedTickets} tickets: {WinningTickets} won, {LosingTickets} lost, total payouts: {TotalPayouts}",
            unprocessedTickets.Count,
            winningTickets,
            losingTickets,
            totalPayouts);

        return new ProcessUnprocessedTicketsResponse(unprocessedTickets.Count, winningTickets, losingTickets,
            totalPayouts);
    }
}