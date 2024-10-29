using Magnet.Core;
using System;


namespace Magnet
{
    internal class ObjectProvider : IObjectProvider
    {
        internal ObjectProvider(Type type, Object value, String slotName)
        {
            this.Type = type;
            this.Value = value;
            this.SlotName = slotName;
        }
        public readonly Type Type;
        public readonly String SlotName;
        public readonly Object Value;

        Type IObjectProvider.Type => Type;

        string IObjectProvider.SlotName => SlotName;

        object IObjectProvider.Value => Value;

        public bool TypeIs<T>(string slotName = null)
        {
            return typeof(T) == this.Type && (String.IsNullOrEmpty(slotName) || slotName == SlotName);
        }
    }
}
