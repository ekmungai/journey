using System.Diagnostics.CodeAnalysis;
using CommandLine;

[Verb("scaffold", HelpText = "Scaffold the next version migration file.")]
public class ScaffoldOptions : Options { }