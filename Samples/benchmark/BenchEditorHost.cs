#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.Shared;

namespace ReactiveUITK.Bench
{
    public class BenchEditorHost : EditorWindow, IVNodeHostRenderer
    {
        private VisualElement _mount;
        private VisualElement _hostVE;

        [MenuItem("Window/ReactiveUITK/Benchmark")]
        public static void Open()
        {
            var w = GetWindow<BenchEditorHost>();
            w.titleContent = new GUIContent("ReactiveUITK Bench");
            w.minSize = new Vector2(520, 360);
            w.Show();
        }

        private void OnEnable()
        {
            _mount = rootVisualElement;
            _mount.style.flexGrow = 1;

            // Header (status)
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = 22,
                    marginBottom = 4,
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.8f),
                    alignItems = Align.Center,
                    paddingLeft = 6
                }
            };
            header.Add(new Label("Bench: running  —  Space/→ to skip"));
            _mount.Add(header);

            // Content mount
            _hostVE = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.10f, 0.10f, 0.10f, 1f)
                },
                // MUST be focusable to receive keyboard events
                focusable = true,
                pickingMode = PickingMode.Position,
                tabIndex = 0
            };
            _mount.Add(_hostVE);

            // Keyboard handler (UI Toolkit)
            _hostVE.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            // Initialize benchmark harness (uses this window as the renderer)
            BenchSharedHost.Init(this, BenchOutputTarget.Editor);

            // Auto-wire SharedDemo hook via reflection (best effort)
            TrySetSharedDemoHook();

            // Drive the bench tick from the editor update loop
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (_hostVE != null)
                _hostVE.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            Unmount();
        }

        private void OnFocus()
        {
            // Ensure the content mount has focus so it receives KeyDownEvent
            _hostVE?.Focus();
        }

        private void OnInspectorUpdate()
        {
            // Extra repaint heartbeat in case nothing else is dirty
            Repaint();
        }

        private void OnEditorUpdate()
        {
            BenchSharedHost.Tick();
            Repaint();
        }

        // ===== Keyboard handler =====
        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Space || e.keyCode == KeyCode.RightArrow)
            {
                BenchSharedHost.SkipScenario();
                e.StopImmediatePropagation();
                Repaint();
            }
        }

        // ===== IVNodeHostRenderer via EditorRootRendererUtility =====
        public void Render(VirtualNode vnode)
        {
            if (_hostVE == null) return;
            EditorRootRendererUtility.Render(_hostVE, vnode);
        }

        public void Unmount()
        {
            if (_hostVE == null) return;
            EditorRootRendererUtility.Unmount(_hostVE);
        }

        private static void TrySetSharedDemoHook()
        {
            try
            {
                BenchSharedHost.SharedDemoRenderer = () => V.Func(SharedDemoPage.Render);
                Debug.Log("[BenchEditorHost] SharedDemo hook set.");
            }
            catch { /* best-effort */ }
        }
    }
}
#endif
