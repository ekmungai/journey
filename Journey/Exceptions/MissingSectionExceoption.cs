using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The migration file is missing a required section.
/// </summary>
/// <param name="section">The missing section.</param>
internal class MissingSectionException(string section) : ValidationException($"The migration file is missing a {section} section") {
}