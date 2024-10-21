using System;

namespace Magnet.Core
{


    /// <summary>
    /// Field injection, where an object of the related type is injected into the field by the state machine before script initialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AutowiredAttribute : Attribute
    {
        public AutowiredAttribute()
        {
            
        }

        public AutowiredAttribute(Type type, String slotName)
        {
            this.SlotName = slotName;
            this.Type = type;
        }

        public AutowiredAttribute(String name)
        {
            this.SlotName = name;
        }

        public AutowiredAttribute(Type type)
        {
            this.Type = type;
        }


        /// <summary>
        /// You must specify the name of the slot
        /// </summary>
        public String SlotName { get; set; }

        /// <summary>
        /// Specifies the type of slot
        /// </summary>
        public Type Type { get; set; }


    }
}
