using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using ReactiveUITK.Core;

namespace ReactiveUITK
{
    public abstract class ErrorBoundaryComponent : ReactiveComponent
    {
        private bool hasError;
        private Exception captured;

        protected sealed override bool ShouldUpdate(Dictionary<string, object> nextProps)
        {
            return true; // always allow update; boundary controls rendering
        }

        protected override VirtualNode Render()
        {
            if (hasError)
            {
                return RenderError(captured);
            }
            try
            {
                return RenderSafe();
            }
            catch (Exception ex)
            {
                hasError = true;
                captured = ex;
                OnError(ex);
                return RenderError(ex);
            }
        }

        protected abstract VirtualNode RenderSafe();
        protected abstract VirtualNode RenderError(Exception ex);
        protected virtual void OnError(Exception ex) { Debug.LogError($"ReactiveUITK ErrorBoundary: {ex}"); }

        public void ResetError()
        {
            hasError = false;
            captured = null;
            ForceUpdate();
        }
    }
}
