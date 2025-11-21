export const INSTALL_URL = 'https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist'

export const HELLO_WORLD_EDITOR = `// EditorWindow sample (C#)
[MenuItem("Window/ReactiveUITK/Hello World")]
static void Open() {
  var w = GetWindow<EditorWindow>("Hello");
  ReactiveUITK.EditorSupport.EditorRootRendererUtility.Render(
    w.rootVisualElement,
    ReactiveUITK.V.VisualElement(null, null,
      ReactiveUITK.V.Label(new ReactiveUITK.Props.Typed.LabelProps { Text = "Hello ReactiveUITK" })
    )
  );
}`

