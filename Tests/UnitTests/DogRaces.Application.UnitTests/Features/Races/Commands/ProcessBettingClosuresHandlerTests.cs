using DogRaces.Application.Data;
using DogRaces.Application.Features.Races.Commands.ProcessBettingClosures;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DogRaces.Application.UnitTests.Features.Races.Commands;

public class ProcessBettingClosuresHandlerTests
{
    private readonly Mock<IDogRacesContext> _contextMock;
    private readonly Mock<ILogger<ProcessBettingClosuresHandler>> _loggerMock;
    private readonly Mock<DbSet<Race>> _racesDbSetMock;
    private readonly ProcessBettingClosuresHandler _handler;

    public ProcessBettingClosuresHandlerTests()
    {
        _contextMock = new Mock<IDogRacesContext>();
        _loggerMock = new Mock<ILogger<ProcessBettingClosuresHandler>>();
        _racesDbSetMock = new Mock<DbSet<Race>>();

        _contextMock.Setup(x => x.Races).Returns(_racesDbSetMock.Object);

        _handler = new ProcessBettingClosuresHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCloseBettingFor_RacesStartingInLessThanFiveSeconds()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var raceStartingInThreeSeconds = CreateScheduledRace(now.AddSeconds(3));
        var raceFarInFuture = CreateScheduledRace(now.AddMinutes(10));
        var raceInPast = CreateScheduledRace(now.AddSeconds(-10));

        var races = new List<Race> { raceStartingInThreeSeconds, raceFarInFuture, raceInPast }.AsQueryable();

        SetupDbSetMock(races);

        // Act
        var result = await _handler.Handle(new ProcessBettingClosuresCommand(), CancellationToken.None);

        // Assert
        Assert.Equal(1, result.BettingClosed);
        Assert.Equal(RaceStatus.BettingClosed, raceStartingInThreeSeconds.Status);
        Assert.Equal(RaceStatus.Scheduled, raceFarInFuture.Status);
        Assert.Equal(RaceStatus.Scheduled, raceInPast.Status);
    }

    [Fact]
    public async Task Handle_ShouldNotCloseBetting_ForRacesAlreadyBettingClosed()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var race = CreateScheduledRace(now.AddSeconds(3));
        race.CloseBetting(); // Already closed

        var races = new List<Race> { race }.AsQueryable();
        SetupDbSetMock(races);

        // Act
        var result = await _handler.Handle(new ProcessBettingClosuresCommand(), CancellationToken.None);

        // Assert
        Assert.Equal(0, result.BettingClosed);
    }

    private Race CreateScheduledRace(DateTimeOffset startTime)
    {
        return new Race(0, startTime, 30);
    }

    private void SetupDbSetMock(IQueryable<Race> races)
    {
        _racesDbSetMock.As<IQueryable<Race>>().Setup(m => m.Provider).Returns(races.Provider);
        _racesDbSetMock.As<IQueryable<Race>>().Setup(m => m.Expression).Returns(races.Expression);
        _racesDbSetMock.As<IQueryable<Race>>().Setup(m => m.ElementType).Returns(races.ElementType);
        _racesDbSetMock.As<IQueryable<Race>>().Setup(m => m.GetEnumerator()).Returns(races.GetEnumerator());
    }
}