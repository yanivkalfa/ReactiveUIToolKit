using System;
using ReactiveUITK.Core;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ErrorBoundaryProps
    {
        public VirtualNode Fallback { get; set; }
        public Action<Exception> OnError { get; set; }
        public string ResetKey { get; set; }
    }
}
