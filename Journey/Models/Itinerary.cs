namespace Journey.Models;

/// <summary>
/// Represents an entry in the versions table, i.e. a stop on the migration journey.
/// </summary>
/// <param name="Version">The version number.</param>
/// <param name="Description">A short narration of the migration.</param>
/// <param name="RunTime">When the migration was executed.</param>
/// <param name="RunBy">The party (user/system/email) who triggered the migration.</param>
/// <param name="Author">The party (user/email) who prepared the migration.</param>
public record Itinerary(
    string Version,
    string Description,
    DateTimeOffset RunTime,
    string RunBy,
    string Author
) {
    /// <summary>
    /// The string representation of the Itenerary.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        return $"{Version} \t| {RunTime} \t| {Description} \t| {RunBy} \t| {Author}";
    }
};