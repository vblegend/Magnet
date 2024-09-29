using Magnet.Context;

namespace Magnet
{
    internal class ScriptMetaInfo
    {
        public ScriptMetaInfo(ScriptAttribute attribute, Type type)
        {
            this.Attribute = attribute;
            this.Type = type;
        }
        public Type Type { get;private set; }

        public ScriptAttribute Attribute { get; private set; }

    }
}
