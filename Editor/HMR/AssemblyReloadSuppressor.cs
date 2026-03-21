using System;
using UnityEditor;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Manages the LockReloadAssemblies / DisallowAutoRefresh lifecycle.
    /// Both APIs are counter-based and must be balanced.
    /// </summary>
    internal sealed class AssemblyReloadSuppressor : IDisposable
    {
        private bool _active;

        public bool IsActive => _active;

        public void Lock()
        {
            if (_active)
                return;
            _active = true;
            EditorApplication.LockReloadAssemblies();
            AssetDatabase.DisallowAutoRefresh();
        }

        public void Unlock()
        {
            if (!_active)
                return;
            _active = false;
            AssetDatabase.AllowAutoRefresh();
            EditorApplication.UnlockReloadAssemblies();
            // Defer the refresh so the unlock fully propagates first
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            };
        }

        public void Dispose() => Unlock();
    }
}
