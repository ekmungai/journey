namespace Journey.Helpers;

/// <summary>
/// Tells the code in a piece of sql apart from its comments and quoted text.
///
/// Migration files are read a line at a time, and both quoted text and block comments may span
/// lines, so the scanner keeps its state between calls. Scanning the lines of a file in order
/// therefore reports each character exactly once, in the context it actually appears in.
/// </summary>
internal sealed class SqlScanner(string lineComment) {
    private enum State { Code, LineComment, BlockComment, Quoted, DollarQuoted }

    private State _state = State.Code;
    private char _quote;
    private string _dollarTag = "";

    /// <summary>
    /// Whether the scanner is currently in code, rather than in the middle of a block comment or
    /// of quoted text that was left open by the previous line.
    /// </summary>
    public bool InCode => _state == State.Code;

    /// <summary>
    /// Scans the text, advancing the state of the scanner, and reports whether the terminator
    /// appears in it as code. A terminator inside a comment or a string literal does not end a
    /// statement, so it is not reported.
    /// </summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="terminator">The end of statement symbol to look for.</param>
    /// <returns>True if the text contains the terminator as code.</returns>
    public bool ContainsTerminator(string text, string terminator) {
        var found = false;
        Scan(text, (start, length) => found |= text.IndexOf(terminator, start, length, StringComparison.Ordinal) >= 0);
        return found;
    }

    /// <summary>
    /// Scans the text, advancing the state of the scanner, and reports every run of characters in
    /// it that is code rather than a comment or quoted text.
    /// </summary>
    /// <param name="text">The text to scan.</param>
    /// <param name="onCode">Called with the offset and length of each run of code.</param>
    public void Scan(string text, Action<int, int>? onCode = null) {
        var codeStart = InCode ? 0 : -1;
        var index = 0;

        void Leave(State state, int at) {
            if (codeStart >= 0 && at > codeStart) {
                onCode?.Invoke(codeStart, at - codeStart);
            }
            codeStart = -1;
            _state = state;
        }

        void Enter(int at) {
            _state = State.Code;
            codeStart = at;
        }

        while (index < text.Length) {
            var character = text[index];
            switch (_state) {
                case State.LineComment:
                    // A line comment runs to the end of the line and no further.
                    if (character is '\n') {
                        Enter(index + 1);
                    }
                    index++;
                    break;
                case State.BlockComment:
                    if (character is '*' && Follows(text, index + 1, '/')) {
                        index += 2;
                        Enter(index);
                    } else {
                        index++;
                    }
                    break;
                case State.Quoted:
                    if (character != _quote) {
                        index++;
                    } else if (Follows(text, index + 1, _quote)) {
                        index += 2; // a doubled quote is an escaped quote, not the end of the text
                    } else {
                        index++;
                        Enter(index);
                    }
                    break;
                case State.DollarQuoted:
                    if (character is '$' && Matches(text, index, _dollarTag)) {
                        index += _dollarTag.Length;
                        Enter(index);
                    } else {
                        index++;
                    }
                    break;
                default:
                    if (Matches(text, index, lineComment)) {
                        Leave(State.LineComment, index);
                        index += lineComment.Length;
                    } else if (character is '/' && Follows(text, index + 1, '*')) {
                        Leave(State.BlockComment, index);
                        index += 2;
                    } else if (character is '\'' or '"' or '`') {
                        _quote = character;
                        Leave(State.Quoted, index);
                        index++;
                    } else if (character is '$' && TryReadDollarTag(text, index, out var tag)) {
                        _dollarTag = tag;
                        Leave(State.DollarQuoted, index);
                        index += tag.Length;
                    } else {
                        index++;
                    }
                    break;
            }
        }

        if (_state == State.LineComment) {
            Enter(text.Length); // the text ends the line, and with it the comment
        }
        if (codeStart >= 0 && text.Length > codeStart) {
            onCode?.Invoke(codeStart, text.Length - codeStart);
        }
    }

    /// Reads the opening tag of a postgres dollar quoted string ($$ or $tag$) at the given offset.
    private static bool TryReadDollarTag(string text, int index, out string tag) {
        tag = "";
        var end = index + 1;
        while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '_')) {
            // A tag is an identifier, so it cannot start with a digit. Anything else that follows a
            // '$' is something other than a dollar quote, such as a $1 parameter placeholder.
            if (char.IsDigit(text[end]) && end == index + 1) return false;
            end++;
        }
        if (end >= text.Length || text[end] != '$') return false;
        tag = text.Substring(index, end - index + 1);
        return true;
    }

    private static bool Follows(string text, int index, char character)
        => index < text.Length && text[index] == character;

    private static bool Matches(string text, int index, string token)
        => token.Length > 0 && index + token.Length <= text.Length
            && string.CompareOrdinal(text, index, token, 0, token.Length) == 0;
}
