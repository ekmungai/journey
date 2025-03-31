using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
[Verb("scaffold", HelpText = "Scaffold the next version migration file.")]
internal class ScaffoldOptions : Options
{
}