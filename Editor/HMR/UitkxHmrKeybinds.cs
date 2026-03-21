using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ReactiveUITK.EditorSupport.HMR
{
    /// <summary>
    /// Global keyboard shortcuts for HMR actions.
    /// Hooks into EditorApplication's internal global event handler via reflection.
    /// Keybinds are stored in EditorPrefs and configured from the HMR window.
    /// </summary>
    [InitializeOnLoad]
    internal static class UitkxHmrKeybinds
    {
        private const string PrefToggle = "UITKX_HMR_Key_Toggle";
        private const string PrefWindow = "UITKX_HMR_Key_Window";

        public static KeyCombo ToggleHmrKey
        {
            get => KeyCombo.Parse(EditorPrefs.GetString(PrefToggle, ""));
            set => EditorPrefs.SetString(PrefToggle, value.Serialize());
        }

        public static KeyCombo ToggleWindowKey
        {
            get => KeyCombo.Parse(EditorPrefs.GetString(PrefWindow, ""));
            set => EditorPrefs.SetString(PrefWindow, value.Serialize());
        }

        static UitkxHmrKeybinds()
        {
            // Hook into Unity's internal global event handler via reflection.
            // This field exists in all Unity versions and fires for every GUI event
            // in every editor window — the standard way to implement global shortcuts
            // without [MenuItem].
            var fi = typeof(EditorApplication).GetField(
                "globalEventHandler",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (fi != null)
            {
                var existing = (EditorApplication.CallbackFunction)fi.GetValue(null);
                existing += OnGlobalEvent;
                fi.SetValue(null, existing);
            }
        }

        private static void OnGlobalEvent()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown || e.keyCode == KeyCode.None)
                return;

            var toggle = ToggleHmrKey;
            if (toggle.IsValid && toggle.Matches(e))
            {
                e.Use();
                if (UitkxHmrController.IsActive)
                {
                    UitkxHmrController.Instance?.Stop();
                    UitkxHmrWindow.RepaintIfOpen();
                }
                else
                {
                    var controller = new UitkxHmrController();
                    if (!controller.Start(out string error))
                        Debug.LogWarning($"[HMR] Failed to start: {error}");
                    else
                        UitkxHmrWindow.RepaintIfOpen();
                }
                return;
            }

            var window = ToggleWindowKey;
            if (window.IsValid && window.Matches(e))
            {
                e.Use();
                if (EditorWindow.HasOpenInstances<UitkxHmrWindow>())
                    EditorWindow.GetWindow<UitkxHmrWindow>().Close();
                else
                    UitkxHmrWindow.ShowWindow();
            }
        }
    }

    internal struct KeyCombo
    {
        public bool Ctrl;
        public bool Alt;
        public bool Shift;
        public KeyCode Key;

        public bool IsValid => Key != KeyCode.None;

        public KeyCombo(bool ctrl, bool alt, bool shift, KeyCode key)
        {
            Ctrl = ctrl; Alt = alt; Shift = shift; Key = key;
        }

        public bool Matches(Event e) =>
            IsValid && e.keyCode == Key && e.control == Ctrl && e.alt == Alt && e.shift == Shift;

        public string Serialize()
        {
            if (!IsValid) return "";
            string s = "";
            if (Ctrl) s += "Ctrl+";
            if (Alt) s += "Alt+";
            if (Shift) s += "Shift+";
            return s + Key;
        }

        public static KeyCombo Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return default;
            var c = new KeyCombo();
            foreach (var p in s.Split('+'))
            {
                var t = p.Trim();
                if (t.Equals("Ctrl", StringComparison.OrdinalIgnoreCase)) c.Ctrl = true;
                else if (t.Equals("Alt", StringComparison.OrdinalIgnoreCase)) c.Alt = true;
                else if (t.Equals("Shift", StringComparison.OrdinalIgnoreCase)) c.Shift = true;
                else if (Enum.TryParse<KeyCode>(t, true, out var kc)) c.Key = kc;
            }
            return c;
        }

        public string ToDisplay()
        {
            if (!IsValid) return "None";
            string s = "";
            if (Ctrl) s += "Ctrl+";
            if (Alt) s += "Alt+";
            if (Shift) s += "Shift+";
            string k = Key.ToString();
            return s + (k.Length == 1 ? k.ToUpper() : k);
        }
    }
}
