namespace Journey.Databases;

/// <inheritdoc/>
internal record Mariadb : Mysql {
    public new const string Name = "mariadb";
}