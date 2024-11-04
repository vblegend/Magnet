using System;

namespace Magnet.Core
{


    /// <summary>
    /// Define a method that can be called by another script or host.
    /// use ScriptState.MethodDelegate()
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {

        /// <summary>
        /// Define script methods that can be called externally, using the method name as the name of the defined function
        /// </summary>
        public FunctionAttribute()
        {

        }

        /// <summary>
        /// Define script methods that can be called externally, and define method aliases
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="description"></param>
        public FunctionAttribute(String alias = "", String description = "")
        {
            this.Alias = alias;
            this.Description = description;
        }

        /// <summary>
        /// script method alias
        /// </summary>
        public String Alias { get; set; }

        /// <summary>
        /// description, It's no use
        /// </summary>
        public String Description { get; set; }

    }
}
