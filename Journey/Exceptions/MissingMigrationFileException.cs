
namespace Journey.Exceptions;

/// <summary>
/// The migration file for the provided version is invalid.
/// </summary>
/// <param name="version">The version number</param>
internal class MissingMigrationFileException(int version) : FileNotFoundException($"Migration file for version {version} was not found") {
}