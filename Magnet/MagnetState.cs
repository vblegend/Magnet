using Magnet.Context;
using System.Reflection;


namespace Magnet
{
    public class MagnetState
    {
        private Assembly assembly = null;
        private ScriptCollection scriptCollection = new ScriptCollection();


        internal MagnetState(ScriptCollection scriptCollection)
        {
            this.scriptCollection = scriptCollection;
            foreach (var item in this.scriptCollection.scripts)
            {
                if (item is IScriptInstance scriptInstance)
                {
                    scriptInstance.Initialize();
                }
            }
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

    }
}
