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
            Func<Func<RouterLocation, RouterLocation, bool>, IDisposable> registerBlocker,
            string basename = null
        )
        {
            Location = location ?? RouterPath.Parse("/");
            Navigate = navigate;
            Replace = replace;
            Go = go;
            CanGo = canGo;
            RegisterBlocker = registerBlocker;
            Basename = string.IsNullOrEmpty(basename) ? "/" : RouterPath.Normalize(basename);
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

        /// <summary>
        /// Optional URL prefix the router treats as the root of the
        /// application (defaults to <c>"/"</c>).  All locations are exposed
        /// to consumers stripped of the basename; navigation calls re-attach
        /// it transparently.
        /// </summary>
        public string Basename { get; }
    }

    public delegate bool RouterNavigateHandler(string path, object state = null);

    public delegate bool RouterGoHandler(int delta);

    public delegate bool RouterCanGoHandler(int delta);
}
