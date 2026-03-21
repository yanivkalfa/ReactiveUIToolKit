using OmniSharp.Extensions.LanguageServer.Protocol;

namespace UitkxLanguageServer;

/// <summary>
/// Thread-safe document text cache. Updated by <see cref="TextSyncHandler"/>.
/// </summary>
public sealed class DocumentStore
{
    private readonly Dictionary<string, string> _docs = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public void Set(DocumentUri uri, string text)
    {
        lock (_lock)
            _docs[uri.ToString()] = text;
    }

    public void Remove(DocumentUri uri)
    {
        lock (_lock)
            _docs.Remove(uri.ToString());
    }

    public bool TryGet(DocumentUri uri, out string text)
    {
        lock (_lock)
            return _docs.TryGetValue(uri.ToString(), out text!);
    }

    /// <summary>Returns a snapshot of all currently open documents as (uriString, text) pairs.</summary>
    public IReadOnlyList<(string UriString, string Text)> GetAll()
    {
        lock (_lock)
        {
            var list = new List<(string, string)>(_docs.Count);
            foreach (var kvp in _docs)
                list.Add((kvp.Key, kvp.Value));
            return list;
        }
    }
}
