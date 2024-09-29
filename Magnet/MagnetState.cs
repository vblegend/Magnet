using Magnet.Context;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Magnet
{
    public class MagnetState : IDisposable
    {
        private MagnetEngine engine;
        private ScriptCollection scriptCollection = new ScriptCollection();
        // ScriptLoadContext scriptLoadContext,
        internal MagnetState(MagnetEngine engine)
        {
            this.engine = engine;
            this.CreateState();
        }



        private void CreateState()
        {
            ScriptCollection scriptCollection = new ScriptCollection();
            List<IScriptInstance> instances = new List<IScriptInstance>();
            foreach (var meta in this.engine.scriptMetaInfos)
            {
                var instance = (BaseScript)Activator.CreateInstance(meta.Type);
                scriptCollection.Add(meta.Attribute, instance);
                instances.Add(instance);
            }
            // Injected Data
            foreach (var instance in instances)
            {
                instance.InjectedContext(scriptCollection);
            }
            // Exec Init Function
            foreach (var instance in instances)
            {
                instance.Initialize();
            }
            this.scriptCollection = scriptCollection;
        }





        public T GetDelegate<T>(String scriptName, String methodName) where T : Delegate
        {
            BaseScript script = scriptCollection.NameOf(scriptName);
            if (script != null)
            {
                // 获取对象的类型
                Type type = script.GetType();
                // 获取方法信息 (MethodInfo)
                MethodInfo methodInfo = type.GetMethod(methodName);
                // 创建一个 Delegate 并绑定到 obj 对象
                return (T)Delegate.CreateDelegate(typeof(T), script, methodInfo);
            }
            throw new Exception("not found script.");
        }





        public object GetVariable(string scriptName, string variableName)
        {
            BaseScript script = scriptCollection.NameOf(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                var instance = Activator.CreateInstance(type);
                var field = type.GetField(variableName);
                return field?.GetValue(instance);
            }

            throw new Exception("Variable not found");
        }

        public void SetVariable(string scriptName, string variableName, object value)
        {
            BaseScript script = scriptCollection.NameOf(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                var instance = Activator.CreateInstance(type);
                var field = type.GetField(variableName);
                field?.SetValue(instance, value);
            }
        }

        public void Dispose()
        {
            this.scriptCollection.Clear();
        }
    }
}
