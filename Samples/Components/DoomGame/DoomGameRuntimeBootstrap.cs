using ReactiveUITK;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;
using Samples.DoomGame;
using UnityEngine;
using UnityEngine.UIElements;

namespace Samples.DoomGame
{
    /// <summary>
    /// Runtime mount for DoomGame. Attach to a GameObject that also has
    /// <see cref="RootRenderer"/> and a <see cref="UIDocument"/> in the scene.
    /// In Play Mode the OS cursor APIs (Cursor.SetCursor / Cursor.visible)
    /// actually take effect over the Game view, unlike in EditorWindow chrome.
    /// </summary>
    [RequireComponent(typeof(RootRenderer))]
    public class DoomGameRuntimeBootstrap : MonoBehaviour
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
                Debug.LogError("DoomGameRuntimeBootstrap: missing RootRenderer or UIDocument.");
                return;
            }

            rootRenderer.Initialize(uiDocument.rootVisualElement);
            var hostProps = new VisualElementProps { PickingMode = PickingMode.Ignore };
            rootRenderer.Render(V.Host(hostProps, null, V.Func(DoomGame.Render)));
        }
    }
}
