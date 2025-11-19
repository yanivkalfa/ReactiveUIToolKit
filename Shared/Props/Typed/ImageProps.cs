using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ImageProps
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public Texture2D Texture { get; set; }
        public Sprite Sprite { get; set; }
        public string ScaleMode { get; set; } 
        public Style Style { get; set; }
        public object Ref { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(Name))
            {
                dict["name"] = Name;
            }
            if (!string.IsNullOrEmpty(ClassName))
            {
                dict["className"] = ClassName;
            }
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
            if (Style != null)
            {
                dict["style"] = Style;
            }
            if (Ref != null)
            {
                dict["ref"] = Ref;
            }
            return dict;
        }
    }
}
