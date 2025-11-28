export const HELP_BOX_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public static class HelpBoxExamples
{
  // Function component – pass HelpBoxExamples.Example to V.Func(...)
  public static VirtualNode Example(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    return V.HelpBox(
      new HelpBoxProps
      {
        Text = "Something went wrong.",
        MessageType = "Error",
      }
    );
  }
}`

