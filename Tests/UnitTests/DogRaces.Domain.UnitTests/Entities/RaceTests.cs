using DogRaces.Domain.Entities;
using Xunit;

namespace DogRaces.Domain.UnitTests.Entities;

public class RaceTests
{
    [Fact]
    public void Race_WhenCreated_ShouldHaveCorrectProperties()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddMinutes(30);
        var durationInSeconds = 120;

        // Act
        var race = new Race(1L, startTime, durationInSeconds);

        // Assert
        Assert.Equal(1L, race.Id);
        Assert.Equal(startTime, race.StartTime);
        Assert.Equal(startTime.AddSeconds(durationInSeconds), race.EndTime);
        Assert.True(race.IsActive);
        Assert.NotNull(race.RaceName);
        Assert.NotEmpty(race.RaceName);
        Assert.Equal(100, race.RandomNumbers.Count);
        Assert.All(race.RandomNumbers, n => Assert.InRange(n, 1, 6));
    }

    [Fact]
    public void Race_WhenCreated_ShouldHaveValidDuration()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddMinutes(30);
        var duration = 90;

        // Act
        var race = new Race(1L, startTime, duration);

        // Assert
        Assert.Equal(duration, (race.EndTime - race.StartTime).TotalSeconds);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Race_WithInvalidDuration_ShouldThrow(int invalidDuration)
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Race(1L, startTime, invalidDuration));
    }

    [Fact]
    public void CloseBetting_WhenActive_ShouldCloseRaceAndGenerateResult()
    {
        // Arrange
        var race = new Race(1L, DateTimeOffset.UtcNow.AddMinutes(30), 120);

        // Act
        race.CloseBetting();

        // Assert
        Assert.False(race.IsActive);
        Assert.NotNull(race.Result);
        Assert.Equal(3, race.Result.Length);
        Assert.All(race.Result, n => Assert.InRange(n, 1, 6));
    }

    [Fact]
    public void CloseBetting_WhenAlreadyClosed_ShouldThrowException()
    {
        // Arrange
        var race = new Race(1L, DateTimeOffset.UtcNow.AddMinutes(30), 120);
        race.CloseBetting(); // First close

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => race.CloseBetting());
    }

    [Fact]
    public void GenerateResults_ShouldProduceRandomResults()
    {
        // Arrange
        var race = new Race(1L, DateTimeOffset.UtcNow.AddMinutes(30), 120);

        // Act
        race.GenerateResults();

        // Assert
        Assert.NotNull(race.Result);
        Assert.Equal(3, race.Result.Length);
        Assert.All(race.Result, n => Assert.InRange(n, 1, 6));
    }

    [Fact]
    public void CalculateOddsFromSequence_ShouldReturnValidOdds()
    {
        // Arrange
        var race = new Race(1L, DateTimeOffset.UtcNow.AddMinutes(30), 120);

        // Act
        var odds = race.CalculateOddsFromSequence();

        // Assert
        Assert.Equal(6, odds.Count); // Should have odds for dogs 1-6
        Assert.All(odds.Values, odd => Assert.InRange(odd, 1.10m, 80.0m)); // Within reasonable range for random sequences
    }
}