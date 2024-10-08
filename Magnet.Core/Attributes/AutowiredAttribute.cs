﻿namespace Magnet.Core
{


    /// <summary>
    /// Field injection, where an object of the related type is injected into the field by the state machine before script initialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class AutowiredAttribute : Attribute
    {
        public AutowiredAttribute()
        {
            
        }

        public AutowiredAttribute(Type type)
        {
            this.Type = type;
        }


        public String Alias { get; set; }
        public Type Type { get; set; }


    }
}
