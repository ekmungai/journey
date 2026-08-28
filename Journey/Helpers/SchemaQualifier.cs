using System.Text;
using System.Text.RegularExpressions;

namespace Journey.Helpers;

/// <summary>
/// Points the bookkeeping table of a query at the schema (or keyspace) the migration is being
/// applied to.
/// </summary>
internal static partial class SchemaQualifier {
    private const string Table = "versions";

    /// <summary>
    /// Qualifies references to the versions table in the query with the given schema.
    ///
    /// Only whole identifiers are qualified, so a table of the user's own whose name merely
    /// contains the word, such as chart_definition_versions or versions_backup, is left alone, as
    /// is a reference that is already qualified, such as reporting.versions. References inside
    /// comments and string literals are left alone as well.
    /// </summary>
    /// <param name="query">The query to qualify.</param>
    /// <param name="schema">The schema the versions table lives in.</param>
    /// <param name="lineComment">The symbol that begins a comment in the dialect of the query.</param>
    /// <returns>The query, with its references to the versions table qualified.</returns>
    public static string Qualify(string query, string schema, string lineComment = "--") {
        if (string.IsNullOrWhiteSpace(schema)) return query;

        var qualified = new StringBuilder(query.Length);
        var qualifiedUpTo = 0;
        new SqlScanner(lineComment).Scan(query, (start, length) => {
            qualified.Append(query, qualifiedUpTo, start - qualifiedUpTo);
            qualified.Append(Reference().Replace(query.Substring(start, length), $"{schema}.{Table}"));
            qualifiedUpTo = start + length;
        });
        qualified.Append(query, qualifiedUpTo, query.Length - qualifiedUpTo);
        return qualified.ToString();
    }

    /// Matches the versions table only where it stands as a whole, unqualified identifier.
    [GeneratedRegex(@"(?<![\w.])" + Table + @"(?![\w])")]
    private static partial Regex Reference();
}
