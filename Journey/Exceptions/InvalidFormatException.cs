using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// Migration file does not conform to the expected structure
/// </summary>
/// <param name="line">The line in file that is invalid</param>
internal class InvalidFormatException(string line) : ValidationException($"The migration file is malformed at: {line}") { }