#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.PureUIToolkit.Editor
{
    public sealed class PureUIToolkitStressTestWindow : EditorWindow
    {
        [MenuItem("ReactiveUITK/Demos/Stress Test (Pure UI Toolkit)")]
        public static void ShowWindow()
        {
            var window = GetWindow<PureUIToolkitStressTestWindow>("Stress Test (Pure UI Toolkit)");
            window.minSize = new Vector2(640, 480);
            window.Show();
        }

        struct Box
        {
            public float x,
                y,
                size,
                vx,
                vy;
            public Color color;
            public VisualElement element;
        }

        private readonly List<Box> _boxes = new();
        private int _boxCount = 300;
        private float _duration = 10f;
        private float _lastTime = -1f;
        private int _totalFrames;
        private float _totalTime;
        private float _elapsed;
        private bool _running;
        private bool _finished;
        private float _finalAvgFps;

        private VisualElement _area;
        private Label _statsLabel;
        private TextField _countInput;
        private TextField _durationInput;
        private Button _actionBtn;
        private IVisualElementScheduledItem _ticker;

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexGrow = 1f;
            root.style.flexDirection = FlexDirection.Column;
            root.style.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);

            // ── Header bar ──
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.paddingTop = 8;
            header.style.paddingBottom = 8;
            header.style.paddingLeft = 8;
            header.style.paddingRight = 8;
            header.style.flexShrink = 0;
            header.style.alignItems = Align.Center;

            _statsLabel = new Label("Pure UI Toolkit Stress Test — Ready");
            _statsLabel.style.color = Color.white;
            _statsLabel.style.fontSize = 16;
            _statsLabel.style.flexGrow = 1f;
            header.Add(_statsLabel);

            var durationLabel = new Label("Duration(s):");
            durationLabel.style.color = Color.white;
            durationLabel.style.marginLeft = 12;
            header.Add(durationLabel);

            _durationInput = new TextField();
            _durationInput.value = "10";
            _durationInput.style.width = 50;
            _durationInput.style.marginLeft = 4;
            header.Add(_durationInput);

            var countLabel = new Label("Boxes:");
            countLabel.style.color = Color.white;
            countLabel.style.marginLeft = 12;
            header.Add(countLabel);

            _countInput = new TextField();
            _countInput.value = "300";
            _countInput.style.width = 80;
            _countInput.style.marginLeft = 4;
            header.Add(_countInput);

            _actionBtn = new Button(OnActionClicked) { text = "Start" };
            _actionBtn.style.marginLeft = 8;
            _actionBtn.style.height = 24;
            _actionBtn.style.width = 70;
            header.Add(_actionBtn);

            root.Add(header);

            // ── Box area ──
            _area = new VisualElement();
            _area.style.flexGrow = 1f;
            _area.style.position = Position.Relative;
            root.Add(_area);
        }

        private void OnActionClicked()
        {
            // If running, ignore clicks
            if (_running)
                return;

            // If finished, reset to ready state
            if (_finished)
            {
                foreach (var b in _boxes)
                    _area.Remove(b.element);
                _boxes.Clear();
                _ticker?.Pause();
                _ticker = null;
                _finished = false;
                _totalFrames = 0;
                _totalTime = 0f;
                _elapsed = 0f;
                _lastTime = -1f;
                _finalAvgFps = 0f;
                _actionBtn.text = "Start";
                _statsLabel.text = "Pure UI Toolkit Stress Test — Ready";
                _countInput.SetEnabled(true);
                _durationInput.SetEnabled(true);
                return;
            }

            // Start
            if (!int.TryParse(_countInput.value, out int n) || n <= 0 || n > 10000)
                return;
            if (!float.TryParse(_durationInput.value, out float dur) || dur <= 0)
                return;

            _boxCount = n;
            _duration = dur;

            // Clear previous
            foreach (var b in _boxes)
                _area.Remove(b.element);
            _boxes.Clear();
            _ticker?.Pause();
            _ticker = null;

            _running = false;
            _finished = false;
            _totalFrames = 0;
            _totalTime = 0f;
            _elapsed = 0f;
            _lastTime = -1f;
            _finalAvgFps = 0f;

            // Generate boxes
            var rng = new System.Random(42);
            for (int i = 0; i < _boxCount; i++)
            {
                float size = 8f + (float)(rng.NextDouble() * 16.0);
                float hue = (float)rng.NextDouble();
                Color color = Color.HSVToRGB(hue, 0.7f, 0.9f);
                float x = (float)(rng.NextDouble() * 600.0);
                float y = (float)(rng.NextDouble() * 400.0);
                float vx =
                    (60f + (float)(rng.NextDouble() * 120.0)) * (rng.NextDouble() > 0.5 ? 1f : -1f);
                float vy =
                    (60f + (float)(rng.NextDouble() * 120.0)) * (rng.NextDouble() > 0.5 ? 1f : -1f);

                var el = new VisualElement();
                el.style.position = Position.Absolute;
                el.style.left = x;
                el.style.top = y;
                el.style.width = size;
                el.style.height = size;
                el.style.backgroundColor = color;
                el.style.borderTopLeftRadius = size * 0.15f;
                el.style.borderTopRightRadius = size * 0.15f;
                el.style.borderBottomLeftRadius = size * 0.15f;
                el.style.borderBottomRightRadius = size * 0.15f;
                _area.Add(el);

                _boxes.Add(
                    new Box
                    {
                        x = x,
                        y = y,
                        size = size,
                        vx = vx,
                        vy = vy,
                        color = color,
                        element = el,
                    }
                );
            }

            // Start
            _running = true;
            _actionBtn.text = "Running...";
            _countInput.SetEnabled(false);
            _durationInput.SetEnabled(false);
            _ticker = _area.schedule.Execute(Tick).Every(16);
        }

        private void Tick()
        {
            if (!_running)
                return;

            float now = Time.realtimeSinceStartup;
            if (_lastTime < 0)
            {
                _lastTime = now;
                return;
            }
            float dt = now - _lastTime;
            _lastTime = now;

            _totalFrames++;
            _totalTime += dt;
            _elapsed += dt;

            float avgFps = _totalTime > 0 ? _totalFrames / _totalTime : 0f;

            // Check if duration expired
            if (_elapsed >= _duration)
            {
                _running = false;
                _finished = true;
                _finalAvgFps = avgFps;
                _ticker?.Pause();
                _ticker = null;
                _actionBtn.text = "Restart";
                _countInput.SetEnabled(false);
                _durationInput.SetEnabled(false);
                _statsLabel.text =
                    $"DONE — {_boxes.Count} boxes | Avg FPS: {_finalAvgFps:F1} | Duration: {_elapsed:F1}s | Frames: {_totalFrames}";
                return;
            }

            float w = _area.resolvedStyle.width;
            float h = _area.resolvedStyle.height;
            if (w <= 0 || h <= 0)
                return;

            for (int i = 0; i < _boxes.Count; i++)
            {
                var b = _boxes[i];
                float nx = b.x + b.vx * dt;
                float ny = b.y + b.vy * dt;
                float nvx = b.vx;
                float nvy = b.vy;

                if (nx < 0)
                {
                    nx = 0;
                    nvx = -nvx;
                }
                else if (nx + b.size > w)
                {
                    nx = w - b.size;
                    nvx = -nvx;
                }
                if (ny < 0)
                {
                    ny = 0;
                    nvy = -nvy;
                }
                else if (ny + b.size > h)
                {
                    ny = h - b.size;
                    nvy = -nvy;
                }

                b.x = nx;
                b.y = ny;
                b.vx = nvx;
                b.vy = nvy;

                b.element.style.left = nx;
                b.element.style.top = ny;

                _boxes[i] = b;
            }

            _statsLabel.text =
                $"Pure UI Toolkit — {_boxes.Count} boxes | Avg FPS: {avgFps:F1} | Elapsed: {_elapsed:F1}s / {_duration:F0}s | Frames: {_totalFrames}";
        }

        private void OnDisable()
        {
            _ticker?.Pause();
            _ticker = null;
        }
    }
}
#endif
