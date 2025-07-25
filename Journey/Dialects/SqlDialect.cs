using Journey.Interfaces;

namespace Journey.Dialects;

/// <inheritdoc/>
internal abstract record SqlDialect : IDialect {
    /// <inheritdoc/>
    public abstract string MigrateVersionsTable();
    /// <inheritdoc/>
    public abstract string InsertVersion();
    /// <inheritdoc/>
    public virtual string StartTransaction() => "BEGIN;";
    /// <inheritdoc/>
    public virtual string EndTransaction() => "END;";
    /// <inheritdoc/>
    public virtual string DeleteVersion() => "DELETE FROM versions WHERE version = [versionNumber];";
    /// <inheritdoc/>
    public virtual string RollbackVersionsTable() => "DROP TABLE versions;";
    /// Query for getting the current version of the database
    public virtual string CurrentVersionQuery() => "SELECT COUNT(*) as version FROM versions;";
    /// Query for getting the history entries in the database
    public virtual string HistoryQuery() => "SELECT * FROM versions ORDER BY version LIMIT [entries];";
    /// <inheritdoc/>
    public string Comment() => "--";
    /// <inheritdoc/>
    public string Terminator() => ";";
}