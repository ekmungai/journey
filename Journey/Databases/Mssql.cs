
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Journey.Dialects;
using Journey.Exceptions;
using Journey.Interfaces;
using Journey.Models;

namespace Journey.Databases;

/// <inheritdoc/>
internal record Mssql : IDatabase {
    public const string Name = "mssql";
    private readonly SqlDialect _dbDialect = new MssqlDialect();
    private string _connectionString = null!;
    private const string DatabaseNameRegex = "(?i)(database|initial catalog)=([^;]+)";

    /// <inheritdoc/>
    public Task<IDatabase> Connect(string connectionString) {
        _connectionString = connectionString;
        return Task.FromResult<IDatabase>(this);
    }

    /// <inheritdoc/>
    public async Task<IDatabase> Connect(string connectionString, string schema) {
        return await Connect(connectionString);
    }

    /// <inheritdoc/>
    public async Task Execute(string query) {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.CurrentVersionQuery();
        try {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        } catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'dbo.Versions'")) {
            return -1;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.HistoryQuery().Replace("[entries]", entries.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        while (reader.Read()) {
            history.Add(new Itinerary(
                reader["Version"].ToString()!,
                (string)reader["Description"],
                DateTime.SpecifyKind((DateTime)reader["RunTime"], DateTimeKind.Utc),
                (string)reader["RunBy"],
                (string)reader["Author"]
            ));
        }
        await reader.CloseAsync();
        return history;
    }

    /// <inheritdoc/>
    public IDialect GetDialect() {
        return _dbDialect;
    }

    public async Task<bool> CheckDatabase() {
        var databaseName = GetDatabaseName();

        // Connect to 'master' maintenance database to check if target database exists
        var maintenanceConnectionString = Regex.Replace(_connectionString, DatabaseNameRegex, "Database=master");

        await using var connection = new SqlConnection(maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT CAST(COUNT(*) AS BIT) FROM sys.databases WHERE name = '{databaseName}'";
        return (bool)(await command.ExecuteScalarAsync())!;
    }

    /// <inheritdoc/>
    public async Task InitDatabase() {
        var databaseName = GetDatabaseName();

        var maintenanceConnectionString = Regex.Replace(_connectionString, DatabaseNameRegex, "Database=master");

        await using var connection = new SqlConnection(maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose() {
        //
    }

    private string GetDatabaseName() {
        var name = Regex.Match(_connectionString, DatabaseNameRegex).Groups[2].Value;
        return !string.IsNullOrWhiteSpace(name) ? name : throw new MissingDatabaseNameException();
    }
}