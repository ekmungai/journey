using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The migration file is missing a required section.
/// </summary>
/// <param name="version">The version of the migration file.</param>
/// <param name="section">The missing section.</param>
internal class MissingSectionException(int version, string section) : ValidationException($"The migration file for version {version} is missing a {section} section");