using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class ImageElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new Image();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is Image img && properties != null)
            {
                if (properties.TryGetValue("image", out var imageObj))
                {
                    if (imageObj is Texture2D tex)
                    {
                        img.image = tex;
                    }
                    else if (imageObj is Sprite sp)
                    {
                        img.sprite = sp;
                    }
                }
                if (properties.TryGetValue("texture", out var texObj) && texObj is Texture2D t)
                {
                    img.image = t;
                }
                if (properties.TryGetValue("sprite", out var spObj) && spObj is Sprite s)
                {
                    img.sprite = s;
                }
                if (properties.TryGetValue("scaleMode", out var smObj))
                {
                    if (smObj is ScaleMode sm)
                    {
                        img.scaleMode = sm;
                    }
                    else if (smObj is string sms)
                    {
                        sms = sms.ToLowerInvariant();
                        img.scaleMode = sms switch
                        {
                            "scalefit" or "scaletofit" => ScaleMode.ScaleToFit,
                            "scalefill" or "scaletofill" => ScaleMode.ScaleAndCrop,
                            "crop" or "center" => ScaleMode.ScaleAndCrop,
                            _ => ScaleMode.StretchToFill,
                        };
                    }
                }
            }
            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Image img)
            {
                previous ??= s_emptyProps;
                next ??= s_emptyProps;

                previous.TryGetValue("image", out var prevImage);
                next.TryGetValue("image", out var nextImage);
                if (!ReferenceEquals(prevImage, nextImage))
                {
                    if (nextImage is Texture2D tex)
                    {
                        img.image = tex;
                    }
                    else if (nextImage is Sprite sp)
                    {
                        img.sprite = sp;
                    }
                }

                previous.TryGetValue("texture", out var prevTex);
                next.TryGetValue("texture", out var nextTex);
                if (!ReferenceEquals(prevTex, nextTex) && nextTex is Texture2D t)
                {
                    img.image = t;
                }

                previous.TryGetValue("sprite", out var prevSp);
                next.TryGetValue("sprite", out var nextSp);
                if (!ReferenceEquals(prevSp, nextSp) && nextSp is Sprite s)
                {
                    img.sprite = s;
                }

                previous.TryGetValue("scaleMode", out var prevSm);
                next.TryGetValue("scaleMode", out var nextSm);
                if (!Equals(prevSm, nextSm))
                {
                    if (nextSm is ScaleMode sm)
                    {
                        img.scaleMode = sm;
                    }
                    else if (nextSm is string sms)
                    {
                        sms = sms.ToLowerInvariant();
                        img.scaleMode = sms switch
                        {
                            "scalefit" or "scaletofit" => ScaleMode.ScaleToFit,
                            "scalefill" or "scaletofill" => ScaleMode.ScaleAndCrop,
                            "crop" or "center" => ScaleMode.ScaleAndCrop,
                            _ => ScaleMode.StretchToFill,
                        };
                    }
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Image img && props is ImageProps ip)
            {
                if (ip.Texture != null)
                    img.image = ip.Texture;
                if (ip.Sprite != null)
                    img.sprite = ip.Sprite;
                if (ip.ScaleMode != null)
                {
                    var sms = ip.ScaleMode.ToLowerInvariant();
                    img.scaleMode = sms switch
                    {
                        "scalefit" or "scaletofit" => ScaleMode.ScaleToFit,
                        "scalefill" or "scaletofill" => ScaleMode.ScaleAndCrop,
                        "crop" or "center" => ScaleMode.ScaleAndCrop,
                        _ => ScaleMode.StretchToFill,
                    };
                }
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is Image img && prev is ImageProps ip && next is ImageProps inext)
            {
                if (ip.Texture != inext.Texture && inext.Texture != null)
                    img.image = inext.Texture;
                if (ip.Sprite != inext.Sprite && inext.Sprite != null)
                    img.sprite = inext.Sprite;
                if (ip.ScaleMode != inext.ScaleMode && inext.ScaleMode != null)
                {
                    var sms = inext.ScaleMode.ToLowerInvariant();
                    img.scaleMode = sms switch
                    {
                        "scalefit" or "scaletofit" => ScaleMode.ScaleToFit,
                        "scalefill" or "scaletofill" => ScaleMode.ScaleAndCrop,
                        "crop" or "center" => ScaleMode.ScaleAndCrop,
                        _ => ScaleMode.StretchToFill,
                    };
                }
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
