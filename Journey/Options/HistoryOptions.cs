using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
[Verb("history", HelpText = "Get the migrations that have been applied to the database.")]
internal class HistoryOptions : Options
{
}