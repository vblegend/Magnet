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

        public AutowiredAttribute(Type type, String providerName)
        {
            this.ProviderName = providerName;
            this.Type = type;
        }

        public AutowiredAttribute(String name)
        {
            this.ProviderName = name;
        }

        public AutowiredAttribute(Type type)
        {
            this.Type = type;
        }


        /// <summary>
        /// You must specify the name of the slot
        /// </summary>
        public String ProviderName { get; set; }

        /// <summary>
        /// Specifies the type of slot
        /// </summary>
        public Type Type { get; set; }


    }
}
