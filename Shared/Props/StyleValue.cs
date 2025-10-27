namespace ReactiveUITK.Props
{
    public readonly struct StyleValue<T>
    {
        public T Value { get; }

        public StyleValue(T value)
        {
            Value = value;
        }
    }
}
