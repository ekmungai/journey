internal record MssqlDialect() : SqlDialect
{
    public override string MigrateVersionsTable() => """
            IF  NOT EXISTS (SELECT * FROM sys.objects 
            WHERE object_id = OBJECT_ID(N'[dbo].[Versions]') AND type in (N'U'))
            CREATE TABLE  [dbo].[Versions] (
                Version INTEGER NOT NULL,
                RunTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                Description varchar(100) NOT NULL,
                RunBy varchar(100) NOT NULL,
                Author varchar(100) NOT NULL
            );
            """;
    public override string InsertVersion() => """
            INSERT INTO [dbo].[Versions] (
                Version,
                Description,
                RunBy,
                Author)
            VALUES ([versionNumber], '', '', '');
            """;
    public override string CurrentVersionQuery() => "SELECT COUNT(*) as [Version] FROM [dbo].[Versions];";
    public override string RollbackVersionsTable() => "DROP TABLE [dbo].[Versions];";
    public override string DeleteVersion() => "DELETE FROM [dbo].[Versions] WHERE [Version] = [versionNumber]";
    public override string HistoryQuery() => "SELECT TOP([entries]) * FROM [dbo].[Versions] ORDER BY [Version] ASC;";
}