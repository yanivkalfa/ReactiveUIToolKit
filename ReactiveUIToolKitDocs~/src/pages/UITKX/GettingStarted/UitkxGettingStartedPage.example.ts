export const UITKX_INSTALL_URL = 'https://github.com/yanivkalfa/ReactiveUIToolKit.git#dist'

export const UITKX_HELLO_WORLD_COMPONENT = `@namespace MyGame.UI

component HelloWorld {
  var (count, setCount) = useState(0);

  return (
    <VisualElement>
      <Text text="Hello ReactiveUITK" />
      <Text text={$"Count: {count}"} />
      <Button text="Increment" onClick={_ => setCount(count + 1)} />
    </VisualElement>
  );
}`

export const UITKX_HELLO_WORLD_PARTIAL = `namespace MyGame.UI
{
    public partial class HelloWorld { }
}`

export const UITKX_HELLO_WORLD_BOOTSTRAP = `using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK;
using ReactiveUITK.Core;

public sealed class HelloRuntime : MonoBehaviour
{
  [SerializeField] private UIDocument uiDocument;

  private RootRenderer rootRenderer;

  private void Awake()
  {
    rootRenderer = FindObjectOfType<RootRenderer>();
    if (rootRenderer == null)
    {
      rootRenderer = new GameObject("ReactiveUIRoot").AddComponent<RootRenderer>();
    }

    rootRenderer.Initialize(uiDocument.rootVisualElement);
    rootRenderer.Render(V.Func(HelloWorld.Render));
  }
}`
