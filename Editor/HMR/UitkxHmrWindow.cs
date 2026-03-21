using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Editor window for UITKX Hot Module Replacement.
    /// Provides Start/Stop toggle, status readout, and settings.
    /// </summary>
    internal sealed class UitkxHmrWindow : EditorWindow
    {
        private UitkxHmrController _controller;
        private Vector2 _errorScroll;
        private int _recording; // 0=none, 1=toggle, 2=window

        [MenuItem("ReactiveUITK/HMR Mode", priority = 200)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<UitkxHmrWindow>("UITKX Hot Reload");
            wnd.minSize = new Vector2(300, 260);
        }

        public static void RepaintIfOpen()
        {
            if (HasOpenInstances<UitkxHmrWindow>())
                GetWindow<UitkxHmrWindow>().Repaint();
        }

        private void OnEnable()
        {
            _controller = UitkxHmrController.Instance;
        }

        private void OnDisable()
        {
            // Don't stop HMR when the window is closed — controller lives independently
        }

        private void OnDestroy()
        {
            // Auto-stop when the last window is destroyed and editor quits
            if (_controller != null && _controller.Active && !HasOpenInstances<UitkxHmrWindow>())
            {
                _controller.Stop();
            }
        }

        private void OnGUI()
        {
            bool isActive = _controller != null && _controller.Active;

            EditorGUILayout.Space(8);

            // ── Start / Stop button ──────────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (isActive)
                {
                    GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
                    if (
                        GUILayout.Button(
                            "  ■  Stop HMR  ",
                            GUILayout.Height(32),
                            GUILayout.MinWidth(160)
                        )
                    )
                    {
                        _controller.Stop();
                        _controller = null;
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                    if (
                        GUILayout.Button(
                            "  ●  Start HMR  ",
                            GUILayout.Height(32),
                            GUILayout.MinWidth(160)
                        )
                    )
                    {
                        _controller = new UitkxHmrController();
                        if (!_controller.Start(out string error))
                        {
                            EditorUtility.DisplayDialog(
                                "HMR Error",
                                "Failed to start HMR:\n\n" + error,
                                "OK"
                            );
                            _controller = null;
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(4);

            // ── Status ───────────────────────────────────────────────────────
            var statusStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
            };

            if (isActive)
            {
                statusStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
                EditorGUILayout.LabelField("● ACTIVE", statusStyle);
            }
            else
            {
                statusStyle.normal.textColor = Color.gray;
                EditorGUILayout.LabelField("Idle", statusStyle);
            }

            EditorGUILayout.Space(8);
            DrawSeparator();
            EditorGUILayout.Space(4);

            // ── Stats ────────────────────────────────────────────────────────
            if (isActive || (_controller != null && _controller.SwapCount > 0))
            {
                EditorGUILayout.LabelField("Watched", "Assets/**/*.uitkx");
                EditorGUILayout.LabelField("Swaps", _controller?.SwapCount.ToString() ?? "0");
                EditorGUILayout.LabelField("Errors", _controller?.ErrorCount.ToString() ?? "0");

                if (!string.IsNullOrEmpty(_controller?.LastComponentName))
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField(
                        "Last",
                        $"{_controller.LastComponentName} ({_controller.LastSwapMs:F0}ms)"
                    );

                    // Show timing breakdown on hover/always in a smaller font
                    if (!string.IsNullOrEmpty(_controller.LastTimingBreakdown))
                    {
                        var miniStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            wordWrap = true,
                            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) },
                        };
                        EditorGUILayout.LabelField(_controller.LastTimingBreakdown, miniStyle);
                    }
                }

                EditorGUILayout.Space(4);
                DrawSeparator();
                EditorGUILayout.Space(4);
            }

            // ── Settings ─────────────────────────────────────────────────────
            if (_controller != null)
            {
                bool autoStop = EditorGUILayout.Toggle(
                    "Auto-stop on Play Mode",
                    _controller.AutoStopOnPlayMode
                );
                if (autoStop != _controller.AutoStopOnPlayMode)
                    _controller.AutoStopOnPlayMode = autoStop;

                bool showNotify = EditorGUILayout.Toggle(
                    "Show swap notifications",
                    _controller.ShowNotifications
                );
                if (showNotify != _controller.ShowNotifications)
                    _controller.ShowNotifications = showNotify;
            }
            else
            {
                // Show settings from EditorPrefs even when controller is null
                bool autoStop = EditorPrefs.GetBool("UITKX_HMR_AutoStopPlay", true);
                EditorGUILayout.Toggle("Auto-stop on Play Mode", autoStop);
                bool showNotify = EditorPrefs.GetBool("UITKX_HMR_ShowNotify", true);
                EditorGUILayout.Toggle("Show swap notifications", showNotify);
            }

            // ── Shortcuts ────────────────────────────────────────────────────
            EditorGUILayout.Space(4);
            DrawKeybindRow("Toggle HMR", 1, UitkxHmrKeybinds.ToggleHmrKey);
            DrawKeybindRow("Open Window", 2, UitkxHmrKeybinds.ToggleWindowKey);

            // ── Warning ──────────────────────────────────────────────────────
            if (isActive)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "Assembly reload is locked.\n" + "Stop HMR to compile normally.",
                    MessageType.Warning
                );
            }

            // ── Recent errors ────────────────────────────────────────────────
            if (_controller != null && _controller.RecentErrors.Count > 0)
            {
                EditorGUILayout.Space(4);
                DrawSeparator();
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Recent Errors", EditorStyles.boldLabel);

                _errorScroll = EditorGUILayout.BeginScrollView(
                    _errorScroll,
                    GUILayout.MaxHeight(120)
                );
                foreach (var err in _controller.RecentErrors)
                {
                    EditorGUILayout.TextArea(err, EditorStyles.wordWrappedLabel);
                    EditorGUILayout.Space(2);
                }
                EditorGUILayout.EndScrollView();
            }

            // Force repaint while active (for live status updates)
            if (isActive)
                Repaint();
        }

        private static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        }

        private void DrawKeybindRow(string label, int id, KeyCombo current)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(90));

                if (_recording == id)
                {
                    // Recording mode — capture next key combo
                    GUI.backgroundColor = new Color(1f, 0.85f, 0.3f);
                    GUILayout.Button("Press keys...", EditorStyles.miniButton, GUILayout.Width(90));
                    GUI.backgroundColor = Color.white;

                    var e = Event.current;
                    if (e != null && e.type == EventType.KeyDown)
                    {
                        if (e.keyCode == KeyCode.Escape)
                        {
                            _recording = 0;
                            e.Use();
                        }
                        else if (e.keyCode != KeyCode.None
                            && (e.control || e.alt || e.shift)
                            && e.keyCode != KeyCode.LeftControl && e.keyCode != KeyCode.RightControl
                            && e.keyCode != KeyCode.LeftAlt && e.keyCode != KeyCode.RightAlt
                            && e.keyCode != KeyCode.LeftShift && e.keyCode != KeyCode.RightShift)
                        {
                            var combo = new KeyCombo(e.control, e.alt, e.shift, e.keyCode);
                            if (id == 1) UitkxHmrKeybinds.ToggleHmrKey = combo;
                            else UitkxHmrKeybinds.ToggleWindowKey = combo;
                            _recording = 0;
                            e.Use();
                        }
                    }
                    Repaint();
                }
                else
                {
                    if (GUILayout.Button(current.ToDisplay(), EditorStyles.miniButton, GUILayout.Width(90)))
                        _recording = id;

                    EditorGUI.BeginDisabledGroup(!current.IsValid);
                    if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        if (id == 1) UitkxHmrKeybinds.ToggleHmrKey = default;
                        else UitkxHmrKeybinds.ToggleWindowKey = default;
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
        }
    }
}
