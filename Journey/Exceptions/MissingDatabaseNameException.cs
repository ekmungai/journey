using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// The connection string does not contain a database name.
/// </summary>
public class MissingDatabaseNameException() : ValidationException($"The connection string does not contain a database name");