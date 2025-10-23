using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Props.Typed;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;
using ReactiveUITK.Examples.ClassComponents;
using System.Collections.Generic; // added for RowItem
using System; // added for Func<>


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
            //ReactiveUITK.Core.Reconciler.EnableDiffTracing = true;
            //ReactiveUITK.Core.Reconciler.TraceLevel = ReactiveUITK.Core.Reconciler.DiffTraceLevel.Verbose;
            EditorRootRendererUtility.Mount(host, V.Func(EditorAppFunc.Render));
        }

        private void OnDisable()
        {
            EditorRootRendererUtility.Unmount(rootVisualElement);
        }
    }

    internal static class EditorAppFunc
    {
        // Stable row item with immutable Id separate from display text
        private sealed class RowItem
        {
            public string Id; // immutable identity used for VNode key
            public string Text; // mutable display value
            public override string ToString() => $"{Text}({Id.Substring(0,6)})";
        }

        private static string FormatItems(List<RowItem> items)
        {
            if (items == null) return "<null-items>";
            var sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var it = items[i];
                sb.Append(i).Append(":").Append(it?.Text).Append("#").Append(it?.Id.Substring(0,6));
            }
            sb.Append("]");
            return sb.ToString();
        }

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
                if (root == null) { return null; }
                var itemTimer = root.schedule.Execute(() => setNow(System.DateTime.Now)).Every(1000);
                return () => { try { itemTimer?.Pause(); } catch { } };
            }, System.Array.Empty<object>());

            var initialItems = Hooks.UseMemo(() =>
            {
                var list = new List<RowItem>();
                for (int i = 1; i <= 5; i++) list.Add(new RowItem { Id = System.Guid.NewGuid().ToString("N"), Text = $"Item {i}" });
                return list;
            });
            var (items, setItems) = Hooks.UseState(initialItems);

            var rowFn = Hooks.UseMemo(() => (Func<int, object, VirtualNode>)((i, itemObj) =>
            {
                Debug.Log("[RowFn] rendering index=" + i + " itemObj=" + itemObj);
                var rowItem = itemObj as RowItem;
                string display = rowItem?.Text ?? "<null>";
                string key = rowItem != null ? ($"{rowItem.Id}-{i}") : $"row-missing-{i}";
                return V.VisualElement(new Style { (StyleKeys.FlexDirection, "row"), (AlignItems, "center") }, key: key,
                    V.Text(display),
                    V.Button(new ButtonProps
                    {
                        Text = " X ",
                        OnClick = () =>
                        {
                            if (rowItem == null) return;
                            var copy = new List<RowItem>(items);
                            int idx = copy.FindIndex(r => r.Id == rowItem.Id);
                            if (idx >= 0)
                            {
                                copy.RemoveAt(idx);
                                setItems(copy);
                            }
                        },
                        Style = new Style { (MarginLeft, 8f), (Width, 24f), (Height, 18f) }
                    })
                );
            }), items);

            ListViewProps listViewProps = new()
            {
                Items = items,
                FixedItemHeight = 20f,
                Selection = UnityEngine.UIElements.SelectionType.None,
                Row = rowFn
            };

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

            ButtonProps changeFirstProps = new()
            {
                Text = "Change First Item",
                OnClick = () =>
                {
                    if (items.Count > 0)
                    {
                        var copy = new List<RowItem>(items);
                        copy[0] = new RowItem { Id = copy[0].Id, Text = "UPDATED " + System.DateTime.Now.ToLongTimeString() };
                        Debug.Log("[ChangeFirst] newItems=" + FormatItems(copy));
                        setItems(copy);
                    }
                },
                Style = new Style { (MarginTop, 8f), (Width, 160f), (Height, 28f) }
            };

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
