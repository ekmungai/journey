using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The migration file is invalid because of an unclosed transaction.
/// </summary>
internal class OpenTransactionException() : ValidationException;