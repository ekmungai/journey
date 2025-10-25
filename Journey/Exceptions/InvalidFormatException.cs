using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// Migration file does not conform to the expected structure
/// <param name="version">The version of the migration file.</param>
/// <param name="line">The line in file that is invalid</param>
/// </summary>
internal class InvalidFormatException(int version, string line) : ValidationException($"The migration file for version {version} is malformed at: {line}");