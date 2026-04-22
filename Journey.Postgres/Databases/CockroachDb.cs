namespace Journey.Databases;

/// <inheritdoc/>
internal record CockroachDb : Postgres {
    public new const string Name = "cockroachdb";
}