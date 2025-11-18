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

            // Ensure this lightweight wrapper does not grab layout space unexpectedly.
            // Function component containers default to flexGrow=1; for inline wrappers this can collapse siblings.
            // Acquire element ref BEFORE animation hook to preserve historical hook ordering (UseRefElement precedes UseAnimate).
            var container = Hooks.UseRef();

            // Kick the animations on this wrapper container (UseAnimate will record its own hook + effect internally)
            Hooks.UseAnimate(tracks, autoplay, tracks);
            if (container != null)
            {
                container.style.flexGrow = 0f;
                container.style.flexShrink = 0f;
            }

            // Wrap children in a container so the animation applies to that element
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
