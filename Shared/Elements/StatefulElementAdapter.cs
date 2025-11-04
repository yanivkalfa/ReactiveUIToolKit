using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements
{
    // Minimal generic base to hold per-element adapter state via CWT
    public abstract class StatefulElementAdapter<TElement, TState> : BaseElementAdapter
        where TElement : VisualElement
        where TState : class, new()
    {
        private static readonly ConditionalWeakTable<TElement, TState> StateTable = new();

        protected static TState GetState(TElement element)
        {
            return StateTable.GetValue(element, _ => new TState());
        }
    }
}
