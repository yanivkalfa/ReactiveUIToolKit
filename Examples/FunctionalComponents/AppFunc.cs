using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK.Core;
using ReactiveUITK;
using ReactiveUITK.Examples.Shared;
using ReactiveUITK.Props.Typed;
using System;
using static ReactiveUITK.Props.Typed.StyleKeys;
using UColor = UnityEngine.Color;

namespace ReactiveUITK.Examples.FunctionalComponents
{
    public static class AppFunc
    {
        public static VirtualNode Render(Dictionary<string, object> props, IReadOnlyList<VirtualNode> children)
        {
            return V.Func(ReactiveUITK.Examples.Shared.SharedDemoPage.Render);
        }
    }

    public sealed class AppFuncRoot : ReactiveComponent
    {
        protected override VirtualNode Render()
        {
            return V.Func(AppFunc.Render);
        }
    }
}
