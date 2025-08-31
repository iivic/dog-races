using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Tickets.Commands.ProcessUnprocessedTickets;
using DogRaces.Application.Features.Wallet.Commands.ResetWallet;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Tickets;

public class TicketProcessingIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public TicketProcessingIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithWinningTicket_ShouldPayoutToWallet()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        
        // Reset wallet
        await sender.Send(new ResetWalletCommand(100m));
        
        // Create test data
        var race = new Race(0, DateTimeOffset.UtcNow.AddMinutes(-5), 300);
        context.Races.Add(race);
        await context.SaveChangesAsync();
        
        var bet = new Bet(0, race.Id, Guid.NewGuid(), 1, BetType.Winner, 2.5m);
        var ticket = Ticket.Create(10m, [bet]);
        ticket.Approve();
        
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
        
        // Simulate finished race with winning bet
        race.CloseBetting();
        race.StartRace();
        race.FinishRace();
        bet.ProcessResult(new int[] { 1, 2, 3 }); // Selection 1 wins
        await context.SaveChangesAsync();

        // Act
        var result = await sender.Send(new ProcessUnprocessedTicketsCommand());

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(1, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.True(result.TotalPayouts > 0);

        // Check ticket status changed to Won
        var updatedTicket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.Equal(TicketStatus.Won, updatedTicket.Status);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithLosingTicket_ShouldNotPayout()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        
        // Reset wallet
        await sender.Send(new ResetWalletCommand(100m));
        
        // Create test data
        var race = new Race(0, DateTimeOffset.UtcNow.AddMinutes(-5), 300);
        context.Races.Add(race);
        await context.SaveChangesAsync();
        
        var bet = new Bet(0, race.Id, Guid.NewGuid(), 2, BetType.Winner, 3.0m);
        var ticket = Ticket.Create(10m, [bet]);
        ticket.Approve();
        
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
        
        // Simulate finished race with losing bet
        race.CloseBetting();
        race.StartRace();
        race.FinishRace();
        bet.ProcessResult(new int[] { 1, 3, 4 }); // Selection 2 loses
        await context.SaveChangesAsync();

        // Act
        var result = await sender.Send(new ProcessUnprocessedTicketsCommand());

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(1, result.LosingTickets);
        Assert.Equal(0, result.TotalPayouts);

        // Check ticket status changed to Lost
        var updatedTicket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id);
        Assert.NotNull(updatedTicket);
        Assert.Equal(TicketStatus.Lost, updatedTicket.Status);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithNoUnprocessedTickets_ShouldReturnZero()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Act
        var result = await sender.Send(new ProcessUnprocessedTicketsCommand());

        // Assert
        Assert.Equal(0, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.Equal(0, result.TotalPayouts);
    }
}