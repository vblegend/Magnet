using Magnet.Core;
using Magnet.Tracker;
using System;
using System.Collections.Generic;
using System.Reflection;



namespace Magnet
{
    internal class MagnetStateContext : IStateContext, IDisposable
    {

        private MagnetScript _engine;

        private TrackerColllection _referenceTrackers;

        public Dictionary<String, Delegate> _delegateCache;

        private List<ObjectProvider> _providers;


        internal MagnetStateContext(MagnetScript engine, StateOptions stateOptions)
        {
            this._engine = engine;
            this._providers = engine.Options.Providers;

            if (stateOptions.Providers.Count > 0)
            {
                this._providers = new List<ObjectProvider>(engine.Options.Providers);
                foreach (var item in stateOptions.Providers)
                {
                    this.RegisterProviderInternal(item.Type, item.Instance, item.SlotName);
                }
            }
            this._referenceTrackers = engine.ReferenceTrackers;
        }

        public IOutput Output => _engine?.Options?.Output;

        public ScriptRunMode RunMode => _engine.Options.Mode;

        public void Dispose()
        {
            this._engine = null;
            foreach (var instance in this._cache)
            {
                instance.Shutdown();
            }
            this._cache.Clear();
            this._cacheByType.Clear();
            this._cacheByString.Clear();
            this._referenceTrackers = null;
        }

        #region Autowired

        /// <summary>
        /// Register the State private Providers
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="value"></param>
        /// <param name="slotName"></param>
        /// <exception cref="InvalidOperationException"></exception>
        internal void RegisterProviderInternal(Type objectType, Object value, String slotName = null)
        {
            var _object = new ObjectProvider() { Type = objectType, Instance = value, SlotName = slotName };
            if (String.IsNullOrWhiteSpace(slotName))
            {
                _providers.Add(_object);
            }
            else
            {
                _providers.Insert(0, _object);
            }
        }

        internal void Autowired<TObject>(AbstractScript instance, TObject @object, String slotName = null)
        {
            var instanceType = instance.GetType();
            if (_cacheByType.TryGetValue(instanceType, out var scriptInstance))
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

        internal void Autowired(IReadOnlyDictionary<Type, Object> objectMap)
        {
            foreach (var pair in _cacheByType)
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

        internal void Autowired<TObject>(TObject @object, String slotName = null)
        {
            var valType = typeof(TObject);
            foreach (var pair in _cacheByType)
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



        internal void Autowired( AbstractScript instance, ScriptMetadata metadata)
        {
            foreach (var item in this._providers)
            {
                foreach (var field in metadata.AutowriredFields)
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
        #endregion


        #region Method



        public T GetScriptMethod<T>(String scriptName, String methodName) where T : Delegate
        {
            var key = scriptName + "." + methodName;
            if (_delegateCache == null) _delegateCache = new Dictionary<String, Delegate>();
            Delegate _delegate = null;
            if (this._delegateCache.TryGetValue(key, out _delegate))
            {
                return (T)_delegate;
            }

            if (_cacheByString.TryGetValue(scriptName, out ScriptCachedItem instance))
            {
                if (instance.Metadata.ExportMethods.TryGetValue(methodName, out var result))
                {
                    _delegate = Delegate.CreateDelegate(typeof(T), instance.Instance, result.MethodInfo);
                    this._delegateCache.TryAdd(key, _delegate);
                    this._referenceTrackers.Add(_delegate);
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
            if (_delegateCache == null) _delegateCache = new Dictionary<String, Delegate>();
            Delegate _delegate = null;
            if (this._delegateCache.TryGetValue(key, out _delegate))
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
                    this._delegateCache.Add(key, getter);
                    _referenceTrackers.Add(getter);
                    return getter;
                }

            }
            return null;
        }


        public Setter<T> GetScriptPropertySetter<T>(String scriptName, String propertyName)
        {
            var key = $"$set_{scriptName}.{propertyName}";
            if (_delegateCache == null) _delegateCache = new Dictionary<String, Delegate>();
            Delegate _delegate = null;
            if (this._delegateCache.TryGetValue(key, out _delegate))
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
                    _delegateCache.Add(key, setter);
                    _referenceTrackers.Add(setter);
                    return setter;
                }
            }
            return null;
        }
        #endregion





        #region Script 


        public T ScriptAs<T>() where T : class
        {
            foreach (var instance in _cache)
            {
                if (instance is T tt) return tt;
            }
            return null;
        }


        public T ScriptAs<T>(String scriptName) where T : class
        {
            if (_cacheByString.TryGetValue(scriptName, out ScriptCachedItem instance))
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
            foreach (var provider in this._providers)
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






        #region Cache

        internal readonly struct ScriptCachedItem
        {
            public ScriptCachedItem(AbstractScript instance, ScriptMetadata metadata)
            {
                this.Instance = instance;
                this.Metadata = metadata;
            }
            public readonly AbstractScript Instance;
            public readonly ScriptMetadata Metadata;
        }



        private List<IScriptInstance> _cache = new List<IScriptInstance>();
        private Dictionary<Type, ScriptCachedItem> _cacheByType = new Dictionary<Type, ScriptCachedItem>();
        private Dictionary<String, ScriptCachedItem> _cacheByString = new Dictionary<String, ScriptCachedItem>();

        internal void AddInstance(ScriptMetadata meta, AbstractScript script)
        {
            var metadata = new ScriptCachedItem(script, meta);
            _cache.Add(script);
            _cacheByType.Add(meta.ScriptType, metadata);
            _cacheByString.Add(meta.ScriptAlias, metadata);
            this._referenceTrackers.Add(script);
        }
        public AbstractScript InstanceOfName(string scriptName)
        {
            _cacheByString.TryGetValue(scriptName, out ScriptCachedItem instance);
            return instance.Instance;
        }

        public T InstanceOfType<T>() where T : AbstractScript
        {
            _cacheByType.TryGetValue(typeof(T), out ScriptCachedItem instance);
            return (T)instance.Instance;
        }

        public AbstractScript InstanceOfType(Type type)
        {
            _cacheByType.TryGetValue(type, out ScriptCachedItem instance);
            return instance.Instance;
        }


        internal IReadOnlyList<IScriptInstance> Instances => _cache;

        internal IEnumerable<ScriptCachedItem> Instances2 => _cacheByString.Values;


        #endregion




    }
}
