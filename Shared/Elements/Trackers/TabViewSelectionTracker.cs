using System;
using System.Collections.Generic;
using System.Reflection;
using ReactiveUITK.Core;
using UnityEngine.UIElements;

namespace ReactiveUITK.Elements.Trackers
{
    public sealed class TabViewSelectionState
    {
        public bool ActiveTabHooked;
        public bool SelectedIndexHooked;
        public bool SelectedTabIndexHooked;
        public bool ChangeHooked;
        public bool DetachHooked;
        public int SuppressCount;
        public int? LastKnownIndex;
        public Delegate IndexChangedDelegate;
        public Delegate ActiveTabChangedDelegate;
        public Delegate ActiveTabHandler;
        public Delegate SelectedIndexHandler;
        public Delegate SelectedTabIndexHandler;
        public EventCallback<ChangeEvent<int>> ChangeCallback;
    }

    public sealed class TabViewSelectionTracker
        : IElementStateTracker<TabView, TabViewSelectionState>
    {
        private static readonly PropertyInfo SelectedTabIndexProperty = typeof(TabView).GetProperty(
            "selectedTabIndex",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        private static readonly PropertyInfo ActiveTabProperty = typeof(TabView).GetProperty(
            "activeTab",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        private static readonly EventInfo ActiveTabChangedEvent = typeof(TabView).GetEvent(
            "activeTabChanged",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        private static readonly EventInfo SelectedIndexChangedEvent = typeof(TabView).GetEvent(
            "selectedIndexChanged",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        private static readonly EventInfo SelectedTabIndexChangedEvent = typeof(TabView).GetEvent(
            "selectedTabIndexChanged",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        public void Attach(
            TabView element,
            TabViewSelectionState state,
            IReadOnlyDictionary<string, object> props
        )
        {
            if (element == null || state == null)
            {
                return;
            }

            EnsureHooks(element, state);
            UpdateDelegates(state, props);
        }

        public void Detach(TabView element, TabViewSelectionState state)
        {
            DetachHooks(element, state);
        }

        public void Reapply(
            TabView element,
            TabViewSelectionState state,
            IReadOnlyDictionary<string, object> previousProps,
            IReadOnlyDictionary<string, object> nextProps
        )
        {
            if (element == null || state == null)
            {
                return;
            }

            UpdateDelegates(state, nextProps);
            ApplySelection(element, state, nextProps);
        }

        public void BeginSuppression(TabViewSelectionState state)
        {
            if (state == null)
            {
                return;
            }
            state.SuppressCount++;
        }

        public void EndSuppression(TabViewSelectionState state)
        {
            if (state == null)
            {
                return;
            }
            if (state.SuppressCount > 0)
            {
                state.SuppressCount--;
            }
        }

        public bool ShouldSuppressForProps(IReadOnlyDictionary<string, object> props)
        {
            if (props == null)
            {
                return false;
            }
            return props.ContainsKey("selectedTabIndex") || props.ContainsKey("selectedIndex");
        }

        public void SyncFromView(TabView element, TabViewSelectionState state)
        {
            if (element == null || state == null)
            {
                return;
            }
            state.LastKnownIndex = NormalizeIndex(GetSelectedIndex(element));
        }

        public static bool TabsChanged(
            IReadOnlyDictionary<string, object> previous,
            IReadOnlyDictionary<string, object> next
        )
        {
            object prevTabs = null;
            object nextTabs = null;

            bool prevHas = previous != null && previous.TryGetValue("tabs", out prevTabs);
            bool nextHas = next != null && next.TryGetValue("tabs", out nextTabs);

            if (!prevHas && !nextHas)
            {
                return false;
            }
            if (prevHas != nextHas)
            {
                return true;
            }
            if (ReferenceEquals(prevTabs, nextTabs))
            {
                return false;
            }
            return !Equals(prevTabs, nextTabs);
        }

        private void EnsureHooks(TabView view, TabViewSelectionState state)
        {
            if (view == null || state == null)
            {
                return;
            }

            if (!state.ActiveTabHooked && ActiveTabChangedEvent != null)
            {
                try
                {
                    Action<Tab, Tab> handler = (prev, next) =>
                        HandleActiveTabChanged(view, state, prev, next);
                    ActiveTabChangedEvent.AddEventHandler(view, handler);
                    state.ActiveTabHandler = handler;
                    state.ActiveTabHooked = true;
                }
                catch
                {
                    state.ActiveTabHandler = null;
                    state.ActiveTabHooked = false;
                }
            }

            if (!state.SelectedIndexHooked && SelectedIndexChangedEvent != null)
            {
                try
                {
                    Action<int> handler = index => HandleExplicitIndexChanged(view, state, index);
                    SelectedIndexChangedEvent.AddEventHandler(view, handler);
                    state.SelectedIndexHandler = handler;
                    state.SelectedIndexHooked = true;
                }
                catch
                {
                    state.SelectedIndexHandler = null;
                    state.SelectedIndexHooked = false;
                }
            }

            if (!state.SelectedTabIndexHooked && SelectedTabIndexChangedEvent != null)
            {
                try
                {
                    Action<int> handler = index => HandleExplicitIndexChanged(view, state, index);
                    SelectedTabIndexChangedEvent.AddEventHandler(view, handler);
                    state.SelectedTabIndexHandler = handler;
                    state.SelectedTabIndexHooked = true;
                }
                catch
                {
                    state.SelectedTabIndexHandler = null;
                    state.SelectedTabIndexHooked = false;
                }
            }

            if (!state.ChangeHooked)
            {
                try
                {
                    state.ChangeCallback = evt => HandleIndexChangeEvent(view, state, evt);
                    view.RegisterCallback<ChangeEvent<int>>(state.ChangeCallback);
                    state.ChangeHooked = true;
                }
                catch
                {
                    state.ChangeCallback = default;
                    state.ChangeHooked = false;
                }
            }

            if (!state.DetachHooked)
            {
                state.DetachHooked = true;
                view.RegisterCallback<DetachFromPanelEvent>(_ => DetachHooks(view, state));
            }
        }

        private void DetachHooks(TabView view, TabViewSelectionState state)
        {
            if (view == null || state == null)
            {
                return;
            }

            if (
                state.ActiveTabHooked
                && state.ActiveTabHandler != null
                && ActiveTabChangedEvent != null
            )
            {
                try
                {
                    ActiveTabChangedEvent.RemoveEventHandler(view, state.ActiveTabHandler);
                }
                catch { }
            }
            state.ActiveTabHandler = null;
            state.ActiveTabHooked = false;

            if (
                state.SelectedIndexHooked
                && state.SelectedIndexHandler != null
                && SelectedIndexChangedEvent != null
            )
            {
                try
                {
                    SelectedIndexChangedEvent.RemoveEventHandler(view, state.SelectedIndexHandler);
                }
                catch { }
            }
            state.SelectedIndexHandler = null;
            state.SelectedIndexHooked = false;

            if (
                state.SelectedTabIndexHooked
                && state.SelectedTabIndexHandler != null
                && SelectedTabIndexChangedEvent != null
            )
            {
                try
                {
                    SelectedTabIndexChangedEvent.RemoveEventHandler(
                        view,
                        state.SelectedTabIndexHandler
                    );
                }
                catch { }
            }
            state.SelectedTabIndexHandler = null;
            state.SelectedTabIndexHooked = false;

            if (state.ChangeHooked && state.ChangeCallback != null)
            {
                try
                {
                    view.UnregisterCallback<ChangeEvent<int>>(state.ChangeCallback);
                }
                catch { }
            }
            state.ChangeCallback = default;
            state.ChangeHooked = false;
            state.SuppressCount = 0;
        }

        private void UpdateDelegates(
            TabViewSelectionState state,
            IReadOnlyDictionary<string, object> props
        )
        {
            if (state == null)
            {
                return;
            }

            if (props == null)
            {
                return;
            }

            if (props.TryGetValue("selectedIndexChanged", out var idxChanged))
            {
                state.IndexChangedDelegate = NormalizeIndexDelegate(idxChanged);
            }
            if (props.TryGetValue("selectedTabIndexChanged", out var tabIdxChanged))
            {
                state.IndexChangedDelegate = NormalizeIndexDelegate(tabIdxChanged);
            }
            if (props.TryGetValue("activeTabChanged", out var activeChanged))
            {
                state.ActiveTabChangedDelegate = activeChanged as Delegate;
            }
            if (props.TryGetValue("activeTabIndexChanged", out var activeIdxChanged))
            {
                state.ActiveTabChangedDelegate = activeIdxChanged as Delegate;
            }
        }

        private void ApplySelection(
            TabView view,
            TabViewSelectionState state,
            IReadOnlyDictionary<string, object> props
        )
        {
            if (view == null || state == null)
            {
                return;
            }

            if (TryReadIndex(props, out var desiredIndex))
            {
                SetSelectedIndex(view, state, desiredIndex);
            }
            else if (state.LastKnownIndex == null)
            {
                SyncFromView(view, state);
            }
        }

        private void HandleActiveTabChanged(
            TabView view,
            TabViewSelectionState state,
            Tab prev,
            Tab next
        )
        {
            if (IsSuppressed(state))
            {
                return;
            }

            int resolvedIndex = ResolveTabIndex(view, next);
            state.LastKnownIndex = NormalizeIndex(resolvedIndex);
            DispatchSelectionChanged(view, state, resolvedIndex, prev, next);
        }

        private void HandleExplicitIndexChanged(
            TabView view,
            TabViewSelectionState state,
            int index
        )
        {
            if (IsSuppressed(state))
            {
                return;
            }

            int previousIndex = state.LastKnownIndex ?? -1;
            var prevTab = ResolveTabByIndex(view, previousIndex);
            var nextTab = ResolveTabByIndex(view, index);
            state.LastKnownIndex = NormalizeIndex(index);
            DispatchSelectionChanged(view, state, index, prevTab, nextTab);
        }

        private void HandleIndexChangeEvent(
            TabView view,
            TabViewSelectionState state,
            ChangeEvent<int> evt
        )
        {
            if (IsSuppressed(state))
            {
                return;
            }

            int newIndex = evt != null ? evt.newValue : GetSelectedIndex(view);
            int previousIndex = evt != null ? evt.previousValue : state.LastKnownIndex ?? -1;
            var prevTab = ResolveTabByIndex(view, previousIndex);
            var nextTab = ResolveTabByIndex(view, newIndex);
            state.LastKnownIndex = NormalizeIndex(newIndex);
            DispatchSelectionChanged(view, state, newIndex, prevTab, nextTab);
        }

        private void DispatchSelectionChanged(
            TabView view,
            TabViewSelectionState state,
            int index,
            Tab previous,
            Tab next
        )
        {
            NotifyIndexDelegate(state.IndexChangedDelegate, index, previous, next);
            NotifyActiveTabDelegate(state.ActiveTabChangedDelegate, index, previous, next);
        }

        private static void NotifyIndexDelegate(Delegate del, int index, Tab previous, Tab next)
        {
            if (del == null)
            {
                return;
            }
            try
            {
                var parameters = del.Method.GetParameters();
                if (parameters.Length == 0)
                {
                    del.DynamicInvoke();
                    return;
                }

                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    if (type == typeof(int) || type == typeof(int?))
                    {
                        args[i] = index;
                    }
                    else if (typeof(Tab).IsAssignableFrom(type))
                    {
                        args[i] = i == 0 ? next : previous;
                    }
                    else
                    {
                        args[i] = index;
                    }
                }
                del.DynamicInvoke(args);
            }
            catch { }
        }

        private static void NotifyActiveTabDelegate(Delegate del, int index, Tab previous, Tab next)
        {
            if (del == null)
            {
                return;
            }
            try
            {
                var parameters = del.Method.GetParameters();
                if (parameters.Length == 0)
                {
                    del.DynamicInvoke();
                    return;
                }

                var args = new object[parameters.Length];
                bool previousAssigned = false;
                bool nextAssigned = false;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    if (type == typeof(int) || type == typeof(int?))
                    {
                        args[i] = index;
                        continue;
                    }
                    if (typeof(Tab).IsAssignableFrom(type))
                    {
                        if (!previousAssigned)
                        {
                            args[i] = previous;
                            previousAssigned = true;
                        }
                        else if (!nextAssigned)
                        {
                            args[i] = next;
                            nextAssigned = true;
                        }
                        else
                        {
                            args[i] = next ?? previous;
                        }
                        continue;
                    }

                    args[i] = next != null ? next : (previous != null ? previous : (object)index);
                }

                del.DynamicInvoke(args);
            }
            catch { }
        }

        private void SetSelectedIndex(TabView view, TabViewSelectionState state, int index)
        {
            if (view == null || state == null)
            {
                return;
            }

            int current = GetSelectedIndex(view);
            if (current == index)
            {
                state.LastKnownIndex = NormalizeIndex(current);
                return;
            }

            BeginSuppression(state);
            try
            {
                if (!TrySetSelectedIndexInternal(view, index))
                {
                    var target = ResolveTabByIndex(view, index);
                    TrySetActiveTab(view, target);
                }
            }
            finally
            {
                EndSuppression(state);
            }

            state.LastKnownIndex = NormalizeIndex(GetSelectedIndex(view));
        }

        private static bool TrySetSelectedIndexInternal(TabView view, int index)
        {
            if (view == null)
            {
                return false;
            }

            if (SelectedTabIndexProperty != null)
            {
                try
                {
                    SelectedTabIndexProperty.SetValue(view, index);
                    return true;
                }
                catch { }
            }

            return false;
        }

        private static bool TrySetActiveTab(TabView view, Tab tab)
        {
            if (view == null || tab == null)
            {
                return false;
            }

            if (ActiveTabProperty != null)
            {
                try
                {
                    ActiveTabProperty.SetValue(view, tab);
                    return true;
                }
                catch { }
            }

            return false;
        }

        private static int GetSelectedIndex(TabView view)
        {
            if (view == null)
            {
                return -1;
            }

            if (SelectedTabIndexProperty != null)
            {
                try
                {
                    var raw = SelectedTabIndexProperty.GetValue(view);
                    if (raw != null)
                    {
                        return Convert.ToInt32(raw);
                    }
                }
                catch { }
            }

            var active = GetActiveTab(view);
            return ResolveTabIndex(view, active);
        }

        private static Tab GetActiveTab(TabView view)
        {
            if (view == null)
            {
                return null;
            }

            if (ActiveTabProperty != null)
            {
                try
                {
                    return ActiveTabProperty.GetValue(view) as Tab;
                }
                catch { }
            }

            return null;
        }

        private static int ResolveTabIndex(TabView view, Tab tab)
        {
            if (view == null || tab == null)
            {
                return -1;
            }

            try
            {
                return view.IndexOf(tab);
            }
            catch { }

            try
            {
                int count = view.childCount;
                for (int i = 0; i < count; i++)
                {
                    if (ReferenceEquals(view.ElementAt(i), tab))
                    {
                        return i;
                    }
                }
            }
            catch { }

            return -1;
        }

        private static Tab ResolveTabByIndex(TabView view, int index)
        {
            if (view == null || index < 0)
            {
                return null;
            }

            try
            {
                return view.GetTab(index);
            }
            catch { }

            try
            {
                if (index < view.childCount)
                {
                    return view.ElementAt(index) as Tab;
                }
            }
            catch { }

            return null;
        }

        private static bool TryReadIndex(IReadOnlyDictionary<string, object> props, out int index)
        {
            index = 0;
            if (props == null)
            {
                return false;
            }

            if (
                props.TryGetValue("selectedTabIndex", out var primary)
                && TryCoerceInt(primary, out index)
            )
            {
                return true;
            }

            if (
                props.TryGetValue("selectedIndex", out var legacy)
                && TryCoerceInt(legacy, out index)
            )
            {
                return true;
            }

            return false;
        }

        private static bool TryCoerceInt(object value, out int result)
        {
            if (value == null)
            {
                result = 0;
                return false;
            }

            switch (value)
            {
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = (int)l;
                    return true;
                case short s:
                    result = s;
                    return true;
                case byte b:
                    result = b;
                    return true;
                case float f when !float.IsNaN(f) && !float.IsInfinity(f):
                    result = (int)f;
                    return true;
                case double d when !double.IsNaN(d) && !double.IsInfinity(d):
                    result = (int)d;
                    return true;
            }

            try
            {
                result = Convert.ToInt32(value);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        private static int? NormalizeIndex(int index)
        {
            return index < 0 ? null : index;
        }

        private static Delegate NormalizeIndexDelegate(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is Hooks.StateSetter<int> setter)
            {
                return setter.ToValueAction();
            }

            if (value is Action<int> action)
            {
                return action;
            }

            if (value is Delegate del)
            {
                return del;
            }

            return null;
        }

        private static bool IsSuppressed(TabViewSelectionState state)
        {
            return state != null && state.SuppressCount > 0;
        }
    }
}
