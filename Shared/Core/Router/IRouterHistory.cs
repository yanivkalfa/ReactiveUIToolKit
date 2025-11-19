using System;

namespace ReactiveUITK.Router
{
    public interface IRouterHistory
    {
        RouterLocation Location { get; }

        void Push(string path, object state = null);

        void Replace(string path, object state = null);

        IDisposable Listen(Action<RouterLocation> listener);
    }
}
