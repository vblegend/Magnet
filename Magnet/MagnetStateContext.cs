﻿using Magnet.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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



        public IOutput Output => engine.Options.Output;

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

        public void Autowired<TObject>(AbstractScript instance, TObject @object, String slotName = null)
        {
            var instanceType = instance.GetType();
            if (instancesByType.TryGetValue(instanceType, out var scriptInstance))
            {
                var valType = typeof(TObject);
                foreach (var field in scriptInstance.Metadata.AutowriredFields)
                {
                    if (valType == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(valType))
                    {
                        if (slotName == null || slotName == field.SlotName)
                        {
                            field.FieldInfo.SetValue(instance, @object);
                        }
                    }
                }
            }
        }




        public void Autowired(IReadOnlyDictionary<Type, Object> objectMap)
        {
            foreach (var pair in instancesByType)
            {
                var instance = pair.Value;
                foreach (var field in instance.Metadata.AutowriredFields)
                {
                    foreach (var obj in objectMap)
                    {
                        if (obj.Key == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(obj.Key))
                        {
                            field.FieldInfo.SetValue(instance, obj.Value);
                            break;
                        }
                    }
                }
            }
        }


        public void Autowired<TObject>(TObject @object, String slotName = null)
        {
            var valType = typeof(TObject);
            foreach (var pair in instancesByType)
            {
                var instance = pair.Value;
                foreach (var field in instance.Metadata.AutowriredFields)
                {
                    if (valType == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(valType))
                    {
                        if (slotName == null || slotName == field.SlotName)
                        {
                            field.FieldInfo.SetValue(instance, @object);
                        }
                    }
                }
            }
        }



        public void Autowired(Type instanceType, AbstractScript instance, IReadOnlyDictionary<Type, List<Objectinstance>> objectMap)
        {
            if (instancesByType.TryGetValue(instanceType, out var scriptInstance))
            {
                var keys = objectMap.Keys.ToArray();
                foreach (var field in scriptInstance.Metadata.AutowriredFields)
                {
                    foreach (var key in keys)
                    {
                        var value = objectMap[key];
                        var ok = false;
                        if (key == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(key))
                        {
                            foreach (var item in value)
                            {
                                if (field.SlotName == null || field.SlotName == item.SlotName)
                                {
                                    field.FieldInfo.SetValue(instance, item.Instance);
                                    ok = true;
                                    break;
                                }
                            }
                        }
                        if (ok) break;
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
                    return instance.GetOrCreateDelegate<T>(result.MethodInfo);
                    //return (T)instance.DelegateCache.GetOrAdd(result.MethodInfo, (method) =>
                    //{
                    //    return Delegate.CreateDelegate(typeof(T), instance.Instance, method);
                    //});
                }
            }
            return null;
        }
        #endregion



        #region Script 


        public T ScriptAs<T>() where T : class
        {
            foreach (var instance in instances)
            {
                if (instance is T tt) return tt;
            }
            return null;
        }


        public T ScriptAs<T>(String scriptName) where T : class
        {
            if (instancesByString.TryGetValue(scriptName, out ScriptInstance instance))
            {
                if (instance.Instance is T tt)
                {
                    return tt;
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
            }
            public AbstractScript Instance;
            public ScriptMetadata Metadata;
            public Dictionary<MethodInfo, Delegate> DelegateCache;

            internal T GetOrCreateDelegate<T>(MethodInfo methodInfo) where T : Delegate
            {
                if (DelegateCache == null) DelegateCache = new Dictionary<MethodInfo, Delegate>();
                if (this.DelegateCache.TryGetValue(methodInfo, out var del))
                {
                    return (T)del;
                }
                else
                {
                    var del2 = Delegate.CreateDelegate(typeof(T), this.Instance, methodInfo);
                    this.DelegateCache.TryAdd(methodInfo, del2);
                    return (T)del2;
                }
            }
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
