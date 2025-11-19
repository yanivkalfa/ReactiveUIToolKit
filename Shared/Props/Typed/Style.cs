using System.Collections.Generic;

namespace ReactiveUITK.Props.Typed
{
    
    public class Style : Dictionary<string, object>
    {
        public Style() { }

        public Style(int capacity)
            : base(capacity) { }

        public Style(IDictionary<string, object> dictionary)
            : base(dictionary) { }

        
        public void Add((string key, object value) entry)
        {
            this[entry.key] = entry.value;
        }

        public static Style Of(params (string key, object value)[] entries)
        {
            var style = new Style(entries?.Length ?? 0);
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    style[entries[i].key] = entries[i].value;
                }
            }
            return style;
        }
    }
}
