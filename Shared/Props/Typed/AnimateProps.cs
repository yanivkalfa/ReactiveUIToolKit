using System.Collections.Generic;
using ReactiveUITK.Core.Animation;
using UnityEngine;

namespace ReactiveUITK.Props.Typed
{
    public sealed class AnimateProps
    {
        public List<AnimateTrack> Tracks { get; set; }
        public bool Autoplay { get; set; } = true;
        public Style Style { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (Tracks != null) dict["tracks"] = Tracks;
            dict["autoplay"] = Autoplay;
            if (Style != null) dict["style"] = Style;
            return dict;
        }
    }
}

