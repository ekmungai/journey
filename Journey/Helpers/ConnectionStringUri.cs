using System.Data.Common;
using Journey.Exceptions;

namespace Journey.Helpers;

/// <summary>
/// Helpers for translating uri style connection strings into ADO.NET key/value connection strings.
/// </summary>
internal static class ConnectionStringUri {

    /// <summary>
    /// Applies the query string parameters of the uri to the connection string builder, so that
    /// options such as timeouts or ssl settings survive the conversion to key/value format.
    /// </summary>
    /// <param name="uri">The uri style connection string.</param>
    /// <param name="builder">The builder the parameters are applied to.</param>
    /// <param name="aliases">
    /// Maps parameter names to the database's connection string keyword, for parameters whose uri
    /// spelling differs from it.
    /// </param>
    internal static void ApplyQueryParameters(Uri uri, DbConnectionStringBuilder builder,
        IReadOnlyDictionary<string, string>? aliases = null) {
        var query = uri.Query.TrimStart('?');
        if (query.Length == 0) return;

        foreach (var parameter in query.Split('&', StringSplitOptions.RemoveEmptyEntries)) {
            var separator = parameter.IndexOf('=');
            if (separator < 1) throw new InvalidConnectionStringParameterException(parameter);

            var key = Uri.UnescapeDataString(parameter[..separator]);
            var value = Uri.UnescapeDataString(parameter[(separator + 1)..]);
            if (aliases is not null && aliases.TryGetValue(key, out var alias)) key = alias;

            try {
                builder[key] = value;
            } catch (Exception exception) when (exception is ArgumentException or FormatException) {
                throw new InvalidConnectionStringParameterException(key, exception);
            }
        }
    }
}
