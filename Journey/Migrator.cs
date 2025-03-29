internal class Migrator(FileManager fileManager, IDatabase database) : IMigrator
{
    private const string yes = "y|yes|Y|Yes";

    public async Task Init(bool quiet)
    {
        var initFilePath = Path.Combine(fileManager.VersionsDir, "0.sql");
        if (!File.Exists(initFilePath))
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
        var content = await fileManager.ReadFile(version);
        try
        {
            var parser = new Parser(content, database.GetDialect());
            parser.ParseFile();
            var log = $"\nFile for version {version} is valid with the queries: \n";
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
        return $"Version: {version} scaffolded with content \n\n {content}";
    }

    public Task<string> Migrate(int? target)
    {
        throw new NotImplementedException();
    }

    public Task<string> Rollback(int? target)
    {
        throw new NotImplementedException();
    }
}
