using System.Text;
using System.Xml;

internal class Migrator(IFileManager fileManager, IDatabase database) : IMigrator
{
    private const string yes = "y|yes|Y|Yes";
    private List<int> _map = [];
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
                Console.WriteLine($"{_newLine}Migrations have not been initialized. Would you like to do so now? [Y/n]");
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

    public async Task<string> Scaffold(int version)
    {
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

    public async Task<string> Migrate(int? target)
    {
        var (currentVersion, newVersion) = await GetVersions(target);
        var route = GetRoute(currentVersion, newVersion);
        if (route.Count > 0)
        {
            await TravelForwards(route);
            return $"{_newLine}The database was succesfully migrated to Version: {newVersion}{_newLine}";
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

    public async Task<string> Rollback(int? target)
    {
        var (currentVersion, newVersion) = await GetVersions(target);
        var route = GetRoute(currentVersion, newVersion);
        if (route.Count > 0)
        {
            await TravelBackwards(route);
            return $"{_newLine}The database was succesfully rollback back to Version: {newVersion}{_newLine}";
        }
        else
        {
            return $"{_newLine}The database is up to date at Version: {currentVersion}{_newLine}";
        }
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
            route.AddRange(Enumerable.Range(currentVersion + 1, targetVersion - currentVersion));
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

    private async Task TravelForwards(List<int> route)
    {
        if (route.Count > 1)
        {
            Console.WriteLine($"{_newLine}Migration route is: {string.Join(" -> ", route)}");
        }
        foreach (var waypoint in route)
        {
            Console.WriteLine($"{_newLine}Migrating version {waypoint}");
            var parser = await ParseVersion(waypoint);
            var migration = new Migration(database, parser.GetResult());
            await migration.Migrate();

        }
    }

    private async Task TravelBackwards(List<int> route)
    {
        route.Reverse();
        if (route.Count > 1)
        {
            Console.WriteLine($"{_newLine}Migration route is: {string.Join(" -> ", route)}");
        }
        foreach (var waypoint in route)
        {
            Console.WriteLine($"{_newLine}Rolling back version {waypoint}");
            var parser = await ParseVersion(waypoint);
            var rollback = new Rollback(database, parser.GetResult());
            await rollback.Reverse();
        }
    }

    private async Task<(int currentVersion, int nextVersion)> GetVersions(int? target)
    {
        _map = fileManager.GetMap();
        var currentVersion = await database.CurrentVersion();
        var newVersion = target == null
            ? currentVersion + 1
            : target.Value;
        return (currentVersion, newVersion);
    }
}
