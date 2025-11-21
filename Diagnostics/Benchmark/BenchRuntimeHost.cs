using System;
using System.Reflection;
using ReactiveUITK.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Bench
{
    [RequireComponent(typeof(RootRenderer))]
    public class BenchRuntimeHost : MonoBehaviour, IVNodeHostRenderer
    {
        [SerializeField]
        private UIDocument uiDocument;
        private RootRenderer rootRenderer;

        private void Awake()
        {
            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null || uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[BenchRuntimeHost] Missing RootRenderer or UIDocument");
                enabled = false;
                return;
            }

            rootRenderer.Initialize(uiDocument.rootVisualElement);
            BenchSharedHost.Init(this, BenchOutputTarget.Runtime);
        }

        private void Update()
        {
            BenchSharedHost.Tick();

            if (ShouldSkip())
            {
                BenchSharedHost.SkipScenario();
            }
        }

        private static bool ShouldSkip()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

            return TryNewInputSystemPressed();

#elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER

            if (TryNewInputSystemPressed())
            {
                return true;
            }
            return LegacyPressed();

#else

            return LegacyPressed();
#endif
        }

        private static bool LegacyPressed()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                return true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }
            if (Input.touchCount > 0)
            {
                return true;
            }
            return false;
        }

        private static bool _checkedNewInput;
        private static bool _hasNewInput;

        private static Type _tKeyboard,
            _tMouse,
            _tTouchscreen;
        private static PropertyInfo _pKeyboardCurrent,
            _pMouseCurrent,
            _pTouchscreenCurrent;
        private static PropertyInfo _pKeyboardSpaceKey,
            _pMouseLeftButton,
            _pTouchscreenPrimaryTouch;
        private static PropertyInfo _pWasPressedThisFrame,
            _pTouchPress;

        private static bool TryNewInputSystemPressed()
        {
            if (!_checkedNewInput)
            {
                _checkedNewInput = true;

                _tKeyboard = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
                _tMouse = Type.GetType("UnityEngine.InputSystem.Mouse, Unity.InputSystem");
                _tTouchscreen = Type.GetType(
                    "UnityEngine.InputSystem.Touchscreen, Unity.InputSystem"
                );

                _hasNewInput = (_tKeyboard != null) || (_tMouse != null) || (_tTouchscreen != null);

                if (_hasNewInput)
                {
                    _pKeyboardCurrent = _tKeyboard?.GetProperty(
                        "current",
                        BindingFlags.Public | BindingFlags.Static
                    );
                    _pMouseCurrent = _tMouse?.GetProperty(
                        "current",
                        BindingFlags.Public | BindingFlags.Static
                    );
                    _pTouchscreenCurrent = _tTouchscreen?.GetProperty(
                        "current",
                        BindingFlags.Public | BindingFlags.Static
                    );

                    _pKeyboardSpaceKey = _tKeyboard?.GetProperty(
                        "spaceKey",
                        BindingFlags.Public | BindingFlags.Instance
                    );
                    _pMouseLeftButton = _tMouse?.GetProperty(
                        "leftButton",
                        BindingFlags.Public | BindingFlags.Instance
                    );
                    _pTouchscreenPrimaryTouch = _tTouchscreen?.GetProperty(
                        "primaryTouch",
                        BindingFlags.Public | BindingFlags.Instance
                    );

                    var tButtonControl = Type.GetType(
                        "UnityEngine.InputSystem.Controls.ButtonControl, Unity.InputSystem"
                    );
                    _pWasPressedThisFrame = tButtonControl?.GetProperty(
                        "wasPressedThisFrame",
                        BindingFlags.Public | BindingFlags.Instance
                    );

                    var tTouchControl = Type.GetType(
                        "UnityEngine.InputSystem.Controls.TouchControl, Unity.InputSystem"
                    );
                    _pTouchPress = tTouchControl?.GetProperty(
                        "press",
                        BindingFlags.Public | BindingFlags.Instance
                    );
                }
            }

            if (!_hasNewInput)
            {
                return false;
            }

            try
            {
                var kb = _pKeyboardCurrent?.GetValue(null);
                if (kb != null)
                {
                    var spaceKey = _pKeyboardSpaceKey?.GetValue(kb);
                    if (ButtonWasPressed(spaceKey))
                    {
                        return true;
                    }
                }

                var ms = _pMouseCurrent?.GetValue(null);
                if (ms != null)
                {
                    var left = _pMouseLeftButton?.GetValue(ms);
                    if (ButtonWasPressed(left))
                    {
                        return true;
                    }
                }

                var ts = _pTouchscreenCurrent?.GetValue(null);
                if (ts != null)
                {
                    var primary = _pTouchscreenPrimaryTouch?.GetValue(ts);
                    var press = _pTouchPress?.GetValue(primary);
                    if (ButtonWasPressed(press))
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool ButtonWasPressed(object buttonControl)
        {
            if (buttonControl == null || _pWasPressedThisFrame == null)
            {
                return false;
            }
            var val = _pWasPressedThisFrame.GetValue(buttonControl);
            return val is bool b && b;
        }

        public void Render(VirtualNode vnode) => rootRenderer.Render(vnode);

        public void Unmount() => rootRenderer.Unmount();
    }
}
