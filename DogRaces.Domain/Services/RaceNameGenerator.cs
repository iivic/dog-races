namespace DogRaces.Domain.Services;

/// <summary>
/// Generates fun, engaging names for races
/// </summary>
public class RaceNameGenerator
{
    private readonly string[] _tracks = {
        "Thunder Valley", "Sunset Meadows", "Golden Gate", "Silver Stream",
        "Lightning Ridge", "Moonlight Bay", "Crystal Falls", "Storm Peak",
        "Eagle Heights", "Diamond Downs", "Phoenix Park", "Emerald Fields"
    };
    
    private readonly string[] _raceTypes = {
        "Championship Stakes", "Derby Classic", "Sprint Cup", "Grand Prix",
        "Elite Trophy", "Masters Cup", "Victory Stakes", "Premier Classic",
        "Royal Challenge", "Champions League", "Speed Trial", "Glory Run"
    };
    
    private readonly string[] _adjectives = {
        "Lightning", "Golden", "Midnight", "Diamond", "Thunder", "Silver",
        "Crimson", "Sapphire", "Emerald", "Phoenix", "Storm", "Royal",
        "Blazing", "Mystic", "Stellar", "Titan", "Arctic", "Neon"
    };

    private static readonly Random _random = new();

    /// <summary>
    /// Generates a fun race name combining adjective, race type, and track
    /// Examples: "Lightning Championship Stakes at Thunder Valley"
    /// </summary>
    public string GenerateRaceName()
    {
        var adjective = _adjectives[_random.Next(_adjectives.Length)];
        var raceType = _raceTypes[_random.Next(_raceTypes.Length)];
        var track = _tracks[_random.Next(_tracks.Length)];

        return $"{adjective} {raceType} at {track}";
    }
}
