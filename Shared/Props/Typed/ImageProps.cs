using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ImageProps : BaseProps
    {
        public Texture2D Texture { get; set; }
        public Sprite Sprite { get; set; }
        public string ScaleMode { get; set; }

        public override Dictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            if (Texture != null)
            {
                dict["texture"] = Texture;
            }
            if (Sprite != null)
            {
                dict["sprite"] = Sprite;
            }
            if (!string.IsNullOrEmpty(ScaleMode))
            {
                dict["scaleMode"] = ScaleMode;
            }
            return dict;
        }
    }
}
