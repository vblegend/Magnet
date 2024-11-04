using Magnet.Analysis;
using Magnet.Core;
using Magnet.Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


namespace Magnet
{

    /// <summary>
    /// MagnetState option
    /// </summary>
    public class StateOptions
    {
        /// <summary>
        /// Provide a default MagnetState option
        /// </summary>
        public static StateOptions Default => new StateOptions();

        /// <summary>
        /// The unique ID of MagnetState is non-repeatable
        /// </summary>
        public Int64 Identity { get; internal set; } = -1;

        internal List<ObjectProvider> Providers = [];

        /// <summary>
        /// set state Identity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public StateOptions WithIdentity(Int64 identity)
        {
            this.Identity = identity;
            return this;
        }


        /// <summary>
        /// Register the state private provider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="slotName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public StateOptions RegisterProvider<T>(T value, String slotName = null)
        {
            var type = typeof(T);
            foreach (var item in Providers)
            {
                if (((slotName == null && item.SlotName == null) || (slotName == item.SlotName)) && (Object)value == item.Value) throw new InvalidOperationException();
            }
            var _object = new ObjectProvider(null, type, value, slotName);
            if (String.IsNullOrWhiteSpace(slotName))
            {
                Providers.Add(_object);
            }
            else
            {
                Providers.Insert(0, _object);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetType">The qualified target type of the object to which the field belongs</param>
        /// <param name="value"> </param>
        /// <param name="slotName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public StateOptions RegisterProvider<T>(Type targetType, T value, String slotName = null)
        {
            var type = typeof(T);
            foreach (var item in Providers)
            {
                if (((slotName == null && item.SlotName == null) || (slotName == item.SlotName)) && (Object)value == item.Value) throw new InvalidOperationException();
            }
            var _object = new ObjectProvider(targetType, type, value, slotName);
            if (String.IsNullOrWhiteSpace(slotName))
            {
                Providers.Add(_object);
            }
            else
            {
                Providers.Insert(0, _object);
            }
            return this;
        }




    }

    /// <summary>
    /// Attribute get delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public delegate void Setter<T>(T value);


    /// <summary>
    /// Attribute set delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public delegate T Getter<T>();


    /// <summary>
    /// Script state
    /// </summary>
    public interface IMagnetState : IDisposable
    {
        /// <summary>
        /// Batch inject objects into all script instances
        /// </summary>
        /// <param name="objectMap"></param>
        void InjectProvider(IReadOnlyDictionary<Type, Object> objectMap);

        /// <summary>
        /// Inject objects into all script instances
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="targetType">Qualifies the type of the target script, or not if null</param>
        /// <param name="obj"></param>
        /// <param name="slotName">[Autowired] Specifies the slot name</param>
        void InjectProvider<TObject>(Type targetType, TObject obj, String slotName = null);


        /// <summary>
        /// Gets a delegated weak reference to a script method
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="exportMethodName"></param>
        /// <returns></returns>
        IReadOnlyWeakReference<T> CreateDelegate<T>(String scriptName, String exportMethodName) where T : Delegate;


        /// <summary>
        /// Gets a weak reference to the tripartite interface implemented by the script <br/>
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        IReadOnlyWeakReference<T> NameAs<T>(String scriptName) where T : class;


        /// <summary>
        /// Gets weak references to all script objects that implement interface T <br/>
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<IReadOnlyWeakReference<T>> TypeOf<T>() where T : class;

        /// <summary>
        /// Gets a weak reference to a script object whose first type is the parameter type and derived from type T <br/>
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        IReadOnlyWeakReference<T> FirstAs<T>(Type type) where T : AbstractScript;


        /// <summary>
        /// Gets the first weak reference to the script object that implements interface T <br/>
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IReadOnlyWeakReference<T> FirstAs<T>() where T : class;

        /// <summary>
        /// Gets a weak reference to the script's property Getter delegate<br/>
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        IReadOnlyWeakReference<Getter<T>> CreateGetterDelegate<T>(String scriptName, String propertyName);

        /// <summary>
        /// Gets the script's property Setter delegate weak reference
        /// Note: You need to cache weakly referenced objects instead of calling this function frequently, but you can't cache the Target of the weakly referenced object, which will cause the script assembly to fail to unload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        IReadOnlyWeakReference<Setter<T>> CreateSetterDelegate<T>(String scriptName, String propertyName);


        /// <summary>
        /// Get the value of the field in the script (not recommended)
        /// Note: Script Unload is blocked when external references to reference objects inside the script
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        object GetFieldValue(string scriptName, string variableName);


        /// <summary>
        /// Set the value of the field in the script (not recommended)
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        void SetFieldValue(string scriptName, string variableName, object value);
    }



    /// <summary>
    /// Script state
    /// </summary>
    internal sealed class MagnetState : IMagnetState
    {
        private MagnetScript _engine;
        private MagnetStateContext _stateContext;
        private readonly AnalyzerCollection _analyzers;
        /// <summary>
        /// MagnetState Uninstallation or destruction event
        /// </summary>
        public event Action<MagnetState> Unloading;
        /// <summary>
        /// The unique ID of MagnetState is non-repeatable
        /// </summary>
        public readonly Int64 Identity;

        internal MagnetState(MagnetScript engine, StateOptions createStateOptions)
        {
            this._analyzers = engine.Analyzers;
            this.Identity = createStateOptions.Identity;
            this._engine = engine;
            this._stateContext = new MagnetStateContext(engine, createStateOptions);
            using (var injector = new DependencyInjector(_engine, createStateOptions))
            {
                injector.RegisterProviderInternal(typeof(AbstractScript), typeof(IStateContext), this._stateContext);
                this.CreateState(injector);
            }
        }






        private void CreateState(DependencyInjector injector)
        {
            foreach (var meta in this._engine.scriptMetaTables)
            {
                var instance = meta.Generater();
                instance.MetaTable = meta;
                this._stateContext.AddInstance(instance);
                injector.RegisterProviderInternal(meta.Type, instance);
                _analyzers.DefineInstance(meta, instance, _stateContext);
            }
            // Inject && Init 
            foreach (var item in this._stateContext.Cache)
            {
                injector.Autowired(item.Instance);
                var script = (IScriptInstance)item.Instance;
                script.Initialize();
            }
        }




        public void InjectProvider(IReadOnlyDictionary<Type, Object> objectMap)
        {
            this._stateContext.Autowired(objectMap);
        }

        public void InjectProvider<TObject>(Type targetType, TObject obj, String slotName = null)
        {
            this._stateContext.Autowired<TObject>(targetType, obj, slotName);
        }

        public IReadOnlyWeakReference<T> CreateDelegate<T>(String scriptName, String methodName) where T : Delegate
        {
            return this._stateContext.GetScriptMethod<T>(scriptName, methodName);
        }

        public IReadOnlyWeakReference<T> NameAs<T>(String scriptName) where T : class
        {
            return this._stateContext.NameAsTracker<T>(scriptName);
        }

        public IReadOnlyWeakReference<T> FirstAs<T>() where T : class
        {
            return this._stateContext.FirstAsTracker<T>();
        }

        public IReadOnlyWeakReference<T> FirstAs<T>(Type type) where T : AbstractScript
        {
            return this._stateContext.FirstAsTracker<T>(type);
        }

        public IEnumerable<IReadOnlyWeakReference<T>> TypeOf<T>() where T : class
        {
            return this._stateContext.TypeOfTracker<T>();
        }

        public IReadOnlyWeakReference<Getter<T>> CreateGetterDelegate<T>(String scriptName, String propertyName)
        {
            return this._stateContext.CreateGetterTracker<T>(scriptName, propertyName);
        }

        public IReadOnlyWeakReference<Setter<T>> CreateSetterDelegate<T>(String scriptName, String propertyName)
        {
            return this._stateContext.CreateSetterTracker<T>(scriptName, propertyName);
        }


        public object GetFieldValue(string scriptName, string variableName)
        {
            AbstractScript script = this._stateContext.NameAs<AbstractScript>(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                var field = type.GetField(variableName);
                if (field == null) throw new Exception("Field not found");
                return field?.GetValue(script);
            }
            throw new Exception("Field not found");
        }



        public void SetFieldValue(string scriptName, string variableName, object value)
        {
            AbstractScript script = this._stateContext.NameAs<AbstractScript>(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                var field = type.GetField(variableName);
                if (field == null) throw new Exception("Field not found");
                field?.SetValue(script, value);
            }
            throw new Exception("Field not found");
        }



        public void Dispose()
        {
            this.Unloading?.Invoke(this);
            this.Unloading = null;
            this._stateContext?.Dispose();
            this._stateContext = null;
            this._engine = null;
        }


    }
}
