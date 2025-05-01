using System.Diagnostics.CodeAnalysis;
using CommandLine;

[Verb("migrate", HelpText = "Migrate the database.")]
internal class MigrateOptions : Options
{
}