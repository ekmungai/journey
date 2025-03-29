using System.ComponentModel.DataAnnotations;

internal class MissingSectionException(string section) : ValidationException($"The migration file is missing a {section} section")
{
}