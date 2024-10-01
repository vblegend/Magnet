using Magnet.Core;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Magnet
{
    public class MagnetState : IDisposable
    {
        private MagnetScript engine;
        private MagnetStateContext stateContext;

        internal MagnetState(MagnetScript engine)
        {
            this.engine = engine;
            this.stateContext = new MagnetStateContext(engine);
            this.CreateState();
        }



        private void CreateState()
        {
            foreach (var meta in this.engine.scriptMetaInfos)
            {
                var instance = (BaseScript)Activator.CreateInstance(meta.Type);
                this.stateContext.AddInstance(meta.Attribute, instance);
                this.Autowired(instance, engine.Options.InjectedObjectMap);
            }
            // Injected Data
            foreach (var instance in this.stateContext.Instances)
            {
                instance.InjectedContext(this.stateContext);
            }
            // Exec Init Function
            foreach (var instance in this.stateContext.Instances)
            {
                instance.Initialize();
            }
        }


        public void Inject<TYPE>(TYPE obj, IReadOnlyDictionary<Type, Object> objectMap)
        {
            foreach (var instance in this.stateContext.Instances)
            {
                this.Autowired(instance as BaseScript, objectMap);
            }
        }


        private void Autowired(BaseScript instance, IReadOnlyDictionary <Type, Object> objectMap)
        {
            var type = instance.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<AutowiredAttribute>();
                if (attribute != null)
                {
                    if (objectMap.TryGetValue(field.FieldType, out Object value))
                    {
                        field.SetValue(instance, value);
                    }
                }
            }
        }


        public T GetDelegate<T>(String scriptName, String methodName) where T : Delegate
        {
            BaseScript script = this.stateContext.InstanceOfName(scriptName);
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
            BaseScript script = this.stateContext.InstanceOfName(scriptName);
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
            BaseScript script = this.stateContext.InstanceOfName(scriptName);
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
            if (this.stateContext != null)
            {
                this.stateContext.Dispose();
                this.stateContext = null;
            }

        }
    }
}
