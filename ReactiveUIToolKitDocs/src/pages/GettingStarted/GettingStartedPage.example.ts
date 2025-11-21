export const INSTALL_URL = 'https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist'

export const HELLO_WORLD_EDITOR = `using UnityEditor;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.EditorSupport;
using ReactiveUITK.Props.Typed;

// EditorWindow sample (C#)
[MenuItem("Window/ReactiveUITK/Hello World")]
static void Open() {
  var w = GetWindow<EditorWindow>("Hello");
  EditorRootRendererUtility.Render(
    w.rootVisualElement,
    V.VisualElement(null, null,
      V.Label(new LabelProps { Text = "Hello ReactiveUITK" })
    )
  );
}`

export const HELLO_WORLD_RUNTIME = `// Runtime MonoBehaviour with RootRenderer (C#)
using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Props.Typed;

public sealed class HelloRuntime : MonoBehaviour
{
  [SerializeField] private UIDocument uiDocument;

  private RootRenderer _rootRenderer;

  private void Awake()
  {
    if (uiDocument == null)
    {
      Debug.LogError("Assign UIDocument on HelloRuntime");
      return;
    }

    // Create / reuse a RootRenderer in the scene
    _rootRenderer = FindObjectOfType<RootRenderer>();
    if (_rootRenderer == null)
    {
      _rootRenderer = new GameObject("ReactiveUIRoot").AddComponent<RootRenderer>();
    }

    _rootRenderer.Initialize(uiDocument.rootVisualElement);

    // Render a simple VNode tree
    var vnode = V.VisualElement(
      null,
      null,
      V.Label(new LabelProps { Text = "Hello ReactiveUITK (Runtime)" })
    );

    _rootRenderer.Render(vnode);
  }
}`
