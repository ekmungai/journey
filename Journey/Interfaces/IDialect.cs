namespace Journey.Interfaces;

/// <summary>
/// The form of SQL language structures Database specific to a Database
/// including some general operations
/// </summary>
public interface IDialect {
    /// <summary>
    /// The end of statement symbol for the dialect.
    /// </summary>
    /// <returns>A string representing the end of a statement.</returns>
    public string Terminator();
    /// <summary>
    /// The symbol for starting a comment for the dialect.
    /// </summary>
    /// <returns>A string representing the beginning of a comment.</returns>
    public string Comment();
    /// <summary>
    /// The symbol for the start of a transaction for the dialect.
    /// </summary>
    /// <returns>A string representing the beginning of a transaction.</returns>
    public string StartTransaction();
    /// <summary>
    /// The symbol for the end of a transaction for the dialect.
    /// </summary>
    /// <returns>A string representing the end of a transaction.</returns>
    public string EndTransaction();
    /// <summary>
    /// SQL queries for creating the versions table for the dialect.
    /// </summary>
    /// <returns>A string representing the queries.</returns>
    public string MigrateVersionsTable();
    /// <summary>
    /// SQL queries for dropping the versions table for the dialect.
    /// </summary>
    /// <returns>A string representing the queries.</returns>
    public string RollbackVersionsTable();
    /// <summary>
    /// SQL query template for adding a record to the versions table for the dialect.
    /// </summary>
    /// <returns>A string representing the template.</returns>
    public string InsertVersion();
    /// <summary>
    /// SQL query template for removing a record from the versions table for the dialect.
    /// </summary>
    /// <returns>A string representing the template.</returns>
    public string DeleteVersion();
}