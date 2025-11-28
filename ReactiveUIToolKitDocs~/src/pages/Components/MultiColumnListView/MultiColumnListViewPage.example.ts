export const MULTI_COLUMN_LIST_VIEW_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

public static class MultiColumnListViewExamples
{
  private sealed class Row
  {
    public string Name;
    public int Value;
  }

  // Function component – pass MultiColumnListViewExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var (rows, setRows) = Hooks.UseState(new List<Row>
    {
      new Row { Name = "One", Value = 1 },
      new Row { Name = "Two", Value = 2 },
      new Row { Name = "Three", Value = 3 },
    });

    var columns = new List<MultiColumnListViewColumn>
    {
      new MultiColumnListViewColumn
      {
        Name = "Name",
        Width = 160f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Name }),
      },
      new MultiColumnListViewColumn
      {
        Name = "Value",
        Width = 80f,
        Cell = (item, index) => V.Label(new LabelProps { Text = item.Value.ToString() }),
      },
    };

    return V.MultiColumnListView(
      new MultiColumnListViewProps
      {
        Items = rows,
        Columns = columns,
      }
    );
  }
}`

