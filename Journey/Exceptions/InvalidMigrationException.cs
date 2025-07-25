
namespace Journey.Exceptions;

/// <summary>
/// The attempted migration is inconsistent.
/// </summary>
/// <param name="message">A message explaining the cause of the inconsistency.</param>
internal class InvalidMigrationException(string message) : InvalidOperationException(message) { }