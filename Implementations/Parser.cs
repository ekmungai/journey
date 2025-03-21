
using Journey.Interfaces;
namespace Journey
{
    class Parser : IParser
    {
        public Task ParseFile(List<string> fileContents)
        {
            return Task.FromResult(0);
        }
        public void ParseSection(List<string> sectionContents) { }
        public void ParseBlock(List<string> blockContents) { }
        public void ParseQueries(List<string> queries) { }
    }
}