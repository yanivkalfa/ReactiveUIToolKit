using System;
using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Core.Animation;
using ReactiveUITK.Props.Typed;

namespace ReactiveUITK.Core.AnimationComponents
{
    public static class AnimateFunc
    {
        public static VirtualNode Render(
            IProps rawProps,
            IReadOnlyList<VirtualNode> children
        )
        {
            var p = rawProps as AnimateProps;
            var tracks = p?.Tracks;
            bool autoplay = p?.Autoplay ?? true;

            var container = Hooks.UseRef();

            Hooks.UseAnimate(tracks, autoplay, tracks);
            if (container != null)
            {
                container.style.flexGrow = 0f;
                container.style.flexShrink = 0f;
            }

            var style = p?.Style;
            var arr = children is VirtualNode[] a
                ? a
                : (
                    children != null
                        ? new List<VirtualNode>(children).ToArray()
                        : Array.Empty<VirtualNode>()
                );
            return ReactiveUITK.V.VisualElement(style, null, arr);
        }
    }
}
