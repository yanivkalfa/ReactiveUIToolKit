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
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            props ??= new Dictionary<string, object>();
            props.TryGetValue("tracks", out var tracksObj);
            props.TryGetValue("autoplay", out var autoplayObj);
            props.TryGetValue("style", out var styleObj);

            var tracks = tracksObj as IReadOnlyList<AnimateTrack>;
            bool autoplay = autoplayObj is bool b ? b : true;

            
            
            
            var container = Hooks.UseRef();

            
            Hooks.UseAnimate(tracks, autoplay, tracks);
            if (container != null)
            {
                container.style.flexGrow = 0f;
                container.style.flexShrink = 0f;
            }

            
            var style = styleObj as Style;
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
