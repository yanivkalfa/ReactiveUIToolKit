using System;

namespace ReactiveUITK.Router
{
    public sealed class RouterState
    {
        public RouterState(
            RouterLocation location,
            RouterNavigateHandler navigate,
            RouterNavigateHandler replace,
            RouterGoHandler go,
            RouterCanGoHandler canGo,
            Func<Func<RouterLocation, RouterLocation, bool>, IDisposable> registerBlocker
        )
        {
            Location = location ?? RouterPath.Parse("/");
            Navigate = navigate;
            Replace = replace;
            Go = go;
            CanGo = canGo;
            RegisterBlocker = registerBlocker;
        }

        public RouterLocation Location { get; }

        public RouterNavigateHandler Navigate { get; }

        public RouterNavigateHandler Replace { get; }

        public RouterGoHandler Go { get; }

        public RouterCanGoHandler CanGo { get; }

        public Func<
            Func<RouterLocation, RouterLocation, bool>,
            IDisposable
        > RegisterBlocker { get; }
    }

    public delegate bool RouterNavigateHandler(string path, object state = null);

    public delegate bool RouterGoHandler(int delta);

    public delegate bool RouterCanGoHandler(int delta);
}
