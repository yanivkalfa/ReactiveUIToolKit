using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Samples.UITKXShared
{
    /// <summary>
    /// Runtime mount for the Media Playground demo. Attach to a GameObject
    /// that also has <see cref="RootRenderer"/> and a <see cref="UIDocument"/>
    /// in the scene. Used to validate that the &lt;Video&gt; element behaves
    /// the same way in Play Mode (where the runtime player loop ticks
    /// naturally) as in the Editor preview window.
    /// </summary>
    [RequireComponent(typeof(RootRenderer))]
    public class MediaPlaygroundRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField]
        private UIDocument uiDocument;
        private RootRenderer rootRenderer;

        private void Awake()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;

            rootRenderer = GetComponent<RootRenderer>();
            if (rootRenderer == null || uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError(
                    "MediaPlaygroundRuntimeBootstrap: missing RootRenderer or UIDocument."
                );
                return;
            }

            rootRenderer.Initialize(uiDocument.rootVisualElement);
            var hostProps = new VisualElementProps { PickingMode = PickingMode.Ignore };
            rootRenderer.Render(V.Host(hostProps, null, V.Func(MediaPlaygroundDemoPage.Render)));
        }
    }
}
