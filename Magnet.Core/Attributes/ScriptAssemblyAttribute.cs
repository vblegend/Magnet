using System;


namespace Magnet.Core
{

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class ScriptAssemblyAttribute : Attribute
    {
        public ScriptAssemblyAttribute(Int64 uniqueId)
        {
            this.UniqueId = uniqueId;
        }
        public Int64 UniqueId { get; private set; }
    }
}
