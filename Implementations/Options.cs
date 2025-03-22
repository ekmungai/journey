using CommandLine;

internal class Options
{
    [Option('d', "dir", Required = true, HelpText = "The path to the versions directory.")]
    public required string VersionsDir { get; init; }
}