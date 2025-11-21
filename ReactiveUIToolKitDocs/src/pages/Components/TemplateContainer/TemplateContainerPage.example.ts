export const TEMPLATE_CONTAINER_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TemplateContainerExamples
{
  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentStyle = new Style
    {
      (StyleKeys.PaddingTop, 4f),
      (StyleKeys.PaddingBottom, 4f),
    };

    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", contentStyle },
    };

    return V.TemplateContainer(
      new TemplateContainerProps
      {
        ContentContainer = contentContainerProps,
      },
      children
    );
  }
}`

