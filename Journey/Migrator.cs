using System.Xml;

internal class Migrator(IFileManager fileManager, IDatabase database) : IMigrator
{
    private const string yes = "y|yes|Y|Yes";
    private List<int> _map = [];
    private List<int> _route = [];
    private string _newLine = Environment.NewLine;

    public async Task Init(bool quiet)
    {
        if (!fileManager.FileExists(0))
        {
            if (quiet)
            {
                await Scaffold(0);
            }
            else
            {
                Console.WriteLine("Migrations have not been initialized. Would you like to do so now? [Y/n]");
                var answer = Console.ReadLine();
                if (answer != null && yes.Contains(answer))
                {
                    await Scaffold(0);
                }
                else
                {
                    Environment.Exit(-1);
                }
            }
        }
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

    public async Task<string> Scaffold(int version)
    {
        var scaffold = new Scaffold(database.GetDialect());
        Console.WriteLine($"Scafffolding version: {version}");
        if (version == 0)
        {
            scaffold.ScaffoldInit();
        }
        var content = scaffold.ToString();
        await fileManager.WriteFile(version, content);
        return $"Version: {version} scaffolded with content {_newLine}{_newLine} {content}";
    }

    public async Task<string> Migrate(int? target)
    {
        await Init(false);
        _map = fileManager.GetMap();
        var currentVersion = await database.CurrentVersion();
        if (target is null)
        {
            var version = currentVersion + 1;
            var route = GetRoute(currentVersion, version);
            if (route.Count > 0)
            {
                await Travel(route);
                return $"The database was succesfully migrated to Version: {version}{_newLine}";
            }
            else
            {
                Console.WriteLine();
                return $"The database is up to date at Version: {currentVersion}{_newLine}";
            }
        }
        else
        {
            return "";
        }
    }

    public Task<string> Rollback(int? target)
    {
        throw new NotImplementedException();
    }

    private async Task<Parser> ParseVersion(int version)
    {
        var content = await fileManager.ReadFile(version);
        var parser = new Parser(content, database.GetDialect());
        parser.ParseFile();
        return parser;
    }

    private List<int> GetRoute(int currentVersion, int targetVersion)
    {
        var route = new List<int>();
        if (currentVersion + 1 == targetVersion)
        {
            route.Add(targetVersion);
        }
        else
        {
            route.AddRange(Enumerable.Range(currentVersion, targetVersion - currentVersion));
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

    private async Task Travel(List<int> route)
    {
        foreach (var waypoint in route)
        {
            Console.WriteLine();
            Console.WriteLine($"Migrating version {waypoint}");
            var parser = await ParseVersion(waypoint);
            var migration = new Migration(database, parser.GetResult());
            await migration.Migrate();

        }
    }
}
