using System;

namespace ReactiveUITK
{
    /// <summary>
    /// Records the ordered hook call signature (e.g. "UseState,UseEffect,UseMemo")
    /// emitted by both the Source Generator and HMR emitter.
    /// Used during HMR delegate swap to detect hook order/count changes and
    /// proactively reset component state instead of relying on runtime exceptions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HookSignatureAttribute : Attribute
    {
        public string Signature { get; }

        /// <summary>
        /// Family IDs of every custom hook this component (or this hook)
        /// calls transitively at first level. Empty for hooks/components
        /// that only call built-in hooks. The reconciler walks this list
        /// during HMR equality checks (<c>RefreshRuntime.HaveEqualSignatures</c>)
        /// to detect transitive signature changes — analogous to React Fast
        /// Refresh's <c>signature.getCustomHooks()</c>.
        /// </summary>
        public string[] CustomHookFamilyKeys { get; }

        public HookSignatureAttribute(string signature)
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
            CustomHookFamilyKeys = Array.Empty<string>();
        }

        public HookSignatureAttribute(string signature, string[] customHookFamilyKeys)
        {
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
            CustomHookFamilyKeys = customHookFamilyKeys ?? Array.Empty<string>();
        }
    }
}
