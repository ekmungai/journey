using Journey.Interfaces;

namespace Journey.Dialects;

/// <inheritdoc/>
public abstract record SqlDialect : IDialect {
    /// <inheritdoc/>
    public abstract string MigrateVersionsTable();
    /// <inheritdoc/>
    public abstract string InsertVersion();
    /// <inheritdoc/>
    public virtual string StartTransaction() => "BEGIN;";
    /// <inheritdoc/>
    public virtual string[] EndTransaction() => ["COMMIT;", "END;"];
    /// <inheritdoc/>
    public virtual string DeleteVersion() => "DELETE FROM versions WHERE version = [versionNumber];";
    /// <inheritdoc/>
    public virtual string RollbackVersionsTable() => "DROP TABLE versions;";
    /// Query for getting the current version of the database: the highest version recorded, not
    /// the number of rows, so a missing, duplicate or hand inserted row cannot shift it.
    public virtual string CurrentVersionQuery() => "SELECT COALESCE(MAX(version), 0) as version FROM versions;";
    /// Query for getting the history entries in the database
    public virtual string HistoryQuery() => "SELECT * FROM versions ORDER BY version LIMIT [entries];";
    /// <inheritdoc/>
    public string Comment() => "--";
    /// <inheritdoc/>
    public string Terminator() => ";";
}