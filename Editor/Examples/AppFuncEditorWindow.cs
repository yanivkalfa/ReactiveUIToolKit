using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;
using ReactiveUITK.Examples.ClassComponents;

namespace ReactiveUITK.EditorExamples
{
    public sealed class AppFuncEditorWindow : EditorWindow
    {
        [MenuItem("Window/ReactiveUITK/AppFunc Demo")] 
        public static void ShowWindow()
        {
            AppFuncEditorWindow window = GetWindow<AppFuncEditorWindow>("ReactiveUITK AppFunc");
            window.minSize = new UnityEngine.Vector2(420, 320);
            window.Show();
        }

        private void CreateGUI()
        {
            var host = rootVisualElement;
            host.style.flexGrow = 1f;
            ReactiveUITK.Core.Reconciler.EnableDiffTracing = true;
            ReactiveUITK.Core.Reconciler.TraceLevel = ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose;
            EditorRootRendererUtility.Mount(host, V.Func(EditorAppFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }

    internal static class EditorAppFunc
    {
        private static readonly Style TopBarStyle = new()
        {
            (BackgroundColor, new UColor(1f,1f,1f,1f)),
            (StyleKeys.FlexDirection, "row"),
            (JustifyContent, "space-between"),
            (AlignItems, "center"),
            (FlexGrow, 1f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BorderBottomWidth, 1f),
            (BorderBottomColor, new Color(0.85f,0.85f,0.85f,1f))
        };

        private static readonly Style LeftBoxStyle = new()
        {
            (BackgroundColor, new Color(0.2f,0.4f,0.9f,1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f)
        };

        private static readonly Style RightBoxStyle = new()
        {
            (BackgroundColor, new UColor(0.9f,0.3f,0.2f,1f)),
            (TextColor, UColor.white),
            (PaddingLeft, 10f),
            (PaddingRight, 10f),
            (PaddingTop, 6f),
            (PaddingBottom, 6f),
            (BorderRadius, 4f),
            (FontSize, 14f)
        };

        private static readonly Style TextInputStyle = new()
        {
            (FlexGrow, 1f),
            (MarginLeft, 8f),
            (MarginRight, 8f),
            (PaddingLeft, 6f),
            (PaddingRight, 6f),
            (PaddingTop, 4f),
            (PaddingBottom, 4f),
            (BorderRadius, 4f),
            (BorderWidth, 1f),
            (TextColor, UColor.black),
            (BorderColor, new UColor(0.8f,0.8f,0.8f,1f)),
            (BackgroundColor, new UColor(1f,1f,1f,1f))
        };

        private static readonly Style PageStyle = new()
        {
            (StyleKeys.FlexDirection, "column"),
            (FlexGrow, 1f),
            (JustifyContent, "space-between"),
            (BackgroundColor, new UColor(0.95f,0.95f,0.95f,1f))
        };

        private static readonly Style ListContainerStyle = new()
        {
            (FlexGrow, 1f),
            (MarginTop, 8f)
        };

        private static readonly Style ExtrasContainerStyle = new()
        {
            (MarginTop, 12f),
            (PaddingLeft, 12f),
            (PaddingRight, 12f),
            (PaddingTop, 8f),
            (PaddingBottom, 8f),
            (BackgroundColor, UColor.white),
            (BorderTopWidth, 1f),
            (BorderTopColor, new UColor(0.85f,0.85f,0.85f,1f)),
            (StyleKeys.FlexDirection, "column")
        };

        public static VirtualNode Render(System.Collections.Generic.Dictionary<string, object> props, System.Collections.Generic.IReadOnlyList<VirtualNode> children)
        {
            var (showList, setShowList) = Hooks.UseState(true);
            var (textValue, setTextValue) = Hooks.UseState("");
            var (toggleValue, setToggleValue) = Hooks.UseState(false);
            var (radioChecked, setRadioChecked) = Hooks.UseState(false);
            var radioChoices = Hooks.UseMemo(() => new System.Collections.Generic.List<string>{"One","Two","Three"}, 0);
            var (radioIndex, setRadioIndex) = Hooks.UseState(0);
            var (repeatClicks, setRepeatClicks) = Hooks.UseState(0);
            var (now, setNow) = Hooks.UseState(System.DateTime.Now);
            var root = Hooks.UseRef();
            Hooks.UseEffect(() =>
            {
                // Editor timer: throttle via EditorApplication.update to 1Hz
                double last = -1;
                void Tick()
                {
                    double nowSec = UnityEditor.EditorApplication.timeSinceStartup;
                    double whole = System.Math.Floor(nowSec);
                    if (whole != last)
                    {
                        last = whole;
                        setNow(System.DateTime.Now);
                    }
                }
                UnityEditor.EditorApplication.update += Tick;
                return () => { UnityEditor.EditorApplication.update -= Tick; };
            }, System.Array.Empty<object>());

            ButtonProps toggleButtonProps = new()
            {
                Text = showList ? "Hide List" : "Show List",
                OnClick = () => setShowList(!showList),
                Style = new Style { (MarginTop, 8f), (Width, 120f), (Height, 28f) }
            };

            TextFieldProps textFieldProps = new()
            {
                Style = TextInputStyle,
                Placeholder = "Type here...",
                HidePlaceholderOnFocus = false,
                Value = textValue,
                LabelText = string.IsNullOrEmpty(textValue) ? "" : ("Value: " + textValue),
                OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<string>>)(e => setTextValue(e.newValue))
            };

            // Items state
            var initialItems = Hooks.UseMemo(() =>
            {
                var list = new System.Collections.Generic.List<string>();
                for (int i = 1; i <= 20; i++) list.Add($"Item {i}");
                return list;
            });
            var (items, setItems) = Hooks.UseState(initialItems);

            ListViewProps listViewProps = new()
            {
                Items = items,
                FixedItemHeight = 20f,
                Row = (i, item) =>
                {
                    string text = item?.ToString() ?? "<null>";
                    string id = text;
                    return V.VisualElement(new Style { (StyleKeys.FlexDirection, "row"), (AlignItems, "center") }, key: $"row-{id}",
                        V.Text(text),
                        V.Button(new ButtonProps
                        {
                            Text = " X ",
                            OnClick = () =>
                            {
                                var copy = new System.Collections.Generic.List<string>(items);
                                int idx = copy.IndexOf(id);
                                if (idx >= 0) { copy.RemoveAt(idx); setItems(copy); }
                            },
                            Style = new Style { (MarginLeft, 8f), (Width, 24f), (Height, 18f) }
                        })
                    );
                }
            };

            ButtonProps changeFirstProps = new()
            {
                Text = "Change First Item",
                OnClick = () =>
                {
                    var copy = new System.Collections.Generic.List<string>(items);
                    if (copy.Count > 0)
                    {
                        copy[0] = "UPDATED " + System.DateTime.Now.ToLongTimeString();
                        setItems(copy);
                    }
                },
                Style = new Style { (MarginTop, 8f), (Width, 160f), (Height, 28f) }
            };

            Debug.Log($"blaaaa, {repeatClicks}");

            var conditionalList = showList
                ? V.VisualElement(new System.Collections.Generic.Dictionary<string, object> { { "style", ListContainerStyle } }, null, V.ListView(listViewProps))
                : V.Text("List hidden");

            return V.VisualElement(new System.Collections.Generic.Dictionary<string, object> { { "style", PageStyle } }, key: "page-root",
                V.VisualElement(new System.Collections.Generic.Dictionary<string, object> { { "style", TopBarStyle } }, null,
                    V.VisualElement(new System.Collections.Generic.Dictionary<string, object> { { "style", LeftBoxStyle } }, null, V.Text("Left")),
                    V.TextField(textFieldProps),
                    V.VisualElement(new System.Collections.Generic.Dictionary<string, object> { { "style", RightBoxStyle } }, null, V.Text("Right"))
                ),
                V.Label(new LabelProps { Text = "Now: " + now.ToLongTimeString() }),
                V.Button(toggleButtonProps),
                V.Button(changeFirstProps),
                conditionalList,
                V.VisualElement(new System.Collections.Generic.Dictionary<string, object> { { "style", ExtrasContainerStyle } }, key: "extras",
                    V.Label(new LabelProps { Text = "Extras" }),
                    V.GroupBox(new GroupBoxProps { Text = "GroupBox", ContentContainer = new System.Collections.Generic.Dictionary<string, object> { { "style", new Style { (PaddingLeft, 6f), (PaddingTop, 4f) } } } }, null,
                        V.Label(new LabelProps { Text = "Inside group" }, key: "inner-one")
                    ),
                    V.Toggle(new ToggleProps
                    {
                        Text = "Enable option",
                        Value = toggleValue,
                        OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<bool>>)(e => setToggleValue(e.newValue))
                    }),
                    V.RadioButton(new RadioButtonProps
                    {
                        Text = "Single radio",
                        Value = radioChecked,
                        OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<bool>>)(e => setRadioChecked(e.newValue))
                    }),
                    V.RadioButtonGroup(new RadioButtonGroupProps
                    {
                        Choices = radioChoices,
                        Index = radioIndex,
                        OnChange = (System.Action<UnityEngine.UIElements.ChangeEvent<int>>)(e => setRadioIndex(e.newValue))
                    }, null,
                        V.Label(new LabelProps { Text = "Pick one" }, key: "radio-label")
                    ),
                    V.ProgressBar(new ProgressBarProps { Value = repeatClicks % 100, Title = "Progress" }),
                    V.RepeatButton(new RepeatButtonProps { Text = $"Repeat ({repeatClicks})", OnClick = () => setRepeatClicks(repeatClicks + 1) })
                )
                ,
                V.Component<BottomBarComponent>(new System.Collections.Generic.Dictionary<string, object>
                {
                    { "inputValue", textValue },
                    { "setTextValue", (System.Action<string>)setTextValue }
                })
            );
        }
    }
}
