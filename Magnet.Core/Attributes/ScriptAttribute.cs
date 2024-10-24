using System;

namespace Magnet.Core
{


    /// <summary>
    /// Define a script object. The script object is instantiated by the state machine. The script object is independent from the state machine
    /// </summary>

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ScriptAttribute : Attribute
    {

        /// <summary>
        /// Define the script object, using the class name as the script name
        /// </summary>
        public ScriptAttribute()
        {
        }

        /// <summary>
        /// Defines a script object with the specified name
        /// </summary>
        /// <param name="alias">script name</param>
        public ScriptAttribute(string alias)
        {
            this.Alias = alias;
        }
        /// <summary>
        ///  script alias
        /// </summary>
        public string Alias { get; set; }


        /// <summary>
        /// description, It's no use
        /// </summary>
        public string Description { get; set; }
    }
}
