namespace Journey.Dialects;

/// <inheritdoc/>
internal record MssqlDialect : SqlDialect {
    /// <inheritdoc/>
    public override string MigrateVersionsTable() => """
                                                     IF  NOT EXISTS (SELECT * FROM sys.objects 
                                                     WHERE object_id = OBJECT_ID(N'[dbo].[Versions]') AND type in (N'U'))
                                                     CREATE TABLE  [dbo].[Versions] (
                                                         Version INTEGER NOT NULL,
                                                         RunTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                                                         description varchar(1000) NOT NULL,
                                                         RunBy varchar(100) NOT NULL,
                                                         Author varchar(100) NOT NULL
                                                     );
                                                     """;
    /// <inheritdoc/>
    public override string InsertVersion() => """
                                              INSERT INTO [dbo].[Versions] (
                                                  Version,
                                                  Description,
                                                  RunBy,
                                                  Author)
                                              VALUES ([versionNumber], '', '', '');
                                              """;
    /// <inheritdoc/>
    public override string CurrentVersionQuery() => "SELECT COUNT(*) as [Version] FROM [dbo].[Versions];";
    /// <inheritdoc/>
    public override string RollbackVersionsTable() => "DROP TABLE [dbo].[Versions];";
    /// <inheritdoc/>
    public override string DeleteVersion() => "DELETE FROM [dbo].[Versions] WHERE [Version] = [versionNumber]";
    /// <inheritdoc/>
    public override string HistoryQuery() => "SELECT TOP([entries]) * FROM [dbo].[Versions] ORDER BY [Version] ASC;";
}