using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using DogRaces.Domain.ValueObjects;

namespace DogRaces.Domain.Services;

/// <summary>
/// Factory for creating race odds based on simulated results from RandomNumbers
/// </summary>
public class RaceOddsFactory
{
    private readonly List<RaceResult> _simulatedResults;

    public RaceOddsFactory(List<int> randomNumbers)
    {
        _simulatedResults = ConvertToRaceResults(randomNumbers);
    }

    /// <summary>
    /// Create all race odds for Winner, Top2, and Top3 bet types
    /// </summary>
    public List<RaceOdds> CreateAllRaceOdds(long raceId)
    {
        return CreateWinnerOdds(raceId)
            .Concat(CreateTop2Odds(raceId))
            .Concat(CreateTop3Odds(raceId))
            .ToList();
    }

    /// <summary>
    /// Create Winner bet odds (must finish 1st)
    /// </summary>
    public IEnumerable<RaceOdds> CreateWinnerOdds(long raceId)
    {
        for (var runnerNumber = 1; runnerNumber <= 6; runnerNumber++)
        {
            var successCount = _simulatedResults.Count(r => r.First == runnerNumber);
            var odds = OddsCalculator.GetOdds(successCount, _simulatedResults.Count);
            
            yield return new RaceOdds(
                id: 0,
                raceId: raceId,
                selection: runnerNumber,
                odds: odds,
                betType: BetType.Winner
            );
        }
    }

    /// <summary>
    /// Create Top2 bet odds (must finish 1st or 2nd)
    /// </summary>
    public IEnumerable<RaceOdds> CreateTop2Odds(long raceId)
    {
        for (var runnerNumber = 1; runnerNumber <= 6; runnerNumber++)
        {
            var successCount = _simulatedResults.Count(r => r.IsInTop2(runnerNumber));
            var odds = OddsCalculator.GetOdds(successCount, _simulatedResults.Count);
            
            yield return new RaceOdds(
                id: 0,
                raceId: raceId,
                selection: runnerNumber,
                odds: odds,
                betType: BetType.Top2
            );
        }
    }

    /// <summary>
    /// Create Top3 bet odds (must finish 1st, 2nd, or 3rd)
    /// </summary>
    public IEnumerable<RaceOdds> CreateTop3Odds(long raceId)
    {
        for (var runnerNumber = 1; runnerNumber <= 6; runnerNumber++)
        {
            var successCount = _simulatedResults.Count(r => r.IsInTop3(runnerNumber));
            var odds = OddsCalculator.GetOdds(successCount, _simulatedResults.Count);
            
            yield return new RaceOdds(
                id: 0,
                raceId: raceId,
                selection: runnerNumber,
                odds: odds,
                betType: BetType.Top3
            );
        }
    }

    /// <summary>
    /// Convert list of random numbers to simulated race results
    /// Groups of 3 numbers represent top 3 positions
    /// </summary>
    private List<RaceResult> ConvertToRaceResults(List<int> randomNumbers)
    {
        var results = new List<RaceResult>();
        
        // Process numbers in groups of 3 (representing 1st, 2nd, 3rd places)
        for (var i = 0; i <= randomNumbers.Count - 3; i += 3)
        {
            var first = randomNumbers[i];
            var second = randomNumbers[i + 1]; 
            var third = randomNumbers[i + 2];
            
            results.Add(new RaceResult(first, second, third));
        }
        
        return results;
    }
}
