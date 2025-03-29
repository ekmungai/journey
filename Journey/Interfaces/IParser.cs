public interface IParser
{
    public Queue<string>? ParseFile();
    public Queue<string>? ParseSection(Queue<string> sectionContents, List<string> section);
    public Queue<string>? ParseBlock(Queue<string> blockContents, List<string> section);
    public Queue<string>? ParseQueries(Queue<string> queries, List<string> section);
    public Dictionary<string, List<string>> GetResult();

}