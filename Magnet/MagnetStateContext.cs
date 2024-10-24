using Magnet.Core;
using Magnet.Tracker;
using System;
using System.Collections.Generic;
using System.Reflection;



namespace Magnet
{
    internal class MagnetStateContext : IStateContext, IDisposable
    {

        private MagnetScript engine;

        private TrackerColllection ReferenceTrackers;

        public Dictionary<String, Delegate> DelegateCache;

        private List<ObjectProvider> Providers;


        internal MagnetStateContext(MagnetScript engine, StateOptions stateOptions)
        {
            this.engine = engine;
            this.Providers = engine.Options.Providers;

            if (stateOptions.Providers.Count > 0)
            {
                this.Providers = new List<ObjectProvider>(engine.Options.Providers);
                foreach (var item in stateOptions.Providers)
                {
                    this.RegisterProviderInternal(item.Type, item.Instance, item.SlotName);
                }
            }
            this.ReferenceTrackers = engine.ReferenceTrackers;
        }

        public IOutput Output => engine?.Options?.Output;

        public ScriptRunMode RunMode => engine.Options.Mode;

        public void Dispose()
        {
            this.engine = null;
            foreach (var instance in this.instances)
            {
                instance.UnInitialize();
            }
            this.instances.Clear();
            this.instancesByType.Clear();
            this.instancesByString.Clear();
            this.ReferenceTrackers = null;
        }

        #region Autowired

        /// <summary>
        /// Register the State private Providers
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="value"></param>
        /// <param name="slotName"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void RegisterProviderInternal(Type objectType, Object value, String slotName = null)
        {
            var type = objectType;
            foreach (var item in Providers)
            {
                if (((slotName == null && item.SlotName == null) || (slotName == item.SlotName)) && (Object)value == item.Instance) throw new InvalidOperationException();
            }
            var _object = new ObjectProvider() { Type = type, Instance = value, SlotName = slotName };
            if (String.IsNullOrWhiteSpace(slotName))
            {
                Providers.Add(_object);
            }
            else
            {
                Providers.Insert(0, _object);
            }
        }

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



        public void Autowired(Type instanceType, AbstractScript instance)
        {
            if (instancesByType.TryGetValue(instanceType, out var scriptInstance))
            {
                foreach (var item in this.Providers)
                {
                    foreach (var field in scriptInstance.Metadata.AutowriredFields)
                    {
                        if (item.Type == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(item.Type))
                        {
                            if (field.SlotName == null || field.SlotName == item.SlotName)
                            {
                                if (field.FieldInfo.IsStatic)
                                {
                                    field.FieldInfo.SetValue(null, item.Instance);
                                }
                                else
                                {
                                    field.FieldInfo.SetValue(instance, item.Instance);
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion


        #region Method



        public T GetScriptMethod<T>(String scriptName, String methodName) where T : Delegate
        {
            var key = scriptName + "." + methodName;
            if (DelegateCache == null) DelegateCache = new Dictionary<String, Delegate>();
            Delegate _delegate = null;
            if (this.DelegateCache.TryGetValue(key, out _delegate))
            {
                return (T)_delegate;
            }

            if (instancesByString.TryGetValue(scriptName, out ScriptInstance instance))
            {
                if (instance.Metadata.ExportMethods.TryGetValue(methodName, out var result))
                {
                    _delegate = Delegate.CreateDelegate(typeof(T), instance.Instance, result.MethodInfo);
                    this.DelegateCache.TryAdd(key, _delegate);
                    this.ReferenceTrackers.Add(_delegate);
                    return (T)_delegate;
                }
            }
            return null;
        }
        #endregion



        #region Property




        public Getter<T> GetScriptPropertyGetter<T>(String scriptName, String propertyName)
        {
            var key = $"$get_{scriptName}.{propertyName}";
            if (DelegateCache == null) DelegateCache = new Dictionary<String, Delegate>();
            Delegate _delegate = null;
            if (this.DelegateCache.TryGetValue(key, out _delegate))
            {
                return (Getter<T>)_delegate;
            }
            AbstractScript script = this.InstanceOfName(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                PropertyInfo propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var getter = (Getter<T>)Delegate.CreateDelegate(typeof(Getter<T>), script, propertyInfo.GetMethod);
                    this.DelegateCache.Add(key, getter);
                    ReferenceTrackers.Add(getter);
                    return getter;
                }

            }
            return null;
        }


        public Setter<T> GetScriptPropertySetter<T>(String scriptName, String propertyName) 
        {
            var key = $"$set_{scriptName}.{propertyName}";
            if (DelegateCache == null) DelegateCache = new Dictionary<String, Delegate>();
            Delegate _delegate = null;
            if (this.DelegateCache.TryGetValue(key, out _delegate))
            {
                return (Setter<T>)_delegate;
            }
            AbstractScript script = this.InstanceOfName(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                PropertyInfo propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var setter = (Setter<T>)Delegate.CreateDelegate(typeof(Setter<T>), script, propertyInfo.SetMethod);
                    DelegateCache.Add(key, setter);
                    ReferenceTrackers.Add(setter);
                    return setter;
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



        public T GetProvider<T>(string providerName = null) where T : class
        {
            var typed = typeof(T);
            foreach (var provider in this.Providers)
            {
                if (typed == provider.Type || typed.IsAssignableFrom(provider.Type))
                {
                    if (String.IsNullOrEmpty(providerName) || provider.SlotName == providerName)
                    {
                        return provider.Instance as T;
                    }
                }
            }
            return null;
        }






        #region Instances

        internal struct ScriptInstance
        {
            public ScriptInstance(AbstractScript instance, ScriptMetadata metadata)
            {
                this.Instance = instance;
                this.Metadata = metadata;
            }
            public AbstractScript Instance;
            public ScriptMetadata Metadata;
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
            this.ReferenceTrackers.Add(script);
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


        internal IReadOnlyList<IScriptInstance> Instances => instances;
        internal IEnumerable<ScriptInstance> Instances2 => instancesByString.Values;


        #endregion




    }
}
