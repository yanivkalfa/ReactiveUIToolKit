export const LIST_VIEW_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;

public static class ListViewExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    IList items = new[] { "One", "Two", "Three" };

    VirtualNode Row(int index, object item)
    {
      return V.Label(
        new LabelProps { Text = $"{index}: {item}" },
        key: $"row-{index}"
      );
    }

    var scrollViewProps = new Dictionary<string, object>
    {
      { "style", new Style { (StyleKeys.MaxHeight, 200f) } },
    };

    var listProps = new ListViewProps
    {
      Items = items,
      FixedItemHeight = 20f,
      Row = Row,
      Selection = SelectionType.None,
      ScrollView = scrollViewProps,
      Style = new Style { (StyleKeys.FlexGrow, 1f) },
    };

    return V.ListView(listProps);
  }
}`

