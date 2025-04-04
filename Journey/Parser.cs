using System.Text;

#pragma warning disable CS8603 // Possible null reference return.
internal class Parser : IParser {
    private int _openSection = 0;
    private int _openTransaction = 0;
    private StringBuilder _block = new();
    private readonly Queue<string> _fileContents;
    private Dictionary<string, List<string>> _result = new() {
        { Migration, [] },
        { Rollback, [] }
    };
    private readonly IDialect _dialect;
    private readonly Scaffold _scaffold;
    private readonly string[] _sectionStart;
    private readonly string[] _sectionEnd;
    public const string Migration = "Migration";
    public const string Rollback = "Rollback";
    public Parser(string[] content, IDialect dialect) {
        _dialect = dialect;
        _scaffold = new Scaffold(dialect, null);
        var firstSectionIndex = GetFirstSectionIndex(content);
        _fileContents = new Queue<string>(content
            .Skip(firstSectionIndex) // skip header
            .Where(q => !string.IsNullOrWhiteSpace(q))); // remove blank spaces
        _sectionStart = [_scaffold.Scaffolding[1], _scaffold.Scaffolding[7]];
        _sectionEnd = [_scaffold.Scaffolding[6], _scaffold.Scaffolding[12]];
    }


    public Dictionary<string, List<string>> GetResult() => _result;

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
        var section = line == _scaffold.Scaffolding[1]
        ? _result[Migration]
        : _result[Rollback];
        return ParseSection(_fileContents, section);
    }

    private Queue<string> ParseSection(Queue<string> sectionContents, List<string> section) {
        if (sectionContents.Count > 0) {
            var line = sectionContents.Peek();
            if (_sectionEnd.Contains(line)) {
                _openSection--;
                sectionContents.Dequeue();
                return ParseFile();
            } else {
                if (_sectionStart.Contains(line)) {
                    _openSection++;
                    line = GetNextLine(sectionContents);
                    if (line != _dialect.StartTransaction()) {
                        throw new InvalidFormatException(line);
                    }
                    _openTransaction++;
                    section.Add(line);
                    return ParseQueries(sectionContents, section);
                }
            }

        }
        return ParseFile();

    }
    private Queue<string>? ParseQueries(Queue<string> queries, List<string> section) {
        if (queries.Count > 0) {
            var line = GetNextLine(queries);
            if (!line.Contains(_dialect.Terminator())) {
                return ParseBlock(line, queries, section);
            } else {
                if (line == _dialect.StartTransaction()) {
                    throw new InvalidFormatException(line);
                }

                if (line == _dialect.EndTransaction()) {
                    _openTransaction--;
                    section.Add(line);
                    ParseSection(queries, section);
                } else {
                    section.Add(line);
                    return ParseQueries(queries, section);
                }
            }
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
        } else {
            _block.AppendLine(line);
            return ParseBlock(null, blockContents, section);
        }
    }
    private void Validate() {
        if (_openSection > 0) {
            throw new OpenSectionException();
        }
        if (_openTransaction > 0) {
            throw new OpenTransactionException();
        }
    }
    private int GetFirstSectionIndex(string[] content) {
        var migrationSectionIndex = content.ToList().IndexOf(_scaffold.Scaffolding[1]);
        var rollbackSectionIndex = content.ToList().IndexOf(_scaffold.Scaffolding[7]);
        if (migrationSectionIndex < 0) {
            throw new MissingSectionException(Migration);
        }
        if (rollbackSectionIndex < 0) {
            throw new MissingSectionException(Rollback);
        }
        return Math.Min(migrationSectionIndex, rollbackSectionIndex);
    }
    private string GetNextLine(Queue<string> lines) {
        try {
            while (IsComment(lines.Peek())) {
                lines.Dequeue();
            }
            return lines.Dequeue();
        } catch (InvalidOperationException) {
            throw new OpenTransactionException();
        }
    }
    private bool IsComment(string line) => line[..2] == _dialect.Comment();
}

#pragma warning restore CS8603 // Possible null reference return.