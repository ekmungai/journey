using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
[Verb("update", HelpText = "Bring the database up to the latest version.")]
internal class UpdateOptions : Options
{
}