
using System.Text.RegularExpressions;
using Journey.Dialects;
using Journey.Exceptions;
using Journey.Interfaces;
using Journey.Models;
using MySqlConnector;

namespace Journey.Databases;

/// <inheritdoc/>
internal record Mysql : IDatabase {
    public const string Name = "mysql";
    private readonly SqlDialect _dbDialect = new MysqlDialect();
    private string _connectionString = null!;
    private const string DatabaseNameRegex = "(?i)(database|db)=([^;]+)";

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
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = query;
        await command.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CurrentVersion() {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.CurrentVersionQuery();
        try {
            var result = await command.ExecuteScalarAsync();
            return int.Parse(result!.ToString() ?? "");
        } catch (MySqlException ex) when (ex.Message.Contains($"versions' doesn't exist")) {
            return -1;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Itinerary>> GetItinerary(int entries) {
        var history = new List<Itinerary>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = _dbDialect.HistoryQuery().Replace("[entries]", entries.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        while (reader.Read()) {
            history.Add(new Itinerary(
                reader["version"].ToString()!,
                (string)reader["description"],
                DateTime.SpecifyKind((DateTime)reader["run_time"], DateTimeKind.Utc),
                (string)reader["run_by"],
                (string)reader["author"]
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

        // Connect without specifying a database to check if target database exists
        var maintenanceConnectionString = Regex.Replace(_connectionString, DatabaseNameRegex + ";?", "");

        await using var connection = new MySqlConnection(maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}')";
        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    /// <inheritdoc/>
    public async Task InitDatabase() {
        var databaseName = GetDatabaseName();

        await using var connection = new MySqlConnection(Regex.Replace(_connectionString, DatabaseNameRegex + ";?", ""));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE `{databaseName}`";
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