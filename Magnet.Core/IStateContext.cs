using System;


namespace Magnet.Core
{
    public interface IStateContext
    {
        public T InstanceOfType<T>() where T : AbstractScript;
        public AbstractScript InstanceOfType(Type type);
        public AbstractScript InstanceOfName(String scriptName);
        public ScriptRunMode RunMode { get; }
        public IOutput Output { get; }
    }
}
