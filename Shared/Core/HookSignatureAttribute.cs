using System;

namespace ReactiveUITK
{
    /// <summary>
    /// Records the ordered hook call signature (e.g. "UseState,UseEffect,UseMemo")
    /// emitted by both the Source Generator and HMR emitter.
    /// Used during HMR delegate swap to detect hook order/count changes and
    /// proactively reset component state instead of relying on runtime exceptions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class HookSignatureAttribute : Attribute
    {
        public string Signature { get; }

        public HookSignatureAttribute(string signature)
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }
    }
}
