using System.ComponentModel.DataAnnotations;

namespace Journey.Exceptions;

/// <summary>
/// Thrown when a query string parameter of a uri style connection string cannot be applied to the
/// database's connection string.
/// </summary>
internal class InvalidConnectionStringParameterException(string parameter, Exception? inner = null)
    : ValidationException($"The connection string parameter {parameter} is not supported by this database", inner);
