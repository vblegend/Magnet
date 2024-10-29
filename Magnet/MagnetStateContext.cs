using Magnet.Core;
using Magnet.Tracker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;



namespace Magnet
{
    internal class MagnetStateContext : IStateContext, IDisposable
    {
#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private MagnetScript _engine;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private TrackerColllection _referenceTrackers;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        public Dictionary<String, Delegate> _delegateCache;

#if RELEASE
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal readonly List<IObjectProvider> _providers;

#if RELEASE
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private readonly List<IScriptInstance> _cache;

#if RELEASE
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private readonly Dictionary<Type, ScriptCachedItem> _cacheByType;

#if RELEASE
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private readonly Dictionary<String, ScriptCachedItem> _cacheByString;



        internal MagnetStateContext(MagnetScript engine, StateOptions stateOptions)
        {
            this._engine = engine;
            this._providers = engine.Options.Providers;

            var count = engine.scriptMetaTable.Count;
            _cache = new List<IScriptInstance>(count);
            _cacheByType = new Dictionary<Type, ScriptCachedItem>(count);
            _cacheByString = new Dictionary<String, ScriptCachedItem>(count);


            if (stateOptions.Providers.Count > 0)
            {
                this._providers = new List<IObjectProvider>(engine.Options.Providers);
                foreach (var item in stateOptions.Providers)
                {
                    this.RegisterProviderInternal(item.Type, item.Value, item.SlotName);
                }
            }
            this._referenceTrackers = engine.ReferenceTrackers;
        }

        public IOutput Output => _engine?.Options?.Output;


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
            var _object = new ObjectProvider(objectType, value, slotName);
            if (String.IsNullOrWhiteSpace(slotName))
            {
                _providers.Add(_object);
            }
            else
            {
                _providers.Insert(0, _object);
            }
        }



        internal void Autowired(IReadOnlyDictionary<Type, Object> objectMap)
        {
            foreach (var pair in _cacheByType)
            {
                var instance = pair.Value;
                foreach (var field in instance.Metadata.AutowriredTable)
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
                var script = pair.Value;
                foreach (var field in script.Metadata.AutowriredTable)
                {
                    if (!field.FieldInfo.IsStatic && valType == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(valType))
                    {
                        if (slotName == null || slotName == field.SlotName)
                        {
                            field.FieldInfo.SetValue(script, @object);
                        }
                    }
                }
            }
        }



        internal void Autowired(AbstractScript instance, ScriptMeta metadata)
        {
            for (int i = 0; i < metadata.AutowriredTable.Count; i++)
            {
                var field = metadata.AutowriredTable[i];
                if (field.FieldInfo.IsStatic && field.IsFilled) continue;
                for (int j = 0; j < this._providers.Count; j++)
                {
                    var item = this._providers[j];
                    if (item.Type == field.FieldInfo.FieldType || field.FieldInfo.FieldType.IsAssignableFrom(item.Type))
                    {
                        if (field.SlotName == null || field.SlotName == item.SlotName)
                        {
                            if (field.FieldInfo.IsStatic)
                            {
                                field.FieldInfo.SetValue(null, item.Value);
                                field.IsFilled = true;
                            }
                            else
                            {
                                field.FieldInfo.SetValue(instance, item.Value);
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
                if (instance.Metadata.ExportMethodTable.TryGetValue(methodName, out var result))
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
                        return provider.Value as T;
                    }
                }
            }
            return null;
        }






        #region Cache

        internal readonly struct ScriptCachedItem
        {
            public ScriptCachedItem(AbstractScript instance, ScriptMeta metadata)
            {
                this.Instance = instance;
                this.Metadata = metadata;
            }
            public readonly AbstractScript Instance;
            public readonly ScriptMeta Metadata;
        }

        internal void AddInstance(ScriptMeta meta, AbstractScript script)
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

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal IReadOnlyList<IScriptInstance> Instances => _cache;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal IEnumerable<ScriptCachedItem> Instances2 => _cacheByString.Values;


        #endregion




    }
}
