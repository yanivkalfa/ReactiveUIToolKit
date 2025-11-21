export const TOOLBAR_BASIC = `// Example namespace: ReactiveUITK.Samples.Editor

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class ToolbarExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.Toolbar(
      new ToolbarProps
      {
        Style = new Style { (StyleKeys.FlexDirection, "row"), (StyleKeys.Gap, 4f) },
      },
      key: null,
      V.ToolbarButton(new ToolbarButtonProps { Text = "Action" }),
      V.ToolbarToggle(new ToolbarToggleProps { Text = "Toggle", Value = true }),
      V.ToolbarSpacer(new ToolbarSpacerProps()),
      V.ToolbarSearchField(new ToolbarSearchFieldProps { Value = "", }),
      V.ToolbarMenu(new ToolbarMenuProps { Text = "Menu" })
    );
  }
}`

