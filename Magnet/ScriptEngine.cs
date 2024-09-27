using Magnet.Context;
using System.Reflection;


namespace Magnet
{
    public class ScriptEngine : IDisposable
    {
        private Assembly assembly = null;
        private ScriptCollection scriptCollection = new ScriptCollection();
        private ScriptLoadContext scriptLoadContext;


        internal ScriptEngine(ScriptLoadContext scriptLoadContext, Assembly assembly)
        {
            this.scriptLoadContext = scriptLoadContext;
            this.assembly = assembly;
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


        public void Initialize()
        {
            var types = this.assembly.GetTypes();
            var baseType = typeof(BaseScript);
            foreach (Type type in types)
            {
                if (type.IsPublic && !type.IsAbstract && type.IsSubclassOf(baseType))
                {
                    var script = type.GetCustomAttribute<ScriptAttribute>();
                    if (script != null)
                    {
                        if (String.IsNullOrEmpty(script.Name))
                        {
                            script.Name = type.Name;
                        }
                        var instance = (BaseScript)Activator.CreateInstance(type);
                        InjectionParameter(instance);
                        scriptCollection.Add(script,instance);
                    }
                }
           }
            // script 对象 初始化
            ScriptInitialize();
        }


        private void ScriptInitialize()
        {
            var type = typeof(BaseScript);
            var method = type.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var item in this.scriptCollection.scripts)
            {
                method.Invoke(item, null);
            }
        }


        private void InjectionParameter(BaseScript instance)
        {
            var type = typeof(BaseScript);
            var field= type.GetField("scriptCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(instance, scriptCollection);
        }





        public void Dispose()
        {
            if (this.scriptLoadContext != null)
            {
                this.scriptLoadContext.Unload();
                this.scriptLoadContext = null;
                this.assembly = null;
            }
        }



    }
}
