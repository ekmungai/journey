using System.Diagnostics.CodeAnalysis;
using CommandLine;

[ExcludeFromCodeCoverage]
[Verb("validate", HelpText = "Validate the taget version migration file.")]
internal class ValidateOptions : Options
{
}