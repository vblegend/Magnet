using System;

namespace Magnet.Core
{


    /// <summary>
    /// Define a method that can be called by another script or host
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {


        public FunctionAttribute()
        {
   
        }
        public FunctionAttribute(String alias = "",String description = "")
        {
            this.Alias = alias;
            this.Description = description;
        }

        public String Alias { get; set; }


        public String Description { get; set; }

    }
}
