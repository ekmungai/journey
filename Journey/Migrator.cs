using System.Text;
using Journey.Exceptions;
using Journey.Interfaces;
using Journey.Loggers;
using Journey.Models;

namespace Journey;

/// <inheritdoc/>
internal class Migrator(IFileManager fileManager, IDatabase database, bool? verbose) : IMigrator {
    private const string Yes = "y|yes|Y|Yes";
    private List<int> _map = [];
    private int _currentVersion;
    private readonly string _newLine = Environment.NewLine;
    private readonly bool _verbose = verbose ?? false;
    private ILogger _logger = new ConsoleLogger();

    internal void SetLogger(ILogger logger) {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Init(bool quiet) {
        await InitState();

        if (!fileManager.FileExists(0)) {
            if (quiet) {
                await MigrateInit();
            } else {
                _logger.Information($"{_newLine}Migrations have not been initialized. Would you like to do so now? [Y/n]");
                var answer = Console.ReadLine();
                if (answer != null && Yes.Contains(answer)) {
                    await MigrateInit();
                } else {
                    Environment.Exit(-1);
                }
            }
        }
    }
    /// <inheritdoc/>
    public async Task Scaffold() {
        await InitState();

        var version = _currentVersion + 1;
        var scaffold = new Scaffold(database.GetDialect(), version);
        _logger.Information($"{_newLine}Scafffolding version: {version}");
        var content = scaffold.ToString();
        await fileManager.WriteFile(version, content);
        var log = $"Version: {version} scaffolded";
        if (_verbose) {
            log += $"with content {_newLine}{_newLine} {content}";
        }
        _logger.Information(log);
    }
    /// <inheritdoc/>
    public async Task<bool> Validate(int version) {
        try {
            var parser = await ParseVersion(version);
            var log = $"{_newLine}File for version {version} is valid";
            if (_verbose) {
                log += $" with the queries: {_newLine}" + parser;
            }
            _logger.Information(log);
            return true;
        } catch (Exception e) {
            _logger.Error(e, $"File for version {version} is invalid with error: '{e.Message}'");
            return false;
        }
    }
    /// <inheritdoc/>
    public async Task Migrate(int? target, bool? dryRun) {
        await InitState();
        var (currentVersion, newVersion) = GetVersions(target, 1);
        var route = GetRoute(currentVersion, newVersion, 1);
        if (route.Count > 0) {
            await Travel(route, 1);
            if (dryRun.GetValueOrDefault()) {
                _logger.Information($"{_newLine}INFO: Dry run mode is enabled, rolling back migration changes");
                await Rollback(currentVersion);
            } else {
                _logger.Information($"{_newLine}The database was successfully migrated to version: {newVersion}{_newLine}");
            }
        } else {
            _logger.Information($"{_newLine}The database is up to date at Version: {currentVersion}{_newLine}");
        }
    }
    /// <inheritdoc/>
    public async Task<string> History(int entries) {
        var diary = new StringBuilder();
        diary.AppendLine($"{_newLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author");
        foreach (var itinerary in await database.GetItinerary(entries)) {
            diary.AppendLine(itinerary.ToString());
        }
        var report = diary.ToString();
        _logger.Information($"{report}");
        return report;
    }
    /// <inheritdoc/>
    public async Task Rollback(int? target) {
        await InitState();
        var (currentVersion, newVersion) = GetVersions(target, -1);
        var route = GetRoute(currentVersion, newVersion, -1);
        if (route.Count > 0) {
            await Travel(route, -1);
            _logger.Information($"{_newLine}The database was succesfully rolled back to version: {newVersion}{_newLine}");
        } else {
            _logger.Information($"{_newLine}The database is up to date at Version: {currentVersion}{_newLine}");
        }
    }
    /// <inheritdoc/>
    public async Task Update(int? target = null) {
        await Init(true);
        int? upgrade = null;
        int? downgrade = null;
        var latest = _map[^1];

        if (target.HasValue && target.Value > _currentVersion) {
            upgrade = target;
        } else if (target.HasValue && target.Value < _currentVersion) {
            downgrade = target;
        } else if (_currentVersion < latest) {
            upgrade = latest;
        } else if (_currentVersion > latest) {
            downgrade = latest;
        }

        if (upgrade.HasValue) {
            await Migrate(upgrade, false);
        }
        if (downgrade.HasValue) {
            await Rollback(downgrade);
        }
    }

    private async Task<Parser> ParseVersion(int version) {
        var content = await fileManager.ReadFile(version);
        var parser = new Parser(content, database.GetDialect());
        parser.ParseFile();
        return parser;
    }

    private List<int> GetRoute(int currentVersion, int targetVersion, int direction) {
        var route = new List<int>();

        if (currentVersion == targetVersion) {
            return route;
        }

        if (direction < 0) {
            route.Add(currentVersion); // Rollbacks always start from the current version
            targetVersion++; // And should stop just short of the target
        }
        while (targetVersion != currentVersion) {
            currentVersion += direction;
            if (currentVersion == -1 && direction == -1) {
                break;
            }
            route.Add(currentVersion);
        }

        foreach (var waypoint in route) {
            if (!_map.Contains(waypoint)) {
                throw new MissingMigrationFileException(waypoint);
            }
        }
        return route;
    }

    private async Task Travel(List<int> route, int direction) {
        if (route.Count > 1) {
            _logger.Information($"{_newLine}Migration route is: {string.Join(" -> ", route)}");
        }
        foreach (var waypoint in route) {

            var parser = await ParseVersion(waypoint);
            var migration = new Migration(database, parser.GetResult(), _logger.Debug);
            if (direction > 0) {
                _logger.Information($"{_newLine}Migrating version {waypoint}");
                await migration.Migrate(_verbose);

            } else {
                _logger.Information($"{_newLine}Rolling back version {waypoint}");
                await migration.Rollback(_verbose);
            }

        }
    }

    private (int currentVersion, int nextVersion) GetVersions(int? target, int direction) {
        var newVersion = target ?? _currentVersion + direction;
        if (newVersion < -1) {
            throw new InvalidRollbackException("Cannot rollback beyond version -1");
        }
        if (newVersion > _currentVersion && direction < 0) {
            throw new InvalidRollbackException($"Cannot rollback to a higher version. Target: {newVersion} > Current: {_currentVersion}");
        }
        if (_currentVersion > newVersion && direction > 0) {
            throw new InvalidMigrationException($"Cannot migrate to a lower version. Target: {newVersion} < Current: {_currentVersion}");
        }
        return (_currentVersion, newVersion);
    }
    private async Task InitState() {
        if (_map.Count == 0) {
            _map = fileManager.GetMap();
        }
        _currentVersion = await database.CurrentVersion();
    }

    private async Task MigrateInit() {
        var scaffold = new Scaffold(database.GetDialect(), 0);
        scaffold.ScaffoldInit();
        var content = scaffold.ToString();
        await fileManager.WriteFile(0, content);
        await Migrate(0, false);
    }
}