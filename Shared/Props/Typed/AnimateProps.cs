using System.Collections.Generic;
using ReactiveUITK.Core.Animation;
using UnityEngine;

namespace ReactiveUITK.Props.Typed
{
    public sealed class AnimateProps : global::ReactiveUITK.Core.IProps
    {
        public List<AnimateTrack> Tracks { get; set; }
        public bool Autoplay { get; set; } = true;
        public Style Style { get; set; }
        public object Ref { get; set; }
    }
}
