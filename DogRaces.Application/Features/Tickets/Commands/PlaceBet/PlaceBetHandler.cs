using System.Runtime.CompilerServices;
using DogRaces.Application.Data;
using DogRaces.Application.Interfaces;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DogRaces.Application.Features.Tickets.Commands.PlaceBet;

public class PlaceBetHandler : IRequestHandler<PlaceBetCommand, PlaceBetResponse>
{
    private readonly IDogRacesContext _context;
    private readonly IWalletService _walletService;
    private readonly ILogger<PlaceBetHandler> _logger;

    public PlaceBetHandler(
        IDogRacesContext context,
        IWalletService walletService,
        ILogger<PlaceBetHandler> logger)
    {
        _context = context;
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<PlaceBetResponse> Handle(PlaceBetCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing bet placement for stake {Stake} with {BetCount} bets", 
            request.TotalStake, request.Bets.Count);

        try
        {
            var validationResult = await ValidateTicketAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Ticket validation failed: {Errors}", string.Join(", ", validationResult.Errors));
                return new PlaceBetResponse(false, null, "Ticket validation failed", validationResult.Errors);
            }

            var ticketId = Guid.NewGuid();
            var fundsReserved = _walletService.TryReserveForTicket(request.TotalStake, ticketId);
            
            if (!fundsReserved)
            {
                _logger.LogWarning("Insufficient funds for ticket {TicketId}, stake: {Stake}", ticketId, request.TotalStake);
                return new PlaceBetResponse(
                    false, 
                    null, 
                    "Insufficient funds", 
                    [$"Unable to reserve {request.TotalStake} from wallet"]);
            }

            try
            {
                // Step 3: Revalidacija tiketa (2.3) - Check if races haven't started yet
                var raceIds = validationResult.ValidatedBets.Select(vb => vb.RaceOdds.RaceId).Distinct().ToList();
                var isStillValid = await RevalidateTicketTimingAsync(raceIds, cancellationToken);
                
                if (!isStillValid)
                {
                    // Release reserved funds if revalidation fails
                    _walletService.ReleaseForTicket(request.TotalStake, ticketId);
                    _logger.LogWarning("Ticket revalidation failed for ticket {TicketId} - races have started", ticketId);
                    
                    return new PlaceBetResponse(
                        false, 
                        null, 
                        "Betting closed", 
                        ["One or more races have started or closed for betting"]);
                }

                // Step 4: Create ticket and finalize (2.4)
                var bets = validationResult.ValidatedBets.Select(validatedBet => new Bet(
                    id: 0,
                    raceId: validatedBet.RaceOdds.RaceId,
                    ticketId: ticketId,
                    selection: validatedBet.RaceOdds.Selection,
                    betType: validatedBet.RaceOdds.BetType,
                    odds: validatedBet.RaceOdds.Odds
                )).ToList();

                var ticket = Ticket.Create(request.TotalStake, bets);
                ticket.Approve();
                
                _walletService.TryCommitForTicket(request.TotalStake, ticketId);

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully placed bet for ticket {TicketId} with potential win {PotentialWin}", 
                    ticketId,
                    validationResult.PotentialWin);
                
                return new PlaceBetResponse(
                    true, 
                    ticketId, 
                    $"Bet placed successfully. Potential win: {validationResult.PotentialWin:F2}", 
                    new List<string>());
            }
            catch (Exception ex)
            {
                // If anything fails after reserving funds, release them
                _walletService.ReleaseForTicket(request.TotalStake, ticketId);
                _logger.LogError(ex, "Error finalizing ticket {TicketId}", ticketId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bet placement");
            return new PlaceBetResponse(
                false, 
                null, 
                "Internal error occurred", 
                new List<string> { "Please try again later" });
        }
    }

    private async Task<TicketValidationResult> ValidateTicketAsync(PlaceBetCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        var config = await _context.GlobalConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
        {
            errors.Add("System configuration not found");
            return new TicketValidationResult(false, errors, new List<(BetRequest Bet, RaceOdds RaceOdds)>(), 0);
        }

        if (request.TotalStake < config.MinTicketStake)
        {
            errors.Add($"Minimum stake is {config.MinTicketStake}");
        }

        if (request.TotalStake <= 0)
        {
            errors.Add("Total stake must be greater than 0");
        }

        if (!request.Bets.Any())
        {
            errors.Add("At least one bet is required");
        }

        var raceOddsIds = request.Bets.Select(b => b.RaceOddsId).ToList();
        var raceOdds = await _context.RaceOdds
            .Where(ro => raceOddsIds.Contains(ro.Id))
            .ToListAsync(cancellationToken);

        decimal totalOdds = 1;
        var validatedBets = new List<(BetRequest Bet, RaceOdds RaceOdds)>();

        foreach (var bet in request.Bets)
        {
            var odds = raceOdds.FirstOrDefault(ro => ro.Id == bet.RaceOddsId);

            if (odds == null)
            {
                errors.Add($"Race odds with ID {bet.RaceOddsId} not found");
                continue;
            }

            totalOdds *= odds.Odds;
            validatedBets.Add((bet, odds));
        }

        var potentialWin = request.TotalStake * totalOdds;

        // Step 4: Validate maximum potential win
        if (potentialWin > config.MaxTicketWin)
        {
            errors.Add($"Maximum potential win is {config.MaxTicketWin}. This ticket could win {potentialWin:F2}");
        }

        var isValid = errors.Count == 0;
        return new TicketValidationResult(isValid, errors, validatedBets, potentialWin);
    }

    /// <summary>
    /// Check if races haven't started yet
    /// </summary>
    private async Task<bool> RevalidateTicketTimingAsync(List<long> raceIds, CancellationToken cancellationToken)
    {
        return await _context.Races
            .Where(r => raceIds.Contains(r.Id))
            .AllAsync(r => r.Status == RaceStatus.Scheduled, cancellationToken);
    }
}

/// <summary>
/// Result of ticket validation within the handler
/// </summary>
internal record TicketValidationResult(
    bool IsValid,
    List<string> Errors,
    List<(BetRequest Bet, RaceOdds RaceOdds)> ValidatedBets,
    decimal PotentialWin
);
