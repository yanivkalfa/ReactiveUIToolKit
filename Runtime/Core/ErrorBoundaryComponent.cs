using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using ReactiveUITK.Core;

namespace ReactiveUITK
{
    public abstract class ErrorBoundaryComponent : ReactiveComponent
    {
        private bool errorCapturedFlag;
        private Exception capturedException;

        protected sealed override bool ShouldUpdate(Dictionary<string, object> nextProps)
        {
            return true;
        }

        protected override VirtualNode Render()
        {
            if (errorCapturedFlag)
            {
                return RenderError(capturedException);
            }
            try
            {
                return RenderSafe();
            }
            catch (Exception ex)
            {
                errorCapturedFlag = true;
                capturedException = ex;
                OnError(ex);
                return RenderError(ex);
            }
        }

        protected abstract VirtualNode RenderSafe();
        protected abstract VirtualNode RenderError(Exception ex);
        protected virtual void OnError(Exception ex)
        {
            Debug.LogError($"ReactiveUITK ErrorBoundary: {ex}");
        }

        public void ResetError()
        {
            errorCapturedFlag = false;
            capturedException = null;
            ForceUpdate();
        }
    }
}
