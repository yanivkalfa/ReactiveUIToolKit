using System;

namespace ReactiveUITK.Router
{
    public sealed class RouterState
    {
        public RouterState(
            RouterLocation location,
            RouterNavigateHandler navigate,
            RouterNavigateHandler replace
        )
        {
            Location = location ?? RouterPath.Parse("/");
            Navigate = navigate;
            Replace = replace;
        }

        public RouterLocation Location { get; }

        public RouterNavigateHandler Navigate { get; }

        public RouterNavigateHandler Replace { get; }
    }

    public delegate bool RouterNavigateHandler(string path, object state = null);
}
