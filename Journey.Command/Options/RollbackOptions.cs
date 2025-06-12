using System.Diagnostics.CodeAnalysis;
using CommandLine;

[Verb("rollback", HelpText = "Rollback the database.")]
internal class RollbackOptions : Options { }