using DogRaces.Application.Features.Races.Commands.EnsureMinimumRaces;
using DogRaces.Application.Features.Races.Commands.ProcessBettingClosures;
using DogRaces.Application.Features.Races.Commands.ProcessRaceStarts;
using DogRaces.Application.Features.Races.Commands.ProcessRaceFinishes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DogRaces.BackgroundServices.Workers;

/// <summary>
/// Background worker that manages the complete race lifecycle using focused CQRS commands:
/// - Processes betting closures for races ready to start
/// - Starts races when their time comes
/// - Finishes races that have completed
/// - Maintains minimum number of concurrent races
/// </summary>
public class RaceSchedulingWorker : BaseWorker
{
    private const int CheckIntervalSeconds = 2; // Check every 2 seconds for responsive timing
    
    public RaceSchedulingWorker(
        ILogger<BaseWorker> logger,
        IServiceProvider serviceProvider) 
        : base(logger, serviceProvider, CheckIntervalSeconds)
    {
    }

    protected override async Task Execute(IServiceScope scope, CancellationToken cancellationToken)
    {
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var bettingResult = await sender.Send(new ProcessBettingClosuresCommand(), cancellationToken);
        var startsResult = await sender.Send(new ProcessRaceStartsCommand(), cancellationToken);
        var finishesResult = await sender.Send(new ProcessRaceFinishesCommand(), cancellationToken);
        var poolResult = await sender.Send(new EnsureMinimumRacesCommand(), cancellationToken);

        LogTransitionResults(bettingResult, startsResult, finishesResult);
        LogPoolResults(poolResult);
    }

    private void LogTransitionResults(
        ProcessBettingClosuresResponse bettingResult,
        ProcessRaceStartsResponse startsResult, 
        ProcessRaceFinishesResponse finishesResult)
    {
        var anyChanges = bettingResult.BettingClosed > 0 || startsResult.RacesStarted > 0 || finishesResult.RacesFinished > 0;
        
        if (anyChanges)
        {
            var totalProcessed = bettingResult.RacesProcessed + startsResult.RacesProcessed + finishesResult.RacesProcessed;
            Logger.LogInformation(
                "🔄 Processed {Total} races: {Closed} betting closed, {Started} started, {Finished} finished", 
                totalProcessed,
                bettingResult.BettingClosed,
                startsResult.RacesStarted,
                finishesResult.RacesFinished);
        }
    }

    private void LogPoolResults(EnsureMinimumRacesResponse result)
    {
        var anyChanges = result.RacesCreated > 0;

        if (anyChanges)
        {
            Logger.LogInformation(
                "➕ Created {Count} new races. Active races: {Before} → {After}", 
                result.RacesCreated,
                result.ActiveRacesBefore,
                result.ActiveRacesAfter);
        }
    }
}
