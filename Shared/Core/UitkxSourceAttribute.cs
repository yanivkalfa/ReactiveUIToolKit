using System;

namespace ReactiveUITK
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UitkxSourceAttribute : Attribute
    {
        public string SourcePath { get; }

        public UitkxSourceAttribute(string sourcePath)
        {
            SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
        }
    }
}
