using Magnet.Core;
using Magnet.Tracker;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;



namespace Magnet
{

    [StructLayout(LayoutKind.Explicit)]
    internal class InstanceCacheItem
    {
        public InstanceCacheItem(AbstractScript instance, ReferenceTracker weakReference)
        {
            this.Instance = instance;
            this.WeakReference = weakReference;
        }

        [FieldOffset(0)]
        public readonly AbstractScript Instance;

        [FieldOffset(0)]
        public readonly IScriptInstance Interface;

        [FieldOffset(8)]
        public readonly ReferenceTracker WeakReference;
    }



    internal class DelegateCacheItem
    {
        internal DelegateCacheItem(Delegate _delegate, ReferenceTracker weakReference)
        {
            this.Delegate = _delegate;
            this.WeakReference = weakReference;
        }


        public readonly Delegate Delegate;

        public readonly ReferenceTracker WeakReference;
    }




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
        private Dictionary<String, DelegateCacheItem> _delegateCache;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        private InstanceCacheItem[] _cache;

        private Int32 _cacheLength = 0;

#if RELEASE
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        internal IReadOnlyList<InstanceCacheItem> Cache => _cache;

        internal MagnetStateContext(MagnetScript engine, StateOptions stateOptions)
        {
            this._engine = engine;
            var count = engine.ScriptMetaTables.Count;
            _cache = new InstanceCacheItem[count];
            this._referenceTrackers = engine.ReferenceTrackers;
        }

        public IOutput Output => _engine?.Options?.Output;


        public void Dispose()
        {
            this._engine = null;

            for (int i = 0; i < _cacheLength; i++)
            {
                var script = (IScriptInstance)this._cache[i].Instance;
                script.Shutdown();
                this._cache[i] = null;
            }
            this._cacheLength = 0;
            this._cache = null;
            this._delegateCache = null;
            this._referenceTrackers = null;
        }

        #region Autowired

        internal void Autowired(IReadOnlyDictionary<Type, Object> objectMap)
        {
            foreach (var instance in _cache)
            {
                var scriptInstance = instance.Instance;
                var metaTable = scriptInstance.MetaTable;
                for (int i = 0; i < metaTable.AutowriredTables.Length; i++)
                {
                    var field = metaTable.AutowriredTables[i];
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
                            if (field.IsStatic)
                            {
                                field.IsFilled = true;
                                metaTable.AutowriredTables[i] = field;
                            }
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
                var scriptInstance = instance.Instance;
                var metaTable = scriptInstance.MetaTable;
                for (int i = 0; i < metaTable.AutowriredTables.Length; i++)
                {
                    var field = metaTable.AutowriredTables[i];
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
                        if (field.IsStatic)
                        {
                            field.IsFilled = true;
                            metaTable.AutowriredTables[i] = field;
                        }
                        break;
                    }
                }
            }
        }

        #endregion


        #region Method
        public ReferenceTracker<T> GetScriptMethod<T>(String scriptName, String methodName) where T : Delegate
        {
            var key = scriptName + "." + methodName;
            if (_delegateCache == null) _delegateCache = new Dictionary<String, DelegateCacheItem>();
            DelegateCacheItem _delegate = null;
            if (this._delegateCache.TryGetValue(key, out _delegate))
            {
                return _delegate.WeakReference.As<T>();
            }
            var instance = this.NameAs<AbstractScript>(scriptName);
            if (instance != null)
            {
                if (instance.MetaTable.ExportMethods.TryGetValue(methodName, out var result))
                {
                    var methodDelgate = TypeUtils.CreateMethodDelegate<T>(result.MethodInfo, instance);
                    // var methodDelgate = Delegate.CreateDelegate(typeof(T), instance, result.MethodInfo);
                    var weak = new ReferenceTracker(methodDelgate);
                    _delegate = new DelegateCacheItem(methodDelgate, weak);

                    this._delegateCache.TryAdd(key, _delegate);
                    this._referenceTrackers.AddTracker(weak);
                    return weak.As<T>();
                }
            }
            return null;
        }

        #endregion


        #region Property

        public ReferenceTracker<Getter<T>> CreateGetterTracker<T>(String scriptName, String propertyName)
        {
            var key = $"$get_{scriptName}.{propertyName}";
            if (_delegateCache == null) _delegateCache = new Dictionary<String, DelegateCacheItem>();
            DelegateCacheItem _delegate = null;
            if (this._delegateCache.TryGetValue(key, out _delegate))
            {
                return _delegate.WeakReference.As<Getter<T>>();
            }
            AbstractScript script = this.NameAs<AbstractScript>(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                PropertyInfo propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var getter = (Getter<T>)Delegate.CreateDelegate(typeof(Getter<T>), script, propertyInfo.GetMethod);
                    var weak = new ReferenceTracker(getter);
                    _delegate = new DelegateCacheItem(getter, weak);
                    this._delegateCache.Add(key, _delegate);
                    _referenceTrackers.AddTracker(weak);
                    return weak.As<Getter<T>>();
                }
            }
            return null;
        }

        public ReferenceTracker<Setter<T>> CreateSetterTracker<T>(String scriptName, String propertyName)
        {
            var key = $"$set_{scriptName}.{propertyName}";
            if (_delegateCache == null) _delegateCache = new Dictionary<String, DelegateCacheItem>();
            DelegateCacheItem _delegate = null;
            if (this._delegateCache.TryGetValue(key, out _delegate))
            {
                return _delegate.WeakReference.As<Setter<T>>();
            }
            AbstractScript script = this.NameAs<AbstractScript>(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                PropertyInfo propertyInfo = type.GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    var setter = (Setter<T>)Delegate.CreateDelegate(typeof(Setter<T>), script, propertyInfo.SetMethod);
                    var weak = new ReferenceTracker(setter);
                    _delegate = new DelegateCacheItem(setter, weak);
                    _delegateCache.Add(key, _delegate);
                    _referenceTrackers.AddTracker(weak);
                    return weak.As<Setter<T>>();
                }
            }
            return null;
        }

        #endregion


        #region IStateContext 


        public T FirstAs<T>() where T : class
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                var tObject = _cache[i].Instance as T;
                if (tObject != null) return tObject;
            }
            return null;
        }



        public ReferenceTracker<T> FirstAsTracker<T>() where T : class
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                if (_cache[i].Instance as T != null) return _cache[i].WeakReference.As<T>();
            }
            return null;
        }


        public T FirstAs<T>(Type type) where T : AbstractScript
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                var instance = _cache[i].Instance;
                if (ReferenceEquals(instance.MetaTable.Type, type) && instance is T tObject) return tObject;
            }
            return null;
        }


        public ReferenceTracker<T> FirstAsTracker<T>(Type type) where T : AbstractScript
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                var instance = _cache[i].Instance;
                if (ReferenceEquals(instance.MetaTable.Type, type) && instance as T != null) return _cache[i].WeakReference.As<T>();
            }
            return null;
        }




        public IEnumerable<T> TypeOf<T>() where T : class
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                var tObject = _cache[i].Instance as T;
                if (tObject != null) yield return tObject;
            }
        }


        public IEnumerable<ReferenceTracker<T>> TypeOfTracker<T>() where T : class
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                if (_cache[i].Instance as T != null) yield return _cache[i].WeakReference.As<T>();
            }
        }


        public T NameAs<T>(String scriptName) where T : class
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                var instance = _cache[i].Instance;
                if (instance.MetaTable.Alias == scriptName && instance is T tObject) return tObject;
            }
            return null;
        }

        public ReferenceTracker<T> NameAsTracker<T>(String scriptName) where T : class
        {
            for (int i = 0; i < _cache.Length; i++)
            {
                var instance = _cache[i].Instance;
                if (instance.MetaTable.Alias == scriptName && instance as T != null) return _cache[i].WeakReference.As<T>();
            }
            return null;
        }


        #endregion


        #region AddCache   
        internal void AddInstance(AbstractScript script)
        {
            var weak = new ReferenceTracker(script);
            var cacheItem = new InstanceCacheItem(script, weak);
            _cache[_cacheLength] = cacheItem;
            _cacheLength++;
            this._referenceTrackers.AddTracker(weak);
        }

        #endregion

    }
}
