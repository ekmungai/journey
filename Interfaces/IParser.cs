namespace Journey.Interfaces
{
    public interface IParser
    {
        public Task ParseFile(List<string> fileContents);
        public void ParseSection(List<string> sectionContents);
        public void ParseBlock(List<string> blockContents);
        public void ParseQueries(List<string> queries);

    }
}
