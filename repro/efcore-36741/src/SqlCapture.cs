using System.Text.RegularExpressions;

namespace EfCore36741Repro;

/// <summary>
/// Captures SQL command text emitted via <c>DbContextOptionsBuilder.LogTo</c>
/// (wired up by each <see cref="ProviderFixture"/> against the
/// <c>DbLoggerCategory.Database.Command</c> category at <c>LogLevel.Information</c>),
/// grouped under caller-chosen labels via <see cref="Scope"/>.
///
/// This is how AC#10 is satisfied: a test wraps the inline run in
/// <c>Scope("inline")</c> and the compiled run in <c>Scope("compiled")</c>,
/// then compares <see cref="NormalizedFor"/> for both labels.
/// </summary>
public sealed class SqlCapture
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<string>> _statementsByLabel = new();
    private string? _activeLabel;

    /// <summary>
    /// The sink to pass to <c>DbContextOptionsBuilder.LogTo(sql.Log, ...)</c>.
    /// Appends the raw log message to whatever label is currently active (if any).
    /// </summary>
    public void Log(string message)
    {
        lock (_gate)
        {
            if (_activeLabel is not { } label)
            {
                return;
            }

            if (!_statementsByLabel.TryGetValue(label, out var list))
            {
                list = new List<string>();
                _statementsByLabel[label] = list;
            }

            list.Add(message);
        }
    }

    /// <summary>
    /// Marks every SQL log message emitted for the duration of the returned
    /// scope as belonging to <paramref name="label"/>. Not reentrant/nested --
    /// callers should scope tightly around the single operation under test.
    /// </summary>
    public IDisposable Scope(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            throw new ArgumentException("Label must be non-empty.", nameof(label));
        }

        lock (_gate)
        {
            _activeLabel = label;
        }

        return new ActiveScope(this);
    }

    private void EndScope()
    {
        lock (_gate)
        {
            _activeLabel = null;
        }
    }

    /// <summary>Raw (verbatim) log messages captured under <paramref name="label"/>, in emission order.</summary>
    public IReadOnlyList<string> StatementsFor(string label)
    {
        lock (_gate)
        {
            return _statementsByLabel.TryGetValue(label, out var list)
                ? list.ToArray()
                : Array.Empty<string>();
        }
    }

    /// <summary>Verbatim captured text for <paramref name="label"/>, for inclusion in the report.</summary>
    public string RawFor(string label) => string.Join(Environment.NewLine + Environment.NewLine, StatementsFor(label));

    /// <summary>
    /// SQL for <paramref name="label"/> with the EF log preamble (the
    /// "Executed DbCommand (...) [Parameters=...]" line) stripped, whitespace
    /// collapsed, and parameter names replaced by positional @p0..@pN in
    /// order of first appearance -- so an inline and a compiled run of the
    /// same query, which may assign different EF-generated parameter names,
    /// compare equal.
    /// </summary>
    public string NormalizedFor(string label) =>
        string.Join(Environment.NewLine, StatementsFor(label).Select(Normalize));

    /// <summary>Discards all captured statements and clears the active scope, if any.</summary>
    public void Reset()
    {
        lock (_gate)
        {
            _statementsByLabel.Clear();
            _activeLabel = null;
        }
    }

    private static readonly Regex ParameterToken = new(@"@\w+", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRun = new(@"\s+", RegexOptions.Compiled);

    private static string Normalize(string rawMessage)
    {
        // Strip the single-line "Executed DbCommand (Xms) [Parameters=[...], CommandType='Text', CommandTimeout='30']"
        // preamble that EF's command logger always emits before the SQL text.
        var newlineIndex = rawMessage.IndexOf('\n');
        var body = newlineIndex >= 0 ? rawMessage[(newlineIndex + 1)..] : rawMessage;

        body = WhitespaceRun.Replace(body.Trim(), " ");

        var renumbered = new Dictionary<string, string>();
        body = ParameterToken.Replace(body, match =>
        {
            var token = match.Value;
            if (!renumbered.TryGetValue(token, out var replacement))
            {
                replacement = $"@p{renumbered.Count}";
                renumbered[token] = replacement;
            }

            return replacement;
        });

        return body;
    }

    private sealed class ActiveScope : IDisposable
    {
        private readonly SqlCapture _owner;
        private bool _disposed;

        public ActiveScope(SqlCapture owner) => _owner = owner;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner.EndScope();
        }
    }
}
