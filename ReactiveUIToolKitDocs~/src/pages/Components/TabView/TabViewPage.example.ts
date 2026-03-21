export const TAB_VIEW_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class TabViewExamples
{
  // Function component – pass TabViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (index, setIndex) = Hooks.UseState(0);

    var tabs = new List<TabViewProps.TabDef>
    {
      new TabViewProps.TabDef
      {
        Title = "Tab A",
        StaticContent = V.Label(new LabelProps { Text = "Content A" }),
      },
      new TabViewProps.TabDef
      {
        Title = "Tab B",
        StaticContent = V.Label(new LabelProps { Text = "Content B" }),
      },
    };

    return V.TabView(
      new TabViewProps
      {
        SelectedIndex = index,
        Tabs = tabs,
        SelectedIndexChanged = setIndex,
      }
    );
  }
}`

