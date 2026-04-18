#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Samples.UITKXComponents;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Bench
{
    public class BenchEditorHost : EditorWindow, IVNodeHostRenderer
    {
        private VisualElement _mount;
        private VisualElement _hostVE;
        private VisualElement _setupPanel;
        private IntegerField _runCountField;

        private int _totalRuns = 1;
        private int _currentRun;

        [MenuItem("ReactiveUITK/Diagnostics/Benchmark/Run Tests")]
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

            ShowSetupUI();
        }

        private void ShowSetupUI()
        {
            _mount.Clear();
            _hostVE = null;

            _setupPanel = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.11f, 0.11f, 0.11f, 1f),
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    paddingLeft = 40,
                    paddingRight = 40,
                },
            };

            var title = new Label("ReactiveUITK Benchmark")
            {
                style =
                {
                    fontSize = 18,
                    color = new Color(0.85f, 0.85f, 0.85f),
                    marginBottom = 20,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };
            _setupPanel.Add(title);

            var scenarioInfo = new Label(
                $"{BenchConfig.Default.Count} scenarios  •  "
                    + $"~{TotalDurationSec():F0}s per run"
            )
            {
                style =
                {
                    fontSize = 11,
                    color = new Color(0.55f, 0.55f, 0.55f),
                    marginBottom = 16,
                    unityTextAlign = TextAnchor.MiddleCenter,
                },
            };
            _setupPanel.Add(scenarioInfo);

            var row = new VisualElement
            {
                style =
                {
                    flexDirection = UnityEngine.UIElements.FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 20,
                },
            };

            var label = new Label("Run count")
            {
                style =
                {
                    fontSize = 13,
                    color = new Color(0.75f, 0.75f, 0.75f),
                    marginRight = 8,
                    width = 80,
                },
            };
            row.Add(label);

            _runCountField = new IntegerField { value = _totalRuns };
            _runCountField.style.width = 60;
            _runCountField.RegisterValueChangedCallback(e =>
            {
                _totalRuns = Mathf.Max(1, e.newValue);
            });
            row.Add(_runCountField);

            _setupPanel.Add(row);

            var startBtn = new Button(OnStartClicked)
            {
                text = "Start",
                style =
                {
                    width = 140,
                    height = 32,
                    fontSize = 14,
                },
            };
            _setupPanel.Add(startBtn);

            _mount.Add(_setupPanel);
        }

        private void OnStartClicked()
        {
            _totalRuns = Mathf.Max(1, _runCountField.value);
            _currentRun = 0;
            StartNextRun();
        }

        private void StartNextRun()
        {
            _currentRun++;

            _mount.Clear();

            var header = new VisualElement
            {
                style =
                {
                    flexDirection = UnityEngine.UIElements.FlexDirection.Row,
                    height = 22,
                    marginBottom = 4,
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.8f),
                    alignItems = Align.Center,
                    paddingLeft = 6,
                },
            };
            var runLabel = _totalRuns > 1
                ? $"Bench: run {_currentRun}/{_totalRuns}  —  Space/→ to skip"
                : "Bench: running  —  Space/→ to skip";
            header.Add(new Label(runLabel));
            _mount.Add(header);

            _hostVE = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.10f, 0.10f, 0.10f, 1f),
                },
                focusable = true,
                pickingMode = PickingMode.Position,
                tabIndex = 0,
            };
            _mount.Add(_hostVE);

            _hostVE.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            TrySetSharedDemoHook();

            BenchSharedHost.OnAllScenariosComplete += OnRunComplete;
            BenchSharedHost.Init(this, BenchOutputTarget.Editor);

            EditorApplication.update += OnEditorUpdate;

            _hostVE.schedule.Execute(() => _hostVE.Focus());
        }

        private void OnRunComplete()
        {
            EditorApplication.update -= OnEditorUpdate;
            BenchSharedHost.OnAllScenariosComplete -= OnRunComplete;

            if (_hostVE != null)
            {
                _hostVE.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            }
            Unmount();

            Debug.Log($"[Bench] Run {_currentRun}/{_totalRuns} finished.");

            if (_currentRun < _totalRuns)
            {
                StartNextRun();
            }
            else
            {
                RenderCompletionScreen();
                Repaint();
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            BenchSharedHost.OnAllScenariosComplete -= OnRunComplete;
            if (_hostVE != null)
            {
                _hostVE.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            }
            Unmount();
        }

        private void OnFocus()
        {
            _hostVE?.Focus();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnEditorUpdate()
        {
            BenchSharedHost.Tick();
            Repaint();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Space || e.keyCode == KeyCode.RightArrow)
            {
                BenchSharedHost.SkipScenario();
                e.StopImmediatePropagation();
                Repaint();
            }
        }

        public void Render(VirtualNode vnode)
        {
            if (_hostVE == null)
            {
                return;
            }
            EditorRootRendererUtility.Render(_hostVE, vnode);
        }

        public void Unmount()
        {
            if (_hostVE == null)
            {
                return;
            }
            EditorRootRendererUtility.Unmount(_hostVE);
        }

        private static void TrySetSharedDemoHook()
        {
            try
            {
                BenchSharedHost.SharedDemoRenderer = () => V.Func(ShowcaseDemoPage.Render);
            }
            catch { }
        }

        private static float TotalDurationSec()
        {
            float t = 0;
            for (int i = 0; i < BenchConfig.Default.Count; i++)
            {
                t += BenchConfig.Default[i].DurationSec;
            }
            return t;
        }

        private void RenderCompletionScreen()
        {
            _mount.Clear();
            _hostVE = null;

            var colStyle = new Style
            {
                (StyleKeys.FlexDirection, "column"),
                (FlexGrow, 1f),
                (PaddingLeft, 24f),
                (PaddingRight, 24f),
                (PaddingTop, 24f),
                (PaddingBottom, 24f),
                (BackgroundColor, new UColor(0.11f, 0.11f, 0.11f, 1f)),
                (AlignItems, "center"),
            };

            var headerText = _totalRuns > 1
                ? $"All {_totalRuns} benchmark runs complete"
                : "All benchmark scenarios complete";

            var headerStyle = new Style
            {
                (StyleKeys.Color, new UColor(0.3f, 0.85f, 0.4f, 1f)),
                (FontSize, 20f),
                (MarginBottom, 16f),
            };

            var subStyle = new Style
            {
                (StyleKeys.Color, new UColor(0.7f, 0.7f, 0.7f, 1f)),
                (FontSize, 12f),
                (MarginBottom, 4f),
            };

            var children = new System.Collections.Generic.List<VirtualNode>
            {
                V.Label(
                    new LabelProps
                    {
                        Text = headerText,
                        Style = headerStyle,
                    }
                ),
            };

            for (int i = 0; i < BenchConfig.Default.Count; i++)
            {
                var def = BenchConfig.Default[i];
                children.Add(
                    V.Label(
                        new LabelProps
                        {
                            Text = $"  {def.Name}  ({def.DurationSec:F0}s)",
                            Style = subStyle,
                        }
                    )
                );
            }

            var hintText = _totalRuns > 1
                ? $"Results written to {_totalRuns} separate run folders.\n"
                    + "Open  ReactiveUITK > Diagnostics > Benchmark > Results Viewer  to compare runs."
                : "Open  ReactiveUITK > Diagnostics > Benchmark > Results Viewer  to compare runs.";

            children.Add(
                V.Label(
                    new LabelProps
                    {
                        Text = hintText,
                        Style = new Style
                        {
                            (StyleKeys.Color, new UColor(0.55f, 0.55f, 0.55f, 1f)),
                            (FontSize, 11f),
                            (MarginTop, 20f),
                        },
                    }
                )
            );

            // Build host for completion rendering
            _hostVE = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.10f, 0.10f, 0.10f, 1f),
                },
            };
            _mount.Add(_hostVE);

            var vnode = V.VisualElement(
                new VisualElementProps { Style = colStyle },
                null,
                children.ToArray()
            );

            Render(vnode);
        }
    }
}
#endif
