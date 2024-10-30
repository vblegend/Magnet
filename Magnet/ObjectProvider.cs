using Magnet.Core;
using System;


namespace Magnet
{
    internal class ObjectProvider
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
    }
}
