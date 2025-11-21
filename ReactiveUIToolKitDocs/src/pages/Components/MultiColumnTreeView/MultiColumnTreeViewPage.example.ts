export const MULTI_COLUMN_TREE_VIEW_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class MultiColumnTreeViewExamples
{
  private sealed class Node
  {
    public string Name;
    public int Depth;
    public IList<Node> Children;
  }

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var root = new Node
    {
      Name = "Root",
      Depth = 0,
      Children = new List<Node>
      {
        new Node { Name = "Child A", Depth = 1 },
        new Node { Name = "Child B", Depth = 1 },
      },
    };

    var nodes = new List<Node> { root };

    var columns = new List<MultiColumnTreeViewColumn>
    {
      new MultiColumnTreeViewColumn
      {
        Name = "Name",
        Width = 200f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Name }),
      },
      new MultiColumnTreeViewColumn
      {
        Name = "Depth",
        Width = 80f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Depth.ToString() }),
      },
    };

    return V.MultiColumnTreeView(
      new MultiColumnTreeViewProps
      {
        Items = nodes,
        Columns = columns,
      }
    );
  }
}`

