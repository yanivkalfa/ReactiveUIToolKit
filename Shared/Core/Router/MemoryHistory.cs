using System;
using System.Collections.Generic;

namespace ReactiveUITK.Router
{
    public sealed class MemoryHistory : IRouterHistory
    {
        private readonly List<RouterLocation> entries = new List<RouterLocation>();
        private readonly List<Action<RouterLocation>> listeners = new List<Action<RouterLocation>>();
        private int currentIndex = -1;

        public MemoryHistory(string initialPath = "/")
        {
            Push(initialPath);
        }

        public RouterLocation Location
        {
            get
            {
                if (currentIndex < 0 || currentIndex >= entries.Count)
                {
                    return RouterPath.Parse("/");
                }
                return entries[currentIndex];
            }
        }

        public void Push(string path, object state = null)
        {
            var location = RouterPath.Parse(path, state);
            if (currentIndex < entries.Count - 1)
            {
                entries.RemoveRange(currentIndex + 1, entries.Count - currentIndex - 1);
            }
            entries.Add(location);
            currentIndex = entries.Count - 1;
            Notify(location);
        }

        public void Replace(string path, object state = null)
        {
            var location = RouterPath.Parse(path, state);
            if (currentIndex < 0)
            {
                entries.Add(location);
                currentIndex = 0;
            }
            else
            {
                entries[currentIndex] = location;
            }
            Notify(location);
        }

        public IDisposable Listen(Action<RouterLocation> listener)
        {
            if (listener == null)
            {
                return Disposable.Empty;
            }
            listeners.Add(listener);
            listener(Location);
            return new Subscription(listeners, listener);
        }

        private void Notify(RouterLocation location)
        {
            foreach (var listener in listeners.ToArray())
            {
                listener?.Invoke(location);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly List<Action<RouterLocation>> listeners;
            private Action<RouterLocation> listener;

            public Subscription(
                List<Action<RouterLocation>> listeners,
                Action<RouterLocation> listener
            )
            {
                this.listeners = listeners;
                this.listener = listener;
            }

            public void Dispose()
            {
                if (listener == null)
                {
                    return;
                }
                listeners.Remove(listener);
                listener = null;
            }
        }

        private sealed class Disposable : IDisposable
        {
            public static readonly IDisposable Empty = new Disposable();

            public void Dispose() { }
        }
    }
}
