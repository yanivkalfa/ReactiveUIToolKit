using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using ReactiveUITK.Signals;

namespace ReactiveUITK.Props
{
    /// <summary>
    /// Runtime helper for bridging C# reactive data sources to UITKX component props.
    ///
    /// <para>
    /// Usage pattern:
    /// <code>
    ///   // Inside a MonoBehaviour / presenter:
    ///   var disposables = new CompositeDisposable();
    ///
    ///   // Signal&lt;T&gt; overload — fires onChange whenever the signal value changes.
    ///   disposables.Add(PropsHelper.Bind(
    ///       selector    : (PlayerHUDProps p) => p.Health,
    ///       signal      : healthSignal,
    ///       onChange    : (propName, value) => { props.Health = (int)value; RequestRender(); }));
    ///
    ///   // INotifyPropertyChanged overload — fires onChange when the source property changes.
    ///   disposables.Add(PropsHelper.Bind(
    ///       selector       : (PlayerHUDProps p) => p.Health,
    ///       source         : viewModel,
    ///       sourceProperty : (PlayerViewModel vm) => vm.Health,
    ///       onChange       : (propName, value) => { props.Health = (int)value; RequestRender(); }));
    /// </code>
    /// </para>
    /// </summary>
    public static class PropsHelper
    {
        // ── Property-name extraction ──────────────────────────────────────────

        /// <summary>
        /// Extracts the property name from a simple member-access lambda such as
        /// <c>p =&gt; p.Health</c>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="selector"/> is not a direct property or field access.
        /// </exception>
        public static string GetPropertyName<T>(Expression<Func<T, object>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            Expression body = selector.Body;

            // Strip implicit boxing: (object)(int)p.Health  →  p.Health
            if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert)
                body = u.Operand;

            if (body is MemberExpression m && m.Member is MemberInfo mi &&
                (mi.MemberType == MemberTypes.Property || mi.MemberType == MemberTypes.Field))
                return mi.Name;

            throw new ArgumentException(
                "Selector must be a direct property or field access expression (e.g. p => p.Health).",
                nameof(selector));
        }

        // ── Signal<T> overloads ───────────────────────────────────────────────

        /// <summary>
        /// Creates a binding between a <see cref="Signal{TValue}"/> and a
        /// <typeparamref name="TProps"/> property.
        ///
        /// Each time the signal fires, <paramref name="onChange"/> is invoked with the
        /// property name (extracted from <paramref name="selector"/>) and the new value.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that cancels the subscription when disposed.</returns>
        public static IDisposable Bind<TProps, TValue>(
            Expression<Func<TProps, object>> selector,
            Signal<TValue> signal,
            Action<string, TValue> onChange)
            where TProps : class
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (signal   == null) throw new ArgumentNullException(nameof(signal));
            if (onChange == null) throw new ArgumentNullException(nameof(onChange));

            string propName = GetPropertyName(selector);
            return signal.Subscribe(value => onChange(propName, value));
        }

        /// <summary>
        /// Convenience overload that applies the new signal value directly to a
        /// <typeparamref name="TProps"/> instance via reflection, then calls
        /// <paramref name="onChanged"/> to notify the caller that a re-render is needed.
        /// </summary>
        public static IDisposable Bind<TProps, TValue>(
            TProps propsInstance,
            Expression<Func<TProps, object>> selector,
            Signal<TValue> signal,
            Action onChanged = null)
            where TProps : class
        {
            if (propsInstance == null) throw new ArgumentNullException(nameof(propsInstance));
            if (selector      == null) throw new ArgumentNullException(nameof(selector));
            if (signal        == null) throw new ArgumentNullException(nameof(signal));

            string propName = GetPropertyName(selector);
            PropertyInfo prop = typeof(TProps).GetProperty(propName,
                BindingFlags.Instance | BindingFlags.Public);

            if (prop == null || !prop.CanWrite)
            {
                throw new InvalidOperationException(
                    $"Property '{propName}' on '{typeof(TProps).Name}' does not exist or is read-only.");
            }

            return signal.Subscribe(value =>
            {
                prop.SetValue(propsInstance, value);
                onChanged?.Invoke();
            });
        }

        // ── INotifyPropertyChanged overloads ──────────────────────────────────

        /// <summary>
        /// Creates a binding between an <see cref="INotifyPropertyChanged"/> source and a
        /// <typeparamref name="TProps"/> property.
        ///
        /// Each time <paramref name="sourceProperty"/> changes on <paramref name="source"/>,
        /// <paramref name="onChange"/> is invoked with the mapped props property name and the
        /// current value of <paramref name="sourceProperty"/>.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that removes the event handler when disposed.</returns>
        public static IDisposable Bind<TProps, TSource>(
            Expression<Func<TProps, object>> selector,
            TSource source,
            Expression<Func<TSource, object>> sourceProperty,
            Action<string, object> onChange)
            where TProps  : class
            where TSource : INotifyPropertyChanged
        {
            if (selector       == null) throw new ArgumentNullException(nameof(selector));
            if (source         == null) throw new ArgumentNullException(nameof(source));
            if (sourceProperty == null) throw new ArgumentNullException(nameof(sourceProperty));
            if (onChange       == null) throw new ArgumentNullException(nameof(onChange));

            string propName       = GetPropertyName(selector);
            string sourcePropName = GetPropertyName(sourceProperty);
            Func<TSource, object> getter = sourceProperty.Compile();

            void Handler(object sender, PropertyChangedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == sourcePropName)
                    onChange(propName, getter(source));
            }

            source.PropertyChanged += Handler;
            return new ActionDisposable(() => source.PropertyChanged -= Handler);
        }

        /// <summary>
        /// Convenience overload that applies the source property value directly to a
        /// <typeparamref name="TProps"/> instance via reflection, then calls
        /// <paramref name="onChanged"/>.
        /// </summary>
        public static IDisposable Bind<TProps, TSource>(
            TProps propsInstance,
            Expression<Func<TProps, object>> selector,
            TSource source,
            Expression<Func<TSource, object>> sourceProperty,
            Action onChanged = null)
            where TProps  : class
            where TSource : INotifyPropertyChanged
        {
            if (propsInstance  == null) throw new ArgumentNullException(nameof(propsInstance));
            if (selector       == null) throw new ArgumentNullException(nameof(selector));
            if (source         == null) throw new ArgumentNullException(nameof(source));
            if (sourceProperty == null) throw new ArgumentNullException(nameof(sourceProperty));

            string propName       = GetPropertyName(selector);
            string sourcePropName = GetPropertyName(sourceProperty);
            Func<TSource, object> getter = sourceProperty.Compile();

            PropertyInfo prop = typeof(TProps).GetProperty(propName,
                BindingFlags.Instance | BindingFlags.Public);

            if (prop == null || !prop.CanWrite)
            {
                throw new InvalidOperationException(
                    $"Property '{propName}' on '{typeof(TProps).Name}' does not exist or is read-only.");
            }

            void Handler(object sender, PropertyChangedEventArgs e)
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == sourcePropName)
                {
                    prop.SetValue(propsInstance, getter(source));
                    onChanged?.Invoke();
                }
            }

            source.PropertyChanged += Handler;
            return new ActionDisposable(() => source.PropertyChanged -= Handler);
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private sealed class ActionDisposable : IDisposable
        {
            private Action _action;

            internal ActionDisposable(Action action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public void Dispose()
            {
                Action a = _action;
                _action = null;
                a?.Invoke();
            }
        }
    }
}
