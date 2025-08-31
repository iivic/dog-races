using DogRaces.Application.Data;
using DogRaces.Application.Features.Tickets.Commands.ProcessUnprocessedTickets;
using DogRaces.Application.Interfaces;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DogRaces.Application.UnitTests.Features.Tickets;

public class ProcessUnprocessedTicketsHandlerTests
{
    private readonly Mock<IDogRacesContext> _mockContext;
    private readonly Mock<IWalletService> _mockWalletService;
    private readonly Mock<ILogger<ProcessUnprocessedTicketsHandler>> _mockLogger;
    private readonly ProcessUnprocessedTicketsHandler _handler;
    private readonly Mock<DbSet<Ticket>> _mockTicketSet;

    public ProcessUnprocessedTicketsHandlerTests()
    {
        _mockContext = new Mock<IDogRacesContext>();
        _mockWalletService = new Mock<IWalletService>();
        _mockLogger = new Mock<ILogger<ProcessUnprocessedTicketsHandler>>();
        _mockTicketSet = new Mock<DbSet<Ticket>>();

        _mockContext.Setup(x => x.Tickets).Returns(_mockTicketSet.Object);

        _handler = new ProcessUnprocessedTicketsHandler(
            _mockContext.Object,
            _mockWalletService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithNoUnprocessedTickets_ShouldReturnZeroResults()
    {
        // Arrange
        var emptyTickets = new List<Ticket>().AsQueryable();
        SetupMockDbSet(_mockTicketSet, emptyTickets);

        var command = new ProcessUnprocessedTicketsCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.Equal(0m, result.TotalPayouts);

        _mockWalletService.Verify(x => x.AddPayout(It.IsAny<decimal>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithWinningTicket_ShouldProcessCorrectly()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var race = CreateRace(1, [1, 2, 3]); // Selection 1 wins
        var bet = CreateBet(1, race.Id, ticketId, 1, BetType.Winner, 2.0m, isWinning: true);
        var ticket = CreateTicket(ticketId, TicketStatus.Success, 10m, [bet]);

        // Set up the ticket to calculate payout correctly
        ticket.ProcessResult(); // This should set TotalPayout based on winning bets

        var tickets = new List<Ticket> { ticket }.AsQueryable();
        SetupMockDbSet(_mockTicketSet, tickets);

        var command = new ProcessUnprocessedTicketsCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(1, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.True(result.TotalPayouts > 0);

        _mockWalletService.Verify(x => x.AddPayout(It.IsAny<decimal>(), ticketId), Times.Once);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLosingTicket_ShouldProcessCorrectly()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var race = CreateRace(1, [1, 2, 3]); // Selection 4 loses
        var bet = CreateBet(1, race.Id, ticketId, 4, BetType.Winner, 2.0m, isWinning: false);
        var ticket = CreateTicket(ticketId, TicketStatus.Success, 10m, [bet]);

        ticket.ProcessResult(); // This should set status to Lost

        var tickets = new List<Ticket> { ticket }.AsQueryable();
        SetupMockDbSet(_mockTicketSet, tickets);

        var command = new ProcessUnprocessedTicketsCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(1, result.LosingTickets);
        Assert.Equal(0m, result.TotalPayouts);

        _mockWalletService.Verify(x => x.AddPayout(It.IsAny<decimal>(), It.IsAny<Guid>()), Times.Never);
        _mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMixedTickets_ShouldProcessBothCorrectly()
    {
        // Arrange
        var winningTicketId = Guid.NewGuid();
        var losingTicketId = Guid.NewGuid();
        var race = CreateRace(1, [1, 2, 3]);

        // Winning ticket
        var winningBet = CreateBet(1, race.Id, winningTicketId, 1, BetType.Winner, 2.0m, isWinning: true);
        var winningTicket = CreateTicket(winningTicketId, TicketStatus.Success, 10m, [winningBet]);
        winningTicket.ProcessResult();

        // Losing ticket
        var losingBet = CreateBet(2, race.Id, losingTicketId, 4, BetType.Winner, 2.0m, isWinning: false);
        var losingTicket = CreateTicket(losingTicketId, TicketStatus.Success, 10m, [losingBet]);
        losingTicket.ProcessResult();

        var tickets = new List<Ticket> { winningTicket, losingTicket }.AsQueryable();
        SetupMockDbSet(_mockTicketSet, tickets);

        var command = new ProcessUnprocessedTicketsCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.ProcessedTickets);
        Assert.Equal(1, result.WinningTickets);
        Assert.Equal(1, result.LosingTickets);
        Assert.True(result.TotalPayouts > 0);

        _mockWalletService.Verify(x => x.AddPayout(It.IsAny<decimal>(), winningTicketId), Times.Once);
        _mockWalletService.Verify(x => x.AddPayout(It.IsAny<decimal>(), losingTicketId), Times.Never);
    }

    private static void SetupMockDbSet<T>(Mock<DbSet<T>> mockSet, IQueryable<T> data) where T : class
    {
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    private static Race CreateRace(long id, int[] result)
    {
        var race = new Race(id, DateTimeOffset.UtcNow.AddMinutes(-10), 30);
        race.CloseBetting();
        race.StartRace();
        race.FinishRace();

        // Set the result using reflection
        var resultField = typeof(Race).GetField("_result", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        resultField?.SetValue(race, result);

        return race;
    }

    private static Bet CreateBet(long id, long raceId, Guid ticketId, int selection, BetType betType, decimal odds, bool isWinning)
    {
        var bet = new Bet(id, raceId, ticketId, selection, betType, odds);

        // Set IsWinning using reflection
        var isWinningField = typeof(Bet).GetProperty("IsWinning");
        isWinningField?.SetValue(bet, isWinning);

        return bet;
    }

    private static Ticket CreateTicket(Guid id, TicketStatus status, decimal totalStake, IReadOnlyList<Bet> bets)
    {
        var ticket = Ticket.Create(totalStake, bets);

        // Set the ID and status using reflection
        var idField = typeof(Ticket).GetProperty("Id");
        idField?.SetValue(ticket, id);

        var statusField = typeof(Ticket).GetProperty("Status");
        statusField?.SetValue(ticket, status);

        return ticket;
    }
}