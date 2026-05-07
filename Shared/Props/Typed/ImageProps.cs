using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Props.Typed
{
    public sealed class ImageProps : BaseProps
    {
        public Texture2D Texture { get; set; }
        public Sprite Sprite { get; set; }
        public string ScaleMode { get; set; }

        public override bool ShallowEquals(BaseProps other)
        {
            if (!base.ShallowEquals(other))
                return false;
            if (other is not ImageProps o)
                return false;
            if (Texture != o.Texture)
                return false;
            if (Sprite != o.Sprite)
                return false;
            if (ScaleMode != o.ScaleMode)
                return false;
            return true;
        }

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

        internal override void __ResetFields()
        {
            Texture = null;
            Sprite = null;
            ScaleMode = null;
        }

        internal override void __ReturnToPool()
        {
            Pool<ImageProps>.Return(this);
        }
    }
}
