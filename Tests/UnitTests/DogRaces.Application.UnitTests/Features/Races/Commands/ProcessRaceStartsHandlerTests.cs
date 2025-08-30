using DogRaces.Application.Data;
using DogRaces.Application.Features.Races.Commands.ProcessRaceStarts;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DogRaces.Application.UnitTests.Features.Races.Commands;

public class ProcessRaceStartsHandlerTests
{
    private readonly Mock<IDogRacesContext> _contextMock;
    private readonly Mock<ILogger<ProcessRaceStartsHandler>> _loggerMock;
    private readonly Mock<DbSet<Race>> _racesDbSetMock;
    private readonly ProcessRaceStartsHandler _handler;

    public ProcessRaceStartsHandlerTests()
    {
        _contextMock = new Mock<IDogRacesContext>();
        _loggerMock = new Mock<ILogger<ProcessRaceStartsHandler>>();
        _racesDbSetMock = new Mock<DbSet<Race>>();
        
        _contextMock.Setup(x => x.Races).Returns(_racesDbSetMock.Object);
        
        _handler = new ProcessRaceStartsHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldStartRaces_ThatHaveReachedStartTime()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var raceReadyToStart = CreateBettingClosedRace(now.AddSeconds(-1)); // 1 second ago
        var raceFutureStart = CreateBettingClosedRace(now.AddMinutes(5));   // 5 minutes future

        var races = new List<Race> { raceReadyToStart, raceFutureStart }.AsQueryable();
        SetupDbSetMock(races);

        // Act
        var result = await _handler.Handle(new ProcessRaceStartsCommand(), CancellationToken.None);

        // Assert
        Assert.Equal(1, result.RacesStarted);
        Assert.Equal(RaceStatus.Running, raceReadyToStart.Status);
        Assert.Equal(RaceStatus.BettingClosed, raceFutureStart.Status);
    }

    [Fact]
    public async Task Handle_ShouldNotStartRaces_ThatAreNotBettingClosed()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var scheduledRace = CreateScheduledRace(now.AddSeconds(-1));

        var races = new List<Race> { scheduledRace }.AsQueryable();
        SetupDbSetMock(races);

        // Act
        var result = await _handler.Handle(new ProcessRaceStartsCommand(), CancellationToken.None);

        // Assert
        Assert.Equal(0, result.RacesStarted);
        Assert.Equal(RaceStatus.Scheduled, scheduledRace.Status);
    }

    private Race CreateBettingClosedRace(DateTimeOffset startTime)
    {
        var race = new Race(0, startTime, 30);
        race.CloseBetting();
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
