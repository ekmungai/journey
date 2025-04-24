using System.ComponentModel.DataAnnotations;
/// <summary>
/// The migration file is invalid because of an unclosed transaction.
/// </summary>
internal class OpenTransactionException() : ValidationException { }