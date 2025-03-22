using System.IO;
internal class Migrator(Path versionsDirPath, IFileManager fileManager, Dialect dialect) : IMigrator
{
    private const string yes = "y|yes|Y|Yes";

    public async Task Init()
    {
        var initFilePath = Path.Combine(versionsDirPath, "0.sql");
        if (!File.Exists(initFilePath))
        {
            var answer = Console.ReadLine("Migrations have not been innitialized. Would you like to do so now [Y/N]?");
            if (yes.Contains(answer))
            {
                Scaffold(0);
            }
        }
    }
    public async Task<string> Scaffold(int version)
    {
        var initFilePath = Path.Combine(versionsDirPath, $"{version}.sql");
        var scaffold = version == 0
        ? new ScaffoldInit().Scaffolding
        : new Scaffold().Scaffolding;


        await fileManager.WriteFile(initFilePath, scaffold.ToString());
    }