public record Itinerary(
    string Version,
    string Description,
    DateTimeOffset RunTime,
    string RunBy,
    string Author
) {
    public override string ToString() {
        return $"{Version} \t| {RunTime} \t| {Description} \t| {RunBy} \t| {Author}";
    }
};