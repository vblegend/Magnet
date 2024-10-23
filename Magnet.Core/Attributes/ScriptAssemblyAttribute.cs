using System;


namespace Magnet.Core
{
    /// <summary>
    /// Defines a unique ID attribute for the script assembly
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class ScriptAssemblyAttribute : Attribute
    {
        /// <summary>
        /// Defines a unique ID attribute for the script assembly
        /// </summary>
        /// <param name="uniqueId"></param>
        public ScriptAssemblyAttribute(Int64 uniqueId)
        {
            this.UniqueId = uniqueId;
        }
        /// <summary>
        /// unique ID attribute for the script assembly
        /// </summary>
        public readonly Int64 UniqueId;
    }
}
