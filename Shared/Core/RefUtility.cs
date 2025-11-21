using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReactiveUITK.Core
{
    internal static class RefUtility
    {
        internal static void Assign(object refTarget, VisualElement element)
        {
            if (refTarget == null)
            {
                return;
            }

            try
            {
                if (refTarget is Hooks.MutableRef<VisualElement> directRef)
                {
                    directRef.Value = element;
                    return;
                }

                if (TrySetGenericMutableRef(refTarget, element))
                {
                    return;
                }

                if (refTarget is Action<VisualElement> visualAction)
                {
                    visualAction(element);
                    return;
                }

                if (refTarget is Action<object> objectAction)
                {
                    objectAction(element);
                    return;
                }

                if (refTarget is Delegate del)
                {
                    InvokeDelegate(del, element);
                    return;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Debug.LogWarning($"ReactiveUITK: ref assignment failed: {ex}");
                }
                catch { }
            }
        }

        private static bool TrySetGenericMutableRef(object refTarget, VisualElement element)
        {
            Type type = refTarget.GetType();
            if (
                !type.IsGenericType
                || type.GetGenericTypeDefinition() != typeof(Hooks.MutableRef<>)
            )
            {
                return false;
            }

            if (
                TryAssignProperty(
                    type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)
                )
            )
            {
                return true;
            }

            if (
                TryAssignProperty(
                    type.GetProperty("Current", BindingFlags.Instance | BindingFlags.Public)
                )
            )
            {
                return true;
            }

            if (
                TryAssignField(
                    type.GetField(
                        "Value",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                )
            )
            {
                return true;
            }

            return false;

            bool TryAssignProperty(PropertyInfo property)
            {
                if (property == null || !property.CanWrite)
                {
                    return false;
                }

                Type expectedType = property.PropertyType;
                if (element == null)
                {
                    if (
                        !expectedType.IsValueType
                        || Nullable.GetUnderlyingType(expectedType) != null
                    )
                    {
                        property.SetValue(refTarget, null);
                        return true;
                    }
                    return false;
                }

                if (
                    expectedType.IsInstanceOfType(element)
                    || expectedType.IsAssignableFrom(typeof(VisualElement))
                )
                {
                    property.SetValue(refTarget, element);
                    return true;
                }

                return false;
            }

            bool TryAssignField(FieldInfo field)
            {
                if (field == null)
                {
                    return false;
                }

                Type expectedType = field.FieldType;
                if (element == null)
                {
                    if (
                        !expectedType.IsValueType
                        || Nullable.GetUnderlyingType(expectedType) != null
                    )
                    {
                        field.SetValue(refTarget, null);
                        return true;
                    }
                    return false;
                }

                if (
                    expectedType.IsInstanceOfType(element)
                    || expectedType.IsAssignableFrom(typeof(VisualElement))
                )
                {
                    field.SetValue(refTarget, element);
                    return true;
                }

                return false;
            }
        }

        private static void InvokeDelegate(Delegate del, VisualElement element)
        {
            ParameterInfo[] parameters = del.Method.GetParameters();
            if (parameters.Length == 0)
            {
                del.DynamicInvoke();
                return;
            }

            if (parameters.Length == 1)
            {
                Type parameterType = parameters[0].ParameterType;
                object argument = null;
                bool canAssignNull =
                    !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;

                if (element == null)
                {
                    if (canAssignNull)
                    {
                        del.DynamicInvoke(argument);
                    }
                    return;
                }

                if (parameterType.IsInstanceOfType(element))
                {
                    del.DynamicInvoke(element);
                    return;
                }

                if (parameterType.IsAssignableFrom(typeof(VisualElement)))
                {
                    del.DynamicInvoke(element);
                }
            }
        }
    }
}
