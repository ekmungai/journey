using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The specified database does not exist.
/// </summary>
/// <param name="name">The name of the database</param>
internal class UninitializedDatabaseException(string name) : ValidationException($"The database {name} does not exist");