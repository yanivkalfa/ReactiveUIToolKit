using System.Collections.Generic;
using ReactiveUITK.Props;
using ReactiveUITK.Props.Typed;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    public sealed class FoldoutElementAdapter : BaseElementAdapter
    {
        public override VisualElement Create()
        {
            return new Foldout();
        }

        public override VisualElement ResolveChildHost(VisualElement element)
        {
            var fo = element as Foldout;
            return fo != null ? fo.contentContainer : element;
        }

        private static void ApplySlots(Foldout fo, IReadOnlyDictionary<string, object> properties)
        {
            if (properties == null)
            {
                return;
            }
            if (
                properties.TryGetValue("contentContainer", out var ccObj)
                && ccObj is Dictionary<string, object> ccMap
            )
            {
                PropsApplier.Apply(fo.contentContainer, ccMap);
            }
            if (
                properties.TryGetValue("header", out var headerObj)
                && headerObj is Dictionary<string, object> headerMap
            )
            {
                var header = fo.Q<Toggle>();
                if (header != null)
                {
                    PropsApplier.Apply(header, headerMap);
                }
            }
        }

        private static void ApplySlotsDiff(
            Foldout fo,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            previous ??= s_emptyProps;
            next ??= s_emptyProps;
            previous.TryGetValue("contentContainer", out var prevCC);
            next.TryGetValue("contentContainer", out var nextCC);
            if (!ReferenceEquals(prevCC, nextCC) && nextCC is Dictionary<string, object> ccMap)
            {
                PropsApplier.Apply(fo.contentContainer, ccMap);
            }
            previous.TryGetValue("header", out var prevHeader);
            next.TryGetValue("header", out var nextHeader);
            if (
                !ReferenceEquals(prevHeader, nextHeader)
                && nextHeader is Dictionary<string, object> headerMap
            )
            {
                var header = fo.Q<Toggle>();
                if (header != null)
                {
                    PropsApplier.Apply(header, headerMap);
                }
            }
        }

        public override void ApplyProperties(
            VisualElement element,
            IReadOnlyDictionary<string, object> properties
        )
        {
            if (!(element is Foldout fo) || properties == null)
            {
                PropsApplier.Apply(element, properties);
                return;
            }

            TryApplyProp<string>(properties, "text", t => fo.text = t);
            TryApplyProp<bool>(properties, "value", v => fo.value = v);

            ApplySlots(fo, properties);

            PropsApplier.Apply(element, properties);
        }

        public override void ApplyPropertiesDiff(
            VisualElement element,
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            if (element is Foldout fo)
            {
                TryDiffProp<string>(previous, next, "text", v => fo.text = v);
                TryDiffProp<bool>(previous, next, "value", v => fo.value = v);

                ApplySlotsDiff(fo, previous, next);
            }
            PropsApplier.ApplyDiff(element, previous, next);
        }

        public override void ApplyTypedFull(VisualElement element, BaseProps props)
        {
            if (element is Foldout fo && props is FoldoutProps tp)
            {
                if (tp.Text != null)
                    fo.text = tp.Text;
                if (tp.Value.HasValue)
                    fo.value = tp.Value.Value;
                if (tp.OnChange != null)
                    PropsApplier.ApplySingle(element, null, "onChange", tp.OnChange);
                if (tp.OnChangeCapture != null)
                    PropsApplier.ApplySingle(element, null, "onChangeCapture", tp.OnChangeCapture);
                if (tp.Header != null)
                {
                    var header = fo.Q<Toggle>();
                    if (header != null)
                        PropsApplier.Apply(header, tp.Header);
                }
                if (props.ContentContainer is Dictionary<string, object> ccMap)
                    PropsApplier.Apply(fo.contentContainer, ccMap);
            }
            base.ApplyTypedFull(element, props);
        }

        public override void ApplyTypedDiff(VisualElement element, BaseProps prev, BaseProps next)
        {
            if (element is Foldout fo && prev is FoldoutProps tp && next is FoldoutProps tn)
            {
                if (tp.Text != tn.Text)
                    fo.text = tn.Text ?? string.Empty;
                if (tp.Value != tn.Value && tn.Value.HasValue)
                    fo.value = tn.Value.Value;
                if (tp.OnChange != tn.OnChange)
                {
                    if (tn.OnChange != null)
                        PropsApplier.ApplySingle(element, tp.OnChange, "onChange", tn.OnChange);
                    else if (tp.OnChange != null)
                        PropsApplier.RemoveProp(element, "onChange", tp.OnChange);
                }
                if (tp.OnChangeCapture != tn.OnChangeCapture)
                {
                    if (tn.OnChangeCapture != null)
                        PropsApplier.ApplySingle(
                            element,
                            tp.OnChangeCapture,
                            "onChangeCapture",
                            tn.OnChangeCapture
                        );
                    else if (tp.OnChangeCapture != null)
                        PropsApplier.RemoveProp(element, "onChangeCapture", tp.OnChangeCapture);
                }
                if (!ReferenceEquals(tp.Header, tn.Header))
                {
                    var header = fo.Q<Toggle>();
                    if (header != null)
                    {
                        if (tp.Header != null && tn.Header != null)
                            PropsApplier.ApplyDiff(header, tp.Header, tn.Header);
                        else if (tn.Header != null)
                            PropsApplier.Apply(header, tn.Header);
                    }
                }
                if (
                    !ReferenceEquals(prev.ContentContainer, next.ContentContainer)
                    && next.ContentContainer is Dictionary<string, object> ccMap
                )
                    PropsApplier.Apply(fo.contentContainer, ccMap);
            }
            base.ApplyTypedDiff(element, prev, next);
        }
    }
}
