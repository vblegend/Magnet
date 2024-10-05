﻿using Magnet.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;


namespace Magnet
{
    internal class MagnetStateContext : IStateContext, IDisposable
    {

        private MagnetScript engine;



        internal MagnetStateContext(MagnetScript engine)
        {
            this.engine = engine;
        }


        public ScriptRunMode RunMode => engine.Options.Mode;

        public bool UseDebuggerBreak => engine.Options.UseDebugger;

        public void Dispose()
        {
            this.engine = null;
            this.instances.Clear();
            this.instancesByType.Clear();
            this.instancesByString.Clear();
        }




        #region Autowired

        public void Autowired<TObject>(AbstractScript instance, TObject @object)
        {
            var instanceType = instance.GetType();
            if (instancesByType.TryGetValue(instanceType, out var scriptInstance))
            {
                var valType = typeof(TObject);
                foreach (var field in scriptInstance.Metadata.AutowriredFields)
                {
                    // field.RequiredType
                    // field.Alias
                    if (valType == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(valType))
                    {
                        field.FieldInfo.SetValue(instance, @object);
                    }
                }
            }
        }

        public void Autowired(Type instanceType, AbstractScript instance, IReadOnlyDictionary<Type, Object> objectMap)
        {
            if (instancesByType.TryGetValue(instanceType, out var scriptInstance))
            {
                foreach (var field in scriptInstance.Metadata.AutowriredFields)
                {
                    foreach (var obj in objectMap)
                    {  
                        // field.RequiredType
                        // field.Alias
                        if (obj.Key == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(obj.Key))
                        {
                            field.FieldInfo.SetValue(instance, obj.Value);
                        }
                    }
                }
            }
        }
        #endregion


        #region Method
        public T GetScriptMethod<T>(String scriptName, String methodName) where T : Delegate
        {
            if (instancesByString.TryGetValue(scriptName, out ScriptInstance instance))
            {
                if (instance.Metadata.ExportMethods.TryGetValue(methodName, out var result))
                {
                    return (T)instance.DelegateCache.GetOrAdd(result.MethodInfo, (method) =>
                    {
                        return Delegate.CreateDelegate(typeof(T), instance.Instance, method);
                    });
                }
            }
            return null;
        }
        #endregion











        #region Instances

        private struct ScriptInstance
        {
            public ScriptInstance(AbstractScript instance, ScriptMetadata metadata)
            {
                this.Instance = instance;
                this.Metadata = metadata;
                this.DelegateCache = new ConcurrentDictionary<MethodInfo, Delegate>();
            }
            public AbstractScript Instance;
            public ScriptMetadata Metadata;
            public ConcurrentDictionary<MethodInfo, Delegate> DelegateCache;

        }



        private List<IScriptInstance> instances = new List<IScriptInstance>();
        private Dictionary<Type, ScriptInstance> instancesByType = new Dictionary<Type, ScriptInstance>();
        private Dictionary<String, ScriptInstance> instancesByString = new Dictionary<String, ScriptInstance>();

        internal void AddInstance(ScriptMetadata meta, AbstractScript script)
        {
            var metadata = new ScriptInstance(script, meta);
            instances.Add(script);
            instancesByType.Add(meta.ScriptType, metadata);
            instancesByString.Add(meta.ScriptAlias, metadata);
        }
        public AbstractScript InstanceOfName(string scriptName)
        {
            instancesByString.TryGetValue(scriptName, out ScriptInstance instance);
            return instance.Instance;
        }

        public T InstanceOfType<T>() where T : AbstractScript
        {
            instancesByType.TryGetValue(typeof(T), out ScriptInstance instance);
            return (T)instance.Instance;
        }

        public AbstractScript InstanceOfType(Type type)
        {
            instancesByType.TryGetValue(type, out ScriptInstance instance);
            return instance.Instance;
        }

        public IReadOnlyList<IScriptInstance> Instances => instances;

        #endregion




    }
}
