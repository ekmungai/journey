using System.Text;

internal class Migrator(IFileManager fileManager, IDatabase database) : IMigrator
{
    private const string yes = "y|yes|Y|Yes";
    private List<int> _map = [];
    private int _currentVersion;
    private string _newLine = Environment.NewLine;


    public async Task Init(bool quiet)
    {
        await InitState();

        if (!fileManager.FileExists(0))
        {
            if (quiet)
            {
                await Scaffold();
            }
            else
            {
                Console.WriteLine($"{_newLine}Migrations have not been initialized. Would you like to do so now? [Y/n]");
                var answer = Console.ReadLine();
                if (answer != null && yes.Contains(answer))
                {
                    await Scaffold();
                }
                else
                {
                    Environment.Exit(-1);
                }
            }
        }
    }

    public async Task<string> Scaffold()
    {
        await InitState();

        var version = _currentVersion++;
        var scaffold = new Scaffold(database.GetDialect(), version);
        Console.WriteLine($"{_newLine}Scafffolding version: {version}");
        if (version == 0)
        {
            scaffold.ScaffoldInit();
        }
        var content = scaffold.ToString();
        await fileManager.WriteFile(version, content);
        return $"Version: {version} scaffolded with content {_newLine}{_newLine} {content}";
    }

    public async Task<string> Validate(int version)
    {
        try
        {
            var parser = await ParseVersion(version);
            var log = $"{_newLine}File for version {version} is valid with the queries: {_newLine}";
            return log + parser.ToString();
        }
        catch (Exception e)
        {
            return $"File for version {version} is invalid with error: '{e.Message}'";
        }
    }

    public async Task<string> Migrate(int? target, bool? debug)
    {
        await InitState();
        var (currentVersion, newVersion) = await GetVersions(target, 1);
        var route = GetRoute(currentVersion, newVersion, 1);
        if (route.Count > 0)
        {
            await Travel(route, 1, debug ?? false);
            return $"{_newLine}The database was succesfully migrated to version: {newVersion}{_newLine}";
        }
        else
        {
            return $"{_newLine}The database is up to date at Version: {currentVersion}{_newLine}";
        }
    }

    public async Task<string> History(int entries)
    {
        var diary = new StringBuilder();
        diary.AppendLine($"{_newLine}Version | RunTime \t\t\t| Description \t\t\t| RunBy \t| Author");
        foreach (var Itinerary in await database.GetItinerary(entries))
        {
            diary.AppendLine(Itinerary.ToString());
        }
        return diary.ToString();
    }

    public async Task<string> Rollback(int? target, bool? debug)
    {
        await InitState();
        var (currentVersion, newVersion) = await GetVersions(target, -1);
        var route = GetRoute(currentVersion, newVersion, -1);
        if (route.Count > 0)
        {
            await Travel(route, -1, debug ?? false);
            return $"{_newLine}The database was succesfully rolled back to version: {newVersion}{_newLine}";
        }
        else
        {
            return $"{_newLine}The database is up to date at Version: {currentVersion}{_newLine}";
        }
    }

    public async Task<string> Update(bool? debug)
    {
        await InitState();
        var latest = _map[^1];
        return await Migrate(latest, debug);
    }

    private async Task<Parser> ParseVersion(int version)
    {
        var content = await fileManager.ReadFile(version);
        var parser = new Parser(content, database.GetDialect());
        parser.ParseFile();
        return parser;
    }

    private List<int> GetRoute(int currentVersion, int targetVersion, int direction)
    {
        var route = new List<int>();

        if (currentVersion == targetVersion)
        {
            return route;
        }

        if (direction < 0)
        {
            route.Add(currentVersion); // Rollbacks always start from the current version
        }
        while (targetVersion != currentVersion)
        {
            currentVersion += direction;
            if (currentVersion == -1 && direction == -1)
            {
                break;
            }
            route.Add(currentVersion);
        }

        foreach (var waypoint in route)
        {
            if (!_map.Contains(waypoint))
            {
                throw new MissingMigrationFileException(waypoint);
            }
        }
        return route;
    }

    private async Task Travel(List<int> route, int direction, bool debug)
    {
        if (route.Count > 1)
        {
            Console.WriteLine($"{_newLine}Migration route is: {string.Join(" -> ", route)}");
        }
        foreach (var waypoint in route)
        {

            var parser = await ParseVersion(waypoint);
            var migration = new Migration(database, parser.GetResult());
            if (direction > 0)
            {
                Console.WriteLine($"{_newLine}Migrating version {waypoint}");
                await migration.Migrate(debug);
            }
            else
            {
                Console.WriteLine($"{_newLine}Rolling back version {waypoint}");
                await migration.Rollback(debug);
            }


        }
    }

    private async Task<(int currentVersion, int nextVersion)> GetVersions(int? target, int direction)
    {
        await InitState();
        var newVersion = target == null
            ? _currentVersion + direction
            : target.Value;
        if (newVersion < -1)
        {
            throw new InvalidRollbackException("Cannot rollback beyond version -1");
        }
        if (newVersion > _currentVersion && direction < 0)
        {
            throw new InvalidRollbackException($"Cannot rollback to a higher version. Target: {newVersion} > Current: {_currentVersion}");
        }
        if (_currentVersion > newVersion && direction > 0)
        {
            throw new InvalidMigrationException($"Cannot migrate to a lower version. Target: {newVersion} < Current: {_currentVersion}");
        }
        return (_currentVersion, newVersion);
    }
    private async Task InitState()
    {
        if (_map.Count == 0)
        {
            _map = fileManager.GetMap();
        }
        _currentVersion = await database.CurrentVersion();
    }
}
