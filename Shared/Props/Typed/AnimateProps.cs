using System.Collections.Generic;
using ReactiveUITK.Core.Animation;

namespace ReactiveUITK.Props.Typed
{
    public sealed class AnimateProps : BaseProps
    {
        public List<AnimateTrack> Tracks { get; set; }
        public bool Autoplay { get; set; } = true;
    }
}
