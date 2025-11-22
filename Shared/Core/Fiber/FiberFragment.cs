using System.Collections.Generic;

namespace ReactiveUITK.Core.Fiber
{
    /// <summary>
    /// Fragment support - render multiple children without wrapper
    /// </summary>
    public static class FiberFragment
    {
        /// <summary>
        /// Update a fragment fiber - just reconcile children
        /// </summary>
        public static FiberNode UpdateFragment(FiberNode wipFiber)
        {
            // Fragments have no host element, just reconcile children
            if (wipFiber.Children != null && wipFiber.Children.Count > 0)
            {
                var currentFirstChild = wipFiber.Alternate?.Child;
                FiberChildReconciliation.ReconcileChildren(
                    wipFiber, 
                    currentFirstChild, 
                    wipFiber.Children
                );
            }
            
            return wipFiber.Child;
        }

        /// <summary>
        /// Complete work for fragment - no element to create
        /// </summary>
        public static void CompleteFragment(FiberNode fiber)
        {
            // Fragments have no host element, nothing to do
            // Effects are still collected normally
        }
    }
}
