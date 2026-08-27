using System.Data.Common;
using Journey.Interfaces;

namespace Journey.Helpers;

/// <summary>
/// Runs all the queries of a migration or rollback on a single connection, turning the transactions
/// declared in the migration file into real database transactions.
///
/// The delimiters are not sent to the server: a transaction started on one pooled connection would
/// not cover statements that ran on another, so the file's BEGIN and COMMIT would wrap nothing and
/// a failure part of the way through would leave the earlier statements committed.
/// </summary>
internal static class BatchExecutor {

    /// <summary>
    /// Executes the queries in order, committing at each transaction delimiter and rolling back the
    /// transaction in progress if any of them fails.
    /// </summary>
    /// <param name="connection">The open connection to run every query on.</param>
    /// <param name="queries">The queries to run, including the transaction delimiters.</param>
    /// <param name="dialect">The dialect the transaction delimiters are written in.</param>
    /// <param name="rewrite">Applied to each query before it is executed, if given.</param>
    /// <param name="onQuery">Called with each query before it is executed, if given.</param>
    public static async Task Execute(
        DbConnection connection,
        IReadOnlyList<string> queries,
        IDialect dialect,
        Func<string, string>? rewrite = null,
        Action<string>? onQuery = null
    ) {
        DbTransaction? transaction = null;
        try {
            foreach (var next in queries) {
                var query = next.Trim();
                onQuery?.Invoke(query);

                if (IsStartTransaction(query, dialect)) {
                    transaction = await connection.BeginTransactionAsync();
                    continue;
                }

                if (IsEndTransaction(query, dialect)) {
                    if (transaction != null) {
                        await transaction.CommitAsync();
                        await transaction.DisposeAsync();
                        transaction = null;
                    }
                    continue;
                }

                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = rewrite == null ? query : rewrite(query);
                await command.ExecuteNonQueryAsync();
            }
        } catch {
            if (transaction != null) {
                try {
                    await transaction.RollbackAsync();
                } catch {
                    // The server may have aborted the transaction already, in which case there is
                    // nothing left to roll back and the original failure is the one worth reporting.
                }
                await transaction.DisposeAsync();
            }
            throw;
        }

        if (transaction == null) return;
        // The parser rejects unclosed transactions, so this is only reachable if the queries were
        // not produced by it. Committing them would be exactly the silent half apply the real
        // transaction is here to prevent.
        await transaction.RollbackAsync();
        await transaction.DisposeAsync();
        throw new InvalidOperationException("The queries end with an unclosed transaction, which has been rolled back");
    }

    private static bool IsStartTransaction(string query, IDialect dialect)
        => !string.IsNullOrEmpty(dialect.StartTransaction()) && query == dialect.StartTransaction();

    private static bool IsEndTransaction(string query, IDialect dialect)
        => Array.Exists(dialect.EndTransaction(),
            delimiter => !string.IsNullOrEmpty(delimiter) && delimiter.ToUpperInvariant() == query);
}
