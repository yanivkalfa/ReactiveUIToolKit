using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;
using static ReactiveUITK.Props.Typed.StyleKeys;

namespace ReactiveUITK.Samples.Shared
{
    public static class SharedDemoPage
    {
        public static VirtualNode Render(IProps rawProps, IReadOnlyList<VirtualNode> children)
        {
            // -- State ------------------------------------------------------------------
            var (inputText, setInputText) = Hooks.UseState(string.Empty);
            var (isOptionEnabled, setOptionEnabled) = Hooks.UseState(false);
            var (isRadioSingleSelected, setRadioSingleSelected) = Hooks.UseState(false);
            var selectionChoices = Hooks.UseMemo(() => new List<string> { "One", "Two", "Three" }, 0);
            var (selectionIndex, setSelectionIndex) = Hooks.UseState(0);
            var (repeatClickCount, setRepeatClickCount) = Hooks.UseState(0);
            var (currentTime, setCurrentTime) = Hooks.UseState(DateTime.Now);
            var (sliderValue, setSliderValue) = Hooks.UseState(0.5f);
            var (sliderIntValue, setSliderIntValue) = Hooks.UseState(5);
            var ddChoices = Hooks.UseMemo(() => new List<string> { "Alpha", "Beta", "Gamma" }, 0);
            var (ddValue, setDdValue) = Hooks.UseState("Beta");
            var (foldoutOpen, setFoldoutOpen) = Hooks.UseState(true);
            var (batchClicks, setBatchClicks) = Hooks.UseState(0);

            // Sub-section feedback: counts + tab indices for ValuesBar
            var (treeTabIndex, setTreeTabIndex) = Hooks.UseState(0);
            var (listTabIndex, setListTabIndex) = Hooks.UseState(0);
            var (treeDisplayCount, setTreeDisplayCount) = Hooks.UseState(0);
            var (mctvDisplayCount, setMctvDisplayCount) = Hooks.UseState(0);
            var (simpleListCount, setSimpleListCount) = Hooks.UseState(0);
            var (mclvDisplayCount, setMclvDisplayCount) = Hooks.UseState(0);

            var rootElement = Hooks.UseRef();

            // -- Timer -------------------------------------------------------------------
            Hooks.UseEffect(
                () =>
                {
                    if (rootElement == null) return null;
                    var h = rootElement.schedule.Execute(() => setCurrentTime.Set(DateTime.Now)).Every(1000);
                    return () => { try { h?.Pause(); } catch { } };
                },
                Array.Empty<object>()
            );

            // -- Batch handler -----------------------------------------------------------
            Action batchOnClick = () =>
            {
                setBatchClicks(batchClicks + 1);
                setRepeatClickCount.Set(v => v + 1);
                setRepeatClickCount.Set(v => v + 1);
                setSliderValue.Set(v => Mathf.Clamp01(v + 0.05f));
                setSliderIntValue.Set(v => (v + 1) % 11);
                setOptionEnabled(!isOptionEnabled);
            };

            // -- Values items for ValuesBar ----------------------------------------------
            int totalListCount = treeDisplayCount + mctvDisplayCount + simpleListCount + mclvDisplayCount;
            var valuesItems = new List<KeyValuePair<string, string>>
            {
                new("TextField", inputText ?? string.Empty),
                new("Toggle", isOptionEnabled.ToString()),
                new("RadioSingle", isRadioSingleSelected.ToString()),
                new("RadioIndex", selectionIndex.ToString()),
                new("Slider", sliderValue.ToString("F2")),
                new("SliderInt", sliderIntValue.ToString()),
                new("Dropdown", ddValue ?? string.Empty),
                new("Repeat", repeatClickCount.ToString()),
                new("Time", currentTime.ToLongTimeString()),
                new("TreeTab", treeTabIndex.ToString()),
                new("ListTab", listTabIndex.ToString()),
                new("ListCount", totalListCount.ToString()),
            };

            // -- Render ------------------------------------------------------------------
            return V.VisualElement(
                new VisualElementProps { Style = SharedDemoPageStyles.OuterWrapperStyle },
                null,
                V.VisualElementSafe(
                    SharedDemoPageStyles.SafeWrapperStyle,
                    null,
                    V.VisualElement(
                        new VisualElementProps { Style = SharedDemoPageStyles.BarSlotStyle },
                        null,
                        V.Func<ValuesBarFunc.Props>(
                            ValuesBarFunc.Render,
                            new ValuesBarFunc.Props { Items = valuesItems }
                        )
                    ),
                    V.ScrollView(
                        new ScrollViewProps
                        {
                            Mode = "vertical",
                            Style = SharedDemoPageStyles.MainScrollStyle,
                            Name = "main-scroll",
                        },
                        null,
                        V.VisualElement(
                            new VisualElementProps { Style = SharedDemoPageStyles.PageStyle },
                            null,
                            V.Func<ShowcaseTopBar.Props>(
                                ShowcaseTopBar.Render,
                                new ShowcaseTopBar.Props
                                {
                                    InputText = inputText,
                                    OnInputChange = e => setInputText(e.newValue),
                                    OnSetText = () => setInputText.Set("Updated!"),
                                    CurrentTime = currentTime,
                                }
                            ),
                            V.Func<ShowcaseExtrasPanel.Props>(
                                ShowcaseExtrasPanel.Render,
                                new ShowcaseExtrasPanel.Props
                                {
                                    IsOptionEnabled = isOptionEnabled,
                                    OnOptionEnabledChange = e => setOptionEnabled(e.newValue),
                                    IsRadioSingleSelected = isRadioSingleSelected,
                                    OnRadioSingleChange = e => setRadioSingleSelected(e.newValue),
                                    SelectionChoices = selectionChoices,
                                    SelectionIndex = selectionIndex,
                                    OnSelectionChange = e => setSelectionIndex(e.newValue),
                                    ProgressValue = repeatClickCount % 100,
                                    BatchClicks = batchClicks,
                                    OnBatchClick = batchOnClick,
                                }
                            ),
                            V.Func<ShowcaseNewComponentsPanel.Props>(
                                ShowcaseNewComponentsPanel.Render,
                                new ShowcaseNewComponentsPanel.Props
                                {
                                    FoldoutOpen = foldoutOpen,
                                    OnFoldoutChange = e => setFoldoutOpen(e.newValue),
                                    SliderValue = sliderValue,
                                    OnSliderChange = e => setSliderValue(e.newValue),
                                    SliderIntValue = sliderIntValue,
                                    OnSliderIntChange = e => setSliderIntValue.Set(e.newValue),
                                    DdChoices = ddChoices,
                                    DdValue = ddValue,
                                    OnDdChange = e => setDdValue(e.newValue),
                                }
                            ),
                            V.Func<ShowcaseTreeTabsSection.Props>(
                                ShowcaseTreeTabsSection.Render,
                                new ShowcaseTreeTabsSection.Props
                                {
                                    OnTreeCountChanged = count => { if (count != treeDisplayCount) setTreeDisplayCount(count); },
                                    OnMctvCountChanged = count => { if (count != mctvDisplayCount) setMctvDisplayCount(count); },
                                    OnTreeTabIndexChanged = idx => { if (idx != treeTabIndex) setTreeTabIndex(idx); },
                                }
                            ),
                            V.Func<ShowcaseListTabsSection.Props>(
                                ShowcaseListTabsSection.Render,
                                new ShowcaseListTabsSection.Props
                                {
                                    OnListCountChanged = count => { if (count != simpleListCount) setSimpleListCount(count); },
                                    OnMclvCountChanged = count => { if (count != mclvDisplayCount) setMclvDisplayCount(count); },
                                    OnListTabIndexChanged = idx => { if (idx != listTabIndex) setListTabIndex(idx); },
                                }
                            ),
                            V.Func(ShowcaseFieldsPanel.Render)
                        )
                    )
                )
            );
        }
    }
}
