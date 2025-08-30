using DogRaces.Application.Data;
using DogRaces.Application.Features.Races.Commands.ProcessRaceFinishes;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DogRaces.Application.UnitTests.Features.Races.Commands;

public class ProcessRaceFinishesHandlerTests
{
    private readonly Mock<IDogRacesContext> _contextMock;
    private readonly Mock<ILogger<ProcessRaceFinishesHandler>> _loggerMock;
    private readonly Mock<DbSet<Race>> _racesDbSetMock;
    private readonly ProcessRaceFinishesHandler _handler;

    public ProcessRaceFinishesHandlerTests()
    {
        _contextMock = new Mock<IDogRacesContext>();
        _loggerMock = new Mock<ILogger<ProcessRaceFinishesHandler>>();
        _racesDbSetMock = new Mock<DbSet<Race>>();
        
        _contextMock.Setup(x => x.Races).Returns(_racesDbSetMock.Object);
        
        _handler = new ProcessRaceFinishesHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFinishRaces_ThatHaveReachedEndTime()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var raceToFinish = CreateRunningRace(now.AddSeconds(-35), 30); // Started 35 seconds ago, 30s duration
        var raceStillRunning = CreateRunningRace(now.AddSeconds(-10), 30); // Started 10 seconds ago, 30s duration

        var races = new List<Race> { raceToFinish, raceStillRunning }.AsQueryable();
        SetupDbSetMock(races);

        // Act
        var result = await _handler.Handle(new ProcessRaceFinishesCommand(), CancellationToken.None);

        // Assert
        Assert.Equal(1, result.RacesFinished);
        Assert.Equal(RaceStatus.Finished, raceToFinish.Status);
        Assert.Equal(RaceStatus.Running, raceStillRunning.Status);
        Assert.NotNull(raceToFinish.Result);
        Assert.Null(raceStillRunning.Result);
    }

    [Fact]
    public async Task Handle_ShouldNotFinishRaces_ThatAreNotRunning()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var scheduledRace = CreateScheduledRace(now.AddSeconds(-35));

        var races = new List<Race> { scheduledRace }.AsQueryable();
        SetupDbSetMock(races);

        // Act
        var result = await _handler.Handle(new ProcessRaceFinishesCommand(), CancellationToken.None);

        // Assert
        Assert.Equal(0, result.RacesFinished);
        Assert.Equal(RaceStatus.Scheduled, scheduledRace.Status);
    }

    private Race CreateRunningRace(DateTimeOffset startTime, int durationSeconds)
    {
        var race = new Race(0, startTime, durationSeconds);
        race.CloseBetting();
        race.StartRace();
        return race;
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
