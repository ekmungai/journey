namespace Journey.Exceptions;

/// <summary>
/// The attempted rollback is inconsistent.
/// </summary>
/// <param name="message">A message explaining the cause of the inconsistency.</param>
internal class InvalidRollbackException(string message) : InvalidOperationException(message);