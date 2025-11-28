export const TREE_VIEW_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections;
using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public sealed class TreeItem
{
  public string Label;
  public int Id;
}

public static class TreeViewExamples
{
  private static readonly Style TreeViewStyle = new Style { (StyleKeys.FlexGrow, 1f) };

  // Function component – pass TreeViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var items = new List<TreeItem>
    {
      new TreeItem { Id = 1, Label = "Root 1" },
      new TreeItem { Id = 2, Label = "Root 2" },
    };

    VirtualNode Row(int index, object obj)
    {
      var item = obj as TreeItem;
      return V.Label(
        new LabelProps { Text = item?.Label ?? "<null>" },
        key: $"tree-{item?.Id ?? index}"
      );
    }

    var propsTree = new TreeViewProps
    {
      RootItems = items,
      FixedItemHeight = 20f,
      Selection = SelectionType.Single,
      Row = Row,
      Style = TreeViewStyle,
    };

    return V.TreeView(propsTree);
  }
}`
