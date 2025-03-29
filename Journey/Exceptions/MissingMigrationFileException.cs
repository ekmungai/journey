

internal class MissingMigrationFileException(int version) : FileNotFoundException($"Migration file for version {version} was not found")
{
}