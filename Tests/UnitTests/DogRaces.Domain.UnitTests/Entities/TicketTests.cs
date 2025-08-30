using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Xunit;

namespace DogRaces.Domain.UnitTests.Entities;

public class TicketTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateTicket()
    {
        // Arrange
        var bets = new List<Bet>
        {
            CreateSampleBet(1, BetType.Winner, 2.5m),
            CreateSampleBet(2, BetType.Top2, 1.8m)
        };
        var totalStake = 50m;

        // Act
        var ticket = Ticket.Create(totalStake, bets);

        // Assert
        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(TicketStatus.Pending, ticket.Status);
        Assert.Equal(totalStake, ticket.TotalStake);
        Assert.Equal(2, ticket.Bets.Count);
        Assert.True(ticket.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(0.05)] // Less than minimum 0.1
    public void Create_WithInvalidStake_ShouldThrowException(decimal invalidStake)
    {
        // Arrange
        var bets = new List<Bet> { CreateSampleBet(1, BetType.Winner, 2.0m) };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ticket.Create(invalidStake, bets));
    }

    [Fact]
    public void Approve_WhenPending_ShouldChangeStatusToSuccess()
    {
        // Arrange
        var bets = new List<Bet> { CreateSampleBet(1, BetType.Winner, 2.0m) };
        var ticket = Ticket.Create(25m, bets);

        // Act
        ticket.Approve();

        // Assert
        Assert.Equal(TicketStatus.Success, ticket.Status);
    }

    [Fact]
    public void Approve_WhenNotPending_ShouldThrowException()
    {
        // Arrange
        var bets = new List<Bet> { CreateSampleBet(1, BetType.Winner, 2.0m) };
        var ticket = Ticket.Create(25m, bets);
        ticket.Approve(); // Already approved

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ticket.Approve());
    }

    [Fact]
    public void Reject_WhenPending_ShouldChangeStatusToRejected()
    {
        // Arrange
        var bets = new List<Bet> { CreateSampleBet(1, BetType.Winner, 2.0m) };
        var ticket = Ticket.Create(25m, bets);

        // Act
        ticket.Reject();

        // Assert
        Assert.Equal(TicketStatus.Rejected, ticket.Status);
    }

    private static Bet CreateSampleBet(int selection, BetType betType, decimal odds)
    {
        return new Bet(1L, 1L, Guid.NewGuid(), selection, betType, odds);
    }
}
