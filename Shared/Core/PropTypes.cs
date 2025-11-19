using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReactiveUITK.Core
{
    public abstract class PropTypeDefinition
    {
        protected PropTypeDefinition(string name, bool required, string description)
        {
            Name = name;
            Required = required;
            Description = description;
        }

        public string Name { get; }
        public bool Required { get; }
        public string Description { get; }

        internal abstract bool IsValid(object value);
    }

    internal sealed class SimplePropTypeDefinition : PropTypeDefinition
    {
        private readonly Func<object, bool> validator;

        public SimplePropTypeDefinition(
            string name,
            bool required,
            string description,
            Func<object, bool> validator
        )
            : base(name, required, description)
        {
            this.validator = validator ?? (_ => true);
        }

        internal override bool IsValid(object value) => validator(value);
    }

    public static class PropTypes
    {
        public static PropTypeDefinition String(string name, bool required = false) =>
            new SimplePropTypeDefinition(
                name,
                required,
                "string",
                value => value == null || value is string
            );

        public static PropTypeDefinition Number(string name, bool required = false) =>
            new SimplePropTypeDefinition(
                name,
                required,
                "number",
                value =>
                    value == null
                    || value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal
            );

        public static PropTypeDefinition Boolean(string name, bool required = false) =>
            new SimplePropTypeDefinition(
                name,
                required,
                "boolean",
                value => value == null || value is bool
            );

        public static PropTypeDefinition Enum(
            string name,
            IEnumerable<string> allowedValues,
            bool required = false
        ) =>
            new SimplePropTypeDefinition(
                name,
                required,
                "enum",
                value =>
                {
                    if (value == null)
                    {
                        return true;
                    }
                    if (allowedValues == null)
                    {
                        return false;
                    }
                    foreach (var allowed in allowedValues)
                    {
                        if (string.Equals(allowed, value.ToString(), StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            );

        public static PropTypeDefinition Enum(
            string name,
            bool required = false,
            params string[] allowedValues
        ) => Enum(name, allowedValues, required);

        public static PropTypeDefinition InstanceOf<T>(string name, bool required = false) =>
            new SimplePropTypeDefinition(
                name,
                required,
                typeof(T).Name,
                value => value == null || value is T
            );

        public static PropTypeDefinition Custom(
            string name,
            Func<object, bool> validator,
            string description,
            bool required = false
        ) => new SimplePropTypeDefinition(name, required, description, validator);
    }

    internal static class PropTypeValidator
    {
        public static bool Enabled { get; set; } = true;

        public static void Validate(
            string componentName,
            IReadOnlyDictionary<string, object> props,
            IReadOnlyList<PropTypeDefinition> definitions
        )
        {
            if (!Enabled || definitions == null || definitions.Count == 0)
            {
                return;
            }
            foreach (var definition in definitions)
            {
                if (definition == null || string.IsNullOrEmpty(definition.Name))
                {
                    continue;
                }
                object propValue = null;
                bool hasValue = props != null && props.TryGetValue(definition.Name, out propValue);
                if (!hasValue)
                {
                    if (definition.Required)
                    {
                        try
                        {
                            Debug.LogWarning(
                                $"[PropTypes] Component '{componentName}' is missing required prop '{definition.Name}' (expected {definition.Description})."
                            );
                        }
                        catch
                        {
                        }
                    }
                    continue;
                }
                if (!definition.IsValid(propValue))
                {
                    try
                    {
                        string received = propValue == null ? "null" : propValue.GetType().Name;
                        Debug.LogWarning(
                            $"[PropTypes] Component '{componentName}' received invalid prop '{definition.Name}'. Expected {definition.Description}, received {received}."
                        );
                    }
                    catch
                    {
                    }
                }
            }
        }
    }

    public static class PropTypeExtensions
    {
        public static VirtualNode WithPropTypes(
            this VirtualNode node,
            params PropTypeDefinition[] definitions
        )
        {
            if (node == null || definitions == null || definitions.Length == 0)
            {
                return node;
            }
            return node.WithPropTypesImmutable(definitions);
        }
    }
}
