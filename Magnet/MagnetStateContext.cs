using Magnet.Core;
using Magnet.Tracker;

using System;
using System.Collections.Generic;

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
        internal List<ObjectProvider> _providers;

#if RELEASE
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private List<IScriptInstance> _cache;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal IReadOnlyList<IScriptInstance> Instances => _cache;



        internal MagnetStateContext(MagnetScript engine, StateOptions stateOptions)
        {
            this._engine = engine;
            var count = engine.scriptMetaTables.Count;
            _cache = new List<IScriptInstance>(count);
            // 计算好容量 防止扩容
            var length = engine.Options.Providers.Count + stateOptions.Providers.Count + count;

            this._providers = new List<ObjectProvider>(length);
            this._providers.AddRange(engine.Options.Providers);

            if (stateOptions.Providers.Count > 0)
            {
                foreach (var item in stateOptions.Providers)
                {
                    this.RegisterProviderInternal(item.TargetType, item.ValueType, item.Value, item.SlotName);
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
            this._cache = null;
            this._delegateCache = null;
            this._providers = null;
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
            var _object = new ObjectProvider(null, objectType, value, slotName);
            if (String.IsNullOrWhiteSpace(slotName))
            {
                _providers.Add(_object);
            }
            else
            {
                _providers.Insert(0, _object);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetType">The qualified target type of the object to which the field belongs</param>
        /// <param name="valueType"></param>
        /// <param name="value"></param>
        /// <param name="slotName"></param>
        internal void RegisterProviderInternal(Type targetType, Type valueType, Object value, String slotName = null)
        {
            var _object = new ObjectProvider(targetType, valueType, value, slotName);
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
            foreach (var instance in _cache)
            {
                var scriptInstance = instance as AbstractScript;
                var metaTable = scriptInstance.MetaTable;
                foreach (var field in metaTable.AutowriredTables)
                {
                    var target = scriptInstance;
                    if (field.IsStatic)
                    {
                        if (field.IsFilled) continue;
                        target = null;
                    }
                    foreach (var obj in objectMap)
                    {
                        if ((field.RequiredType == null || obj.Key == field.RequiredType) && obj.Key == field.FieldType || field.FieldType.IsAssignableFrom(obj.Key))
                        {
                            field.Setter(target, obj.Value);
                            if (field.IsStatic) field.IsFilled = true;
                            break;
                        }
                    }
                }
            }
        }

        internal void Autowired<TObject>(Type targetType, TObject @object, String slotName = null)
        {
            var valueType = typeof(TObject);
            foreach (var instance in _cache)
            {
                var scriptInstance = instance as AbstractScript;
                var metaTable = scriptInstance.MetaTable;
                foreach (var field in metaTable.AutowriredTables)
                {
                    var target = scriptInstance;
                    if (field.IsStatic)
                    {
                        if (field.IsFilled) continue;
                        target = null;
                    }
                    if ((field.RequiredType == null || valueType == field.RequiredType) &&            // Autowrired 限定字段类型
                        (field.SlotName == null || field.SlotName == slotName) &&                    // Provider 限定了槽名字
                        (valueType == field.FieldType || field.FieldType.IsAssignableFrom(valueType)))  // 字段类型相同的// 继承的
                    {
                        field.Setter(target, @object);
                        if (field.IsStatic) field.IsFilled = true;
                        break;
                    }
                }
            }
        }



        internal void Autowired(AbstractScript instance)
        {
            var metaTable = instance.MetaTable;
            for (int i = 0; i < metaTable.AutowriredTables.Count; i++)
            {
                var field = metaTable.AutowriredTables[i];
                var target = instance;
                if (field.IsStatic)
                {
                    if (field.IsFilled) continue;
                    target = null;
                }
                for (int j = 0; j < this._providers.Count; j++)
                {
                    var item = this._providers[j];
                    if ((item.TargetType == null || item.TargetType == field.DeclaringType) &&                  // Provider 限定目标类型
                        (field.RequiredType == null || item.ValueType == field.RequiredType) &&                 // Autowrired 限定字段类型
                        (field.SlotName == null || field.SlotName == item.SlotName) &&                         // Provider 限定了槽名字
                        (item.ValueType == field.FieldType || field.FieldType.IsAssignableFrom(item.ValueType)))  // 字段类型相同的// 继承的
                    {
                        field.Setter(target, item.Value);
                        if (field.IsStatic) field.IsFilled = true;
                        break;

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
            var instance = this.NameAs<AbstractScript>(scriptName);
            if (instance != null)
            {
                if (instance.MetaTable.ExportMethods.TryGetValue(methodName, out var result))
                {
                    _delegate = Delegate.CreateDelegate(typeof(T), instance, result.MethodInfo);
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
            AbstractScript script = this.NameAs<AbstractScript>(scriptName);
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
            AbstractScript script = this.NameAs<AbstractScript>(scriptName);
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


        #region IStateContext 


        public T FirstAs<T>() where T : class
        {
            foreach (var instance in _cache)
            {
                if (instance is T tt) return tt;
            }
            return null;
        }



        public T FirstAs<T>(Type type) where T : AbstractScript
        {
            foreach (AbstractScript instance in _cache)
            {
                if (instance.MetaTable.Type == type && instance is T tt) return tt;
            }
            return null;
        }


        public IEnumerable<T> TypeOf<T>() where T : class
        {
            foreach (var instance in _cache)
            {
                if (instance is T tt) yield return tt;
            }
        }

        public T NameAs<T>(String scriptName) where T : class
        {
            foreach (AbstractScript instance in _cache)
            {
                if (instance is T tt && instance.MetaTable.Alias == scriptName) return tt;
            }
            return null;
        }
        #endregion



        public T GetProvider<T>(string providerName = null) where T : class
        {
            var typed = typeof(T);
            foreach (var provider in this._providers)
            {
                if (typed == provider.ValueType || typed.IsAssignableFrom(provider.ValueType))
                {
                    if (String.IsNullOrEmpty(providerName) || provider.SlotName == providerName)
                    {
                        return provider.Value as T;
                    }
                }
            }
            return null;
        }






        #region AddCache   
        internal void AddInstance(AbstractScript script)
        {
            _cache.Add(script);
            this._referenceTrackers.Add(script);
        }

        #endregion

    }
}
