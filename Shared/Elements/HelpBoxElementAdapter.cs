#if UNITY_EDITOR
using System.Collections.Generic;
using ReactiveUITK.Props;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class HelpBoxElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new HelpBox();
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (element is HelpBox hb && properties != null)
            {
                TryApplyProp<string>(properties, "text", v => hb.text = v ?? string.Empty);
                if (properties.TryGetValue("messageType", out var mt))
                {
                    if (mt is HelpBoxMessageType t)
                    {
                        hb.messageType = t;
                    }
                    else if (mt is string ms)
                    {
                        ms = ms.ToLowerInvariant();
                        hb.messageType = ms switch
                        {
                            "warning" => HelpBoxMessageType.Warning,
                            "error" => HelpBoxMessageType.Error,
                            _ => HelpBoxMessageType.Info,
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
            if (element is HelpBox hb)
            {
                TryDiffProp<string>(previous, next, "text", v => hb.text = v ?? string.Empty);
                previous ??= s_emptyProps;
                next ??= s_emptyProps;
                previous.TryGetValue("messageType", out var pm);
                next.TryGetValue("messageType", out var nm);
                if (!Equals(pm, nm))
                {
                    if (nm is HelpBoxMessageType t)
                    {
                        hb.messageType = t;
                    }
                    else if (nm is string ms)
                    {
                        ms = ms.ToLowerInvariant();
                        hb.messageType = ms switch
                        {
                            "warning" => HelpBoxMessageType.Warning,
                            "error" => HelpBoxMessageType.Error,
                            _ => HelpBoxMessageType.Info,
                        };
                    }
                }
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }
    }
}
#endif
