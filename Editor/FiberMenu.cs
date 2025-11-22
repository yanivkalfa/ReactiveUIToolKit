using UnityEngine;
using ReactiveUITK.Core.Fiber;
using ReactiveUITK.Tests;

namespace ReactiveUITK.EditorSupport
{
    /// <summary>
    /// Menu items for toggling Fiber reconciler
    /// </summary>
    public static class FiberMenu
    {
#if UNITY_EDITOR
        private const string MenuPath = "ReactiveUITK/Use Fiber Reconciler";
        
        [UnityEditor.MenuItem(MenuPath)]
        public static void ToggleFiberReconciler()
        {
            FiberConfig.UseFiberReconciler = !FiberConfig.UseFiberReconciler;
            UnityEditor.Menu.SetChecked(MenuPath, FiberConfig.UseFiberReconciler);
            
            if (FiberConfig.UseFiberReconciler)
            {
                Debug.Log("✅ Fiber Reconciler ENABLED - New components will use Fiber");
            }
            else
            {
                Debug.Log("⛔ Fiber Reconciler DISABLED - Using legacy reconciler");
            }
        }
        
        [UnityEditor.MenuItem(MenuPath, true)]
        public static bool ToggleFiberReconcilerValidate()
        {
            UnityEditor.Menu.SetChecked(MenuPath, FiberConfig.UseFiberReconciler);
            return true;
        }

        [UnityEditor.MenuItem("ReactiveUITK/Run Fiber Tests")]
        public static void RunFiberTests()
        {
            Debug.Log("=== Running Fiber Tests ===");
            
            // Create test container
            var testRoot = new UnityEngine.UIElements.VisualElement();
            testRoot.name = "FiberTestRoot";
            
            try
            {
                FiberTest.RunBasicTest(testRoot);
                Debug.Log("✅ Basic tests passed!");
                
                testRoot.Clear();
                FiberTest.RunCounterTest(testRoot);
                Debug.Log("✅ Counter tests passed!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Fiber tests failed: {ex}");
            }
        }
#endif
    }
}
