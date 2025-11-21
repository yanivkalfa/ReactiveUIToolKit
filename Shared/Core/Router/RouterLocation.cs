using System.Collections.Generic;

namespace ReactiveUITK.Router
{
    public sealed class RouterLocation
    {
        public RouterLocation(string path, IReadOnlyDictionary<string, string> query, object state)
        {
            Path = path;
            Query = query ?? RouterContextKeys.EmptyParams;
            State = state;
        }

        public string Path { get; }

        public IReadOnlyDictionary<string, string> Query { get; }

        public object State { get; }

        public override string ToString()
        {
            return Path;
        }
    }
}
