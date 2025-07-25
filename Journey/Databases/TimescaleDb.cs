namespace Journey.Databases;

/// <inheritdoc/>
internal record TimescaleDb : Postgres {
    public new const string Name = "timescaledb";
}