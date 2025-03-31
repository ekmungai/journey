public interface IParser
{
    public Queue<string>? ParseFile();
    public Dictionary<string, List<string>> GetResult();

}