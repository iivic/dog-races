using DogRaces.Domain.Entities;
using Xunit;

namespace DogRaces.Domain.UnitTests.Entities;

public class RaceTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRace()
    {
        // Arrange
        var id = 1L;
        var startTime = DateTimeOffset.UtcNow.AddMinutes(30);
        var durationInSeconds = 120;

        // Act
        var race = new Race(id, startTime, durationInSeconds);

        // Assert
        Assert.Equal(id, race.Id);
        Assert.Equal(startTime, race.StartTime);
        Assert.Equal(startTime.AddSeconds(durationInSeconds), race.EndTime);
        Assert.True(race.IsActive);
        Assert.Null(race.Result);
        Assert.True(race.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(120)]
    public void Constructor_WithValidDuration_ShouldCreateRace(int validDuration)
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var race = new Race(1L, startTime, validDuration);

        // Assert
        Assert.Equal(startTime.AddSeconds(validDuration), race.EndTime);
        Assert.True(race.IsActive);
    }

    [Fact]
    public void CloseRaceForBetting_WhenActive_ShouldCloseRace()
    {
        // Arrange
        var race = new Race(1L, DateTimeOffset.UtcNow.AddMinutes(30), 120);
        var raceResult = new[] { 1, 2, 3 };

        // Act
        race.CloseRaceForBetting(raceResult);

        // Assert
        Assert.False(race.IsActive);
        Assert.Equal(raceResult, race.Result);
    }

    [Fact]
    public void CloseRaceForBetting_WhenAlreadyClosed_ShouldUpdateResult()
    {
        // Arrange
        var race = new Race(1L, DateTimeOffset.UtcNow.AddMinutes(30), 120);
        race.CloseRaceForBetting(new[] { 1, 2, 3 }); // Already closed
        
        var newResult = new[] { 3, 2, 1 };

        // Act
        race.CloseRaceForBetting(newResult);

        // Assert - The race allows updating the result
        Assert.Equal(newResult, race.Result);
        Assert.False(race.IsActive);
    }
}
