using UnityEngine;
using UnityEngine.UIElements;
using ReactiveUITK.Core;
using ReactiveUITK.Samples.FunctionalComponents;

namespace ReactiveUITK.Samples.Showcase.Runtime
{
   [RequireComponent(typeof(RootRenderer))]
   public class RuntimeSimpleCounterDemo : MonoBehaviour
   {
       [SerializeField] private UIDocument uiDocument;
       private RootRenderer rootRenderer;
       private void Awake()
       {
           rootRenderer = GetComponent<RootRenderer>();
           if (rootRenderer == null || uiDocument == null || uiDocument.rootVisualElement == null)
           {
               Debug.LogError("RuntimeSimpleCounterDemo: Missing RootRenderer or UIDocument");
               return;
           }
           rootRenderer.Initialize(uiDocument.rootVisualElement);
           rootRenderer.Render(V.Func(SimpleCounterFunc.Render));
       }
   }
}
