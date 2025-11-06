using System.Text;
using Journey.Exceptions;
using Journey.Interfaces;
using Journey.Models;

#pragma warning disable CS8603 // Possible null reference return.
namespace Journey;

/// <inheritdoc />
internal class Parser : IParser {
    private int _version;
    private int _openSection;
    private int _openTransaction;
    private readonly StringBuilder _block = new();
    private readonly Queue<string> _fileContents;
    private readonly Dictionary<string, List<string>> _result = new() {
        { Migration, [] },
        { Rollback, [] }
    };
    private readonly IDialect _dialect;
    private readonly Scaffold _scaffold;
    private readonly string[] _sectionStart;
    private readonly string[] _sectionEnd;
    public const string Migration = "Migration";
    public const string Rollback = "Rollback";

    public Parser(int version, string[] content, IDialect dialect) {
        _version = version; ;
        _dialect = dialect;
        _scaffold = new Scaffold(dialect, null);
        var firstSectionIndex = GetFirstSectionIndex(content);
        _fileContents = new Queue<string>(content
            .Skip(firstSectionIndex) // skip header
            .Where(q => !string.IsNullOrWhiteSpace(q))); // remove blank spaces
        _sectionStart = [_scaffold.GetScaffolding()[1], _scaffold.GetScaffolding()[7]];
        _sectionEnd = [_scaffold.GetScaffolding()[6], _scaffold.GetScaffolding()[12]];
    }

    /// <inheritdoc />
    public Dictionary<string, List<string>> GetResult() => _result;

    /// <summary>
    /// Represents the contents of the parsed file as a string.
    /// </summary>
    /// <returns>A string representation of the contents in the parsed file.</returns>
    public override string ToString() {
        var sb = new StringBuilder();

        foreach (var section in _result.Keys) {
            sb.AppendLine();
            sb.AppendLine(section);
            sb.AppendLine("----------------");
            foreach (var item in _result[section]) {
                sb.AppendLine(item);
            }

        }
        return sb.ToString();
    }
    public Queue<string>? ParseFile() {
        if (_fileContents.Count <= 0) {
            Validate();
            return null;
        }

        var line = _fileContents.Peek();
        var section = line == _scaffold.GetScaffolding()[1]
            ? _result[Migration]
            : _result[Rollback];
        return ParseSection(_fileContents, section);
    }

    private Queue<string> ParseSection(Queue<string> sectionContents, List<string> section) {
        if (sectionContents.Count <= 0) return ParseFile();
        var line = sectionContents.Peek();

        if (_sectionEnd.Contains(line)) {
            _openSection--;
            sectionContents.Dequeue();
            return ParseFile();
        }

        if (!_sectionStart.Contains(line)) return ParseFile();

        _openSection++;
        return ParseTransaction(sectionContents, section);
    }

    private Queue<string>? ParseTransaction(Queue<string> transactionContent, List<string> section) {
        if (transactionContent.Count <= 0) return null;

        var line = transactionContent.Peek();

        if (_sectionEnd.Contains(line)) {
            _openSection--;
            transactionContent.Dequeue();
            return ParseFile();
        }

        line = GetNextLine(transactionContent);
        if (line != _dialect.StartTransaction()) {
            throw new InvalidFormatException(_version, line);
        }
        _openTransaction++;

        if (Array.Exists(_dialect.EndTransaction(), element => element.ToUpperInvariant() == line)) {
            _openTransaction--;
            section.Add(line);
            ParseSection(transactionContent, section);
        } else {
            section.Add(line);
            return ParseQueries(transactionContent, section);
        }
        return null;
    }
    private Queue<string>? ParseQueries(Queue<string> queries, List<string> section) {
        if (queries.Count <= 0) return null;
        var line = GetNextLine(queries);

        if (line == _dialect.StartTransaction()) {
            throw new InvalidFormatException(_version, line);
        }

        if (!line.Contains(_dialect.Terminator())) {
            return ParseBlock(line, queries, section);
        }

        if (Array.Exists(_dialect.EndTransaction(), element => element.ToUpperInvariant() == line)) {
            _openTransaction--;
            section.Add(line);
            ParseTransaction(queries, section);
        } else {
            section.Add(line);
            return ParseQueries(queries, section);
        }
        return null;
    }
    private Queue<string> ParseBlock(string? firstLine, Queue<string> blockContents, List<string> section) {
        if (firstLine != null) {
            _block.AppendLine(firstLine);
        }
        var line = GetNextLine(blockContents);
        if (line.Contains(_dialect.Terminator())) {
            _block.Append(line);
            section.Add(_block.ToString());
            _block.Clear();
            return ParseQueries(blockContents, section);
        }

        _block.AppendLine(line);
        return ParseBlock(null, blockContents, section);
    }
    private void Validate() {
        if (_openTransaction > 0) {
            throw new OpenTransactionException(_version);
        }
    }
    private int GetFirstSectionIndex(string[] content) {
        var migrationSectionStartIndex = content.ToList().IndexOf(_scaffold.GetScaffolding()[1]);
        var rollbackSectionStartIndex = content.ToList().IndexOf(_scaffold.GetScaffolding()[7]);
        var migrationSectionEndIndex = content.ToList().IndexOf(_scaffold.GetScaffolding()[6]);
        var rollbackSectionEndIndex = content.ToList().IndexOf(_scaffold.GetScaffolding()[12]);

        if (migrationSectionStartIndex < 0) {
            throw new MissingSectionException(_version, Migration);
        }
        if (rollbackSectionStartIndex < 0) {
            throw new MissingSectionException(_version, Rollback);
        }
        if (migrationSectionEndIndex < 0) {
            throw new OpenSectionException(_version, Migration);
        }
        if (rollbackSectionEndIndex < 0) {
            throw new OpenSectionException(_version, Rollback);
        }
        return Math.Min(migrationSectionStartIndex, rollbackSectionStartIndex);
    }
    private string GetNextLine(Queue<string> lines) {
        try {
            while (IsComment(lines.Peek())) {
                lines.Dequeue();
            }
            return lines.Dequeue();
        } catch (InvalidOperationException) {
            throw new OpenTransactionException(_version);
        }
    }
    private bool IsComment(string line) => line[..2] == _dialect.Comment();
}

#pragma warning restore CS8603 // Possible null reference return.