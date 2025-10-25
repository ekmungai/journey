using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The migration file is invalid because of an unclosed transaction.
/// <param name="version">The version of the migration file.</param>
/// </summary>
internal class OpenTransactionException(int version) : ValidationException($"The migration file for version {version} has an unclosed transaction");