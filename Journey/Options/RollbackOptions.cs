using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
[Verb("rollback", HelpText = "Rollback the database.")]
internal class RollbackOptions : Options
{
}