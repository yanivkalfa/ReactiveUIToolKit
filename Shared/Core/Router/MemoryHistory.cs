using System;
using System.Collections.Generic;

namespace ReactiveUITK.Router
{
    public sealed class MemoryHistory : IRouterHistory
    {
        private readonly List<RouterLocation> entries = new List<RouterLocation>();
        private readonly List<Action<RouterLocation>> listeners =
            new List<Action<RouterLocation>>();
        private readonly List<Func<RouterLocation, RouterLocation, bool>> blockers =
            new List<Func<RouterLocation, RouterLocation, bool>>();
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

        public int EntryCount => entries.Count;

        public int Index => currentIndex;

        public bool CanGo(int delta)
        {
            if (delta == 0 || entries.Count == 0)
            {
                return false;
            }
            int target = currentIndex + delta;
            return target >= 0 && target < entries.Count;
        }

        public void Go(int delta)
        {
            if (!CanGo(delta))
            {
                return;
            }
            int target = currentIndex + delta;
            RouterLocation next = entries[target];
            RouterLocation previous = Location;
            if (!AllowTransition(previous, next))
            {
                return;
            }
            currentIndex = target;
            Notify(Location);
        }

        public void Push(string path, object state = null)
        {
            var location = RouterPath.Parse(path, state);
            var previous = Location;
            if (!AllowTransition(previous, location))
            {
                return;
            }
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
            var previous = Location;
            if (!AllowTransition(previous, location))
            {
                return;
            }
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

        public IDisposable RegisterBlocker(Func<RouterLocation, RouterLocation, bool> blocker)
        {
            if (blocker == null)
            {
                return Disposable.Empty;
            }
            blockers.Add(blocker);
            return new BlockerSubscription(blockers, blocker);
        }

        private bool AllowTransition(RouterLocation from, RouterLocation to)
        {
            foreach (var blocker in blockers.ToArray())
            {
                try
                {
                    if (blocker != null && blocker(from, to) == false)
                    {
                        return false;
                    }
                }
                catch
                {
                    // Swallow blocker exceptions but keep navigation.
                }
            }
            return true;
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

        private sealed class BlockerSubscription : IDisposable
        {
            private readonly List<Func<RouterLocation, RouterLocation, bool>> blockers;
            private Func<RouterLocation, RouterLocation, bool> blocker;

            public BlockerSubscription(
                List<Func<RouterLocation, RouterLocation, bool>> blockers,
                Func<RouterLocation, RouterLocation, bool> blocker
            )
            {
                this.blockers = blockers;
                this.blocker = blocker;
            }

            public void Dispose()
            {
                if (blocker == null)
                {
                    return;
                }
                blockers.Remove(blocker);
                blocker = null;
            }
        }

        private sealed class Disposable : IDisposable
        {
            public static readonly IDisposable Empty = new Disposable();

            public void Dispose() { }
        }
    }
}
