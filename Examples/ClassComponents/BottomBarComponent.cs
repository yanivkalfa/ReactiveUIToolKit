using System.Collections.Generic;
using UnityEngine;
using ReactiveUITK.Core;
using ReactiveUITK;

namespace ReactiveUITK.Examples.ClassComponents
{
    public sealed class BottomBarComponent : ReactiveComponent
    {
        private int leftClicks;
        private int rightClicks;

        protected override VirtualNode Render()
        {
            var barStyle = new Dictionary<string, object>
            {
                {"backgroundColor", Color.white},
                {"flexDirection", "row"},
                {"justifyContent", "space-between"},
                {"alignItems", "center"},
                {"paddingLeft", 12f},
                {"paddingRight", 12f},
                {"paddingTop", 8f},
                {"paddingBottom", 8f},
                {"borderTopWidth", 1f},
                {"borderTopColor", new Color(0.85f,0.85f,0.85f,1f)}
            };

            var buttonBaseStyle = new Dictionary<string, object>
            {
                {"color", Color.white},
                {"paddingLeft", 10f},
                {"paddingRight", 10f},
                {"paddingTop", 6f},
                {"paddingBottom", 6f},
                {"borderRadius", 4f},
                {"fontSize", 14f}
            };

            var leftButtonStyle = new Dictionary<string, object>(buttonBaseStyle)
            {
                {"backgroundColor", new Color(0.2f,0.6f,0.3f,1f)}
            };
            var rightButtonStyle = new Dictionary<string, object>(buttonBaseStyle)
            {
                {"backgroundColor", new Color(0.7f,0.2f,0.6f,1f)}
            };

            var leftButtonProps = new Dictionary<string, object>
            {
                {"style", leftButtonStyle},
                {"onClick", (System.Action)(() => SetState(ref leftClicks, leftClicks + 1))},
                {"text", $"Bottom Left ({leftClicks})"}
            };
            var rightButtonProps = new Dictionary<string, object>
            {
                {"style", rightButtonStyle},
                {"onClick", (System.Action)(() => SetState(ref rightClicks, rightClicks + 1))},
                {"text", $"Bottom Right ({rightClicks})"}
            };
            var barProps = new Dictionary<string, object>{{"style", barStyle}};

            return V.VisualElement(barProps, null,
                V.Button(leftButtonProps),
                V.Button(rightButtonProps)
            );
        }
    }
}
