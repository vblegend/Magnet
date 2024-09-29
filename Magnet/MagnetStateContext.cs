using Magnet.Core;
using System.Diagnostics;


namespace Magnet
{
    internal class MagnetStateContext : IStateContext, IDisposable
    {

        private MagnetEngine engine;



        internal MagnetStateContext(MagnetEngine engine)
        {
            this.engine = engine;
        }


        public ScriptRunMode RunMode => engine.Options.Mode;

        public bool UseDebuggerBreak => engine.Options.UseDebugger;

        public void Dispose()
        {
            instancesByType.Clear();
            instancesByString.Clear();
        }










        #region Instances

        private Dictionary<Type, BaseScript> instancesByType = new Dictionary<Type, BaseScript>();
        private Dictionary<String, BaseScript> instancesByString = new Dictionary<String, BaseScript>();

        internal void AddInstance(ScriptAttribute attribute, BaseScript script)
        {
            instancesByType.Add(script.GetType(), script);
            instancesByString.Add(attribute.Name, script);
        }
        public BaseScript InstanceOfName(string scriptName)
        {
            instancesByString.TryGetValue(scriptName, out BaseScript script);
            return script;
        }

        public T InstanceOfType<T>() where T : BaseScript
        {
            instancesByType.TryGetValue(typeof(T), out BaseScript script);
            return (T)script;
        }

        public BaseScript InstanceOfType(Type type)
        {
            instancesByType.TryGetValue(type, out BaseScript script);
            return script;
        }

        #endregion




    }
}
