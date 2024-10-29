using System;

namespace Magnet.Core
{

    
    /// <summary>
    /// Field injection, where an object of the related type is injected into the field by the state machine before script initialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AutowiredAttribute : Attribute
    {

        /// <summary>
        /// The tag field is an injection type and can be injected by an external Provider
        /// </summary>
        public AutowiredAttribute()
        {

        }

        /// <summary>
        /// Mark the field as an injection type, which can be injected by an external Provider. Set the injection type and name of the field
        /// </summary>
        /// <param name="type"></param>
        /// <param name="providerName"></param>
        public AutowiredAttribute(Type type, String providerName)
        {
            this.ProviderName = providerName;
            this.Type = type;
        }

        /// <summary>
        /// Mark the field as an injection type, which can be injected by an external Provider. Set the injection name of the field
        /// </summary>
        /// <param name="name"></param>
        public AutowiredAttribute(String name)
        {
            this.ProviderName = name;
        }


        /// <summary>
        /// Mark the field as an injection type, which can be injected by an external Provider. Set the injection type of the field
        /// </summary>
        /// <param name="type"></param>
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
