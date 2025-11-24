using System.Collections.Generic;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using ReactiveUITK.Router;
using UnityEngine;

namespace ReactiveUITK.Samples.FunctionalComponents
{
    /// <summary>
    /// Minimal repro that nests GuardReproFunc under Router.
    /// Used to compare behavior with the standalone guard demo.
    /// </summary>
    public static class RouterGuardReproFunc
    {
        public static VirtualNode Render(
            Dictionary<string, object> props,
            IReadOnlyList<VirtualNode> children
        )
        {
            Debug.Log("[RouterGuardRepro] Render root");

            return V.Router(
                children: new[]
                {
                    V.VisualElement(
                        new Style
                        {
                            (StyleKeys.FlexDirection, "column"),
                            (StyleKeys.Padding, 10f),
                            (StyleKeys.FlexGrow, 1f),
                        },
                        "router-guard-root",
                        V.Text("Router + Guard repro", "label"),
                        V.Func(GuardReproFunc.Render, key: "guard")
                    ),
                }
            );
        }
    }
}
