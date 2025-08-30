using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Xunit;

namespace DogRaces.Domain.UnitTests.Entities;

public class BetTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateBet()
    {
        // Arrange
        var id = 1L;
        var raceId = 2L;
        var ticketId = Guid.NewGuid();
        var selection = 3;
        var betType = BetType.Winner;
        var odds = 2.5m;

        // Act
        var bet = new Bet(id, raceId, ticketId, selection, betType, odds);

        // Assert
        Assert.Equal(id, bet.Id);
        Assert.Equal(raceId, bet.RaceId);
        Assert.Equal(ticketId, bet.TicketId);
        Assert.Equal(selection, bet.Selection);
        Assert.Equal(betType, bet.BetType);
        Assert.Equal(odds, bet.Odds);
        Assert.Null(bet.IsWinning);
        Assert.True(bet.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)] // Greater than max selection (6)
    public void Constructor_WithInvalidSelection_ShouldThrowException(int invalidSelection)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Bet(1L, 1L, Guid.NewGuid(), invalidSelection, BetType.Winner, 2.0m));
    }

    [Theory]
    [InlineData(0.9)] // Less than minimum odds
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidOdds_ShouldThrowException(decimal invalidOdds)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Bet(1L, 1L, Guid.NewGuid(), 1, BetType.Winner, invalidOdds));
    }

    [Fact]
    public void ProcessResult_WinnerBetWithWinningSelection_ShouldSetIsWinning()
    {
        // Arrange
        var bet = new Bet(1L, 1L, Guid.NewGuid(), 2, BetType.Winner, 3.0m);
        var raceResult = new[] { 2, 1, 3 }; // Selection 2 wins

        // Act
        bet.ProcessResult(raceResult);

        // Assert
        Assert.True(bet.IsWinning);
    }

    [Fact]
    public void ProcessResult_WinnerBetWithLosingSelection_ShouldSetIsWinning()
    {
        // Arrange
        var bet = new Bet(1L, 1L, Guid.NewGuid(), 5, BetType.Winner, 3.0m);
        var raceResult = new[] { 2, 1, 3 }; // Selection 5 doesn't win

        // Act
        bet.ProcessResult(raceResult);

        // Assert
        Assert.False(bet.IsWinning);
    }

    [Fact]
    public void ProcessResult_Top2BetWithTop2Selection_ShouldSetIsWinning()
    {
        // Arrange
        var bet = new Bet(1L, 1L, Guid.NewGuid(), 1, BetType.Top2, 2.0m);
        var raceResult = new[] { 2, 1, 3 }; // Selection 1 is in top 2 (2nd place)

        // Act
        bet.ProcessResult(raceResult);

        // Assert
        Assert.True(bet.IsWinning);
    }

    [Fact]
    public void ProcessResult_Top3BetWithTop3Selection_ShouldSetIsWinning()
    {
        // Arrange
        var bet = new Bet(1L, 1L, Guid.NewGuid(), 3, BetType.Top3, 1.5m);
        var raceResult = new[] { 2, 1, 3 }; // Selection 3 is in top 3 (3rd place)

        // Act
        bet.ProcessResult(raceResult);

        // Assert
        Assert.True(bet.IsWinning);
    }
}
