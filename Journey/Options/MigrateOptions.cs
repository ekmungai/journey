using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
[Verb("migrate", HelpText = "Migrate the database.")]
internal class MigrateOptions : Options
{
}