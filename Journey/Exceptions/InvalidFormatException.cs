using System.ComponentModel.DataAnnotations;

internal class InvalidFormatException(string line) : ValidationException($"The migration file is malformed at: {line}")
{
}