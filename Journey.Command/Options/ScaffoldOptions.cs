using System.Diagnostics.CodeAnalysis;
using CommandLine;

[Verb("scaffold", HelpText = "Scaffold the next version migration file.")]
internal class ScaffoldOptions : Options { }