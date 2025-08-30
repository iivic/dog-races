using DogRaces.Domain.Enums;
using DogRaces.Domain.Services;

namespace DogRaces.Domain.Entities;

/// <summary>
/// Represents a dog race with 6 selections and embedded random sequence
/// </summary>
public class Race
{
    // Private parameterless constructor for EF Core
    private Race() { }

    public Race(long id, DateTimeOffset startTime, int raceDurationInSeconds)
    {
        if (raceDurationInSeconds <= 0)
            throw new ArgumentException("Race duration must be positive", nameof(raceDurationInSeconds));

        Id = id;
        StartTime = startTime;
        SetEndTime(raceDurationInSeconds);
        IsActive = true;
        Status = RaceStatus.Scheduled;
        CreatedAt = DateTimeOffset.UtcNow;
        
        // Generate the race's unique random sequence and name
        GenerateRandomSequence();
        GenerateRaceName();
    }

    public long Id { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public bool IsActive { get; private set; }
    public RaceStatus Status { get; private set; }
    public string RaceName { get; private set; } = string.Empty;
    public List<int> RandomNumbers { get; private set; } = []; // 100 numbers (1-6)
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ResultDeterminedAt { get; private set; }
    public DateTimeOffset? ResultPublishedAt { get; private set; }
    
    /// <summary>
    /// Top 3 positions - derived from first 3 RandomNumbers when race finishes
    /// </summary>
    public int[]? Result { get; private set; }

    // Navigation properties
    public virtual ICollection<Bet> Bets { get; private set; } = [];
    public virtual ICollection<RaceOdds> RaceOdds { get; private set; } = [];

    public bool HasResult() => Result != null;
    
    public void SetEndTime(int raceDurationInSeconds)
    {
        EndTime = StartTime.AddSeconds(raceDurationInSeconds);
    }

    /// <summary>
    /// Check if the race has started
    /// </summary>
    public bool HasStarted => DateTimeOffset.UtcNow >= StartTime;

    /// <summary>
    /// Check if the race has ended
    /// </summary>
    public bool HasEnded => DateTimeOffset.UtcNow >= EndTime;

    private void GenerateRandomSequence()
    {
        var random = new Random();
        
        while (RandomNumbers.Count < 100)
        {
            var number = random.Next(1, 7); // Dogs 1-6
            
            // Validate: No 3 consecutive same numbers
            if (IsValidSequenceNumber(number))
            {
                RandomNumbers.Add(number);
            }
        }
    }
    
    private bool IsValidSequenceNumber(int number)
    {
        if (RandomNumbers.Count < 2)
            return true;
            
        // Check last 2 numbers - don't allow 3 consecutive identical
        var last1 = RandomNumbers[RandomNumbers.Count - 1];
        var last2 = RandomNumbers[RandomNumbers.Count - 2];
        
        return !(number == last1 && number == last2);
    }
    
    private void GenerateRaceName()
    {
        var generator = new RaceNameGenerator();
        RaceName = generator.GenerateRaceName();
    }
    
    /// <summary>
    /// Calculate odds for each dog based on frequency in random sequence
    /// </summary>
    public Dictionary<int, decimal> CalculateOddsFromSequence()
    {
        var frequency = RandomNumbers
            .GroupBy(n => n)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var odds = new Dictionary<int, decimal>();
        foreach (var dog in Enumerable.Range(1, 6))
        {
            var count = frequency.GetValueOrDefault(dog, 0);
            var probability = count / 100.0m;
            var rawOdds = probability > 0 ? 1 / probability : 90m;
            
            odds[dog] = rawOdds;
        }
        
        return odds;
    }

    /// <summary>
    /// Create and add RaceOdds entities for all 6 selections based on calculated odds
    /// </summary>
    public void CreateRaceOdds()
    {
        var calculatedOdds = CalculateOddsFromSequence();
        
        foreach (var (selection, odds) in calculatedOdds)
        {
            var raceOdds = new RaceOdds(
                id: 0,
                raceId: Id,
                selection: selection,
                odds: odds
            );
            
            RaceOdds.Add(raceOdds);
        }
    }
    
    /// <summary>
    /// Generate race results by randomly selecting 3 numbers from the sequence
    /// </summary>
    public void GenerateResults()
    {
        var random = new Random();
        var selectedNumbers = RandomNumbers
            .OrderBy(x => random.Next())
            .Take(3)
            .ToArray();
        SetResult(selectedNumbers);
    }
    
    /// <summary>
    /// Close betting (5 seconds before race starts)
    /// </summary>
    public void CloseBetting()
    {
        if (Status != RaceStatus.Scheduled)
            throw new InvalidOperationException("Can only close betting for scheduled races");
            
        IsActive = false;
        Status = RaceStatus.BettingClosed;
        GenerateResults();
    }
    
    /// <summary>
    /// Start the race
    /// </summary>
    public void StartRace()
    {
        if (Status != RaceStatus.BettingClosed)
            throw new InvalidOperationException("Can only start races with closed betting");
            
        Status = RaceStatus.Running;
    }
    
    /// <summary>
    /// Finish the race with results
    /// </summary>
    public void FinishRace()
    {
        if (Status != RaceStatus.Running)
            throw new InvalidOperationException("Can only finish running races");
            
        Status = RaceStatus.Finished;
        ResultPublishedAt = DateTimeOffset.UtcNow;
        ProcessBetsResult();
    }

    private void ProcessBetsResult()
    {
        foreach (var bet in Bets)
        {
            bet.ProcessResult(Result!);
        }
    }

    private void SetResult(int[] result)
    {
        Result = result;
        ResultDeterminedAt = DateTimeOffset.UtcNow;
    }
}