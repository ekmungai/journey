using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The migration file is invalid because of an unclosed section.
/// <param name="version">The version of the migration file.</param>
/// </summary>
internal class OpenSectionException(int version, string section) : ValidationException($"The migration file for version {version} has an unclosed {section} section");