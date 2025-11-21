export const TEMPLATE_CONTAINER_BASIC = `// Example namespace: ReactiveUITK.Samples.Components

using System.Collections.Generic;
using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

public static class TemplateContainerExamples
{
  private static readonly Style ContentStyle = new Style
  {
    (StyleKeys.PaddingTop, 4f),
    (StyleKeys.PaddingBottom, 4f),
  };

  public static VirtualNode Render(
    Dictionary<string, object> props,
    IReadOnlyList<VirtualNode> children
  )
  {
    var contentContainerProps = new Dictionary<string, object>
    {
      { "style", ContentStyle },
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
