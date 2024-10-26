using Magnet.Analysis;
using Magnet.Core;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;


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
        public Int64 Identity { get; set; } = -1;

        internal List<ObjectProvider> Providers = [];

        internal StateOptions WithIdentity(Int64 identity)
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
            return this;
        }

    }

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
        /// <param name="obj"></param>
        /// <param name="slotName">[Autowired] Specifies the slot name</param>
        void InjectProvider<TObject>(TObject obj, String slotName = null);


        /// <summary>
        /// Gets a delegated weak reference to a script method
        /// Note: Script Unload is blocked when external references to reference objects inside the script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        WeakReference<T> MethodDelegate<T>(String scriptName, String methodName) where T : Delegate;



        /// <summary>
        /// Gets a weak reference to the tripartite interface implemented by the script
        /// Note: Script Unload is blocked when external references to reference objects inside the script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        WeakReference<T> ScriptAs<T>(String scriptName) where T : class;


        /// <summary>
        /// Gets a weak reference to the tripartite interface implemented by any script
        /// Note: Script Unload is blocked when external references to reference objects inside the script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        WeakReference<T> ScriptAs<T>() where T : class;



        /// <summary>
        /// Gets a weak reference to the script's property Getter delegate
        /// Note: Script Unload is blocked when external references to reference objects inside the script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        WeakReference<Getter<T>> PropertyGetterDelegate<T>(String scriptName, String propertyName);

        /// <summary>
        /// Gets the script's property Setter delegate weak reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        WeakReference<Setter<T>> PropertySetterDelegate<T>(String scriptName, String propertyName);


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
    internal class MagnetState : IMagnetState
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
            this.CreateState();
        }


        private void CreateState()
        {
            foreach (var meta in this._engine.scriptMetaInfos)
            {
                var instance = (AbstractScript)Activator.CreateInstance(meta.ScriptType);
                this._stateContext.AddInstance(meta, instance);
                this._stateContext.RegisterProviderInternal(meta.ScriptType, instance);
                _analyzers.DefineInstance(meta, instance, _stateContext);
            }
            //Injected Data
            foreach (var item in this._stateContext.Instances2)
            {
                ((IScriptInstance)item.Instance).InjectedContext(this._stateContext);
                this._stateContext.Autowired(item.Instance, item.Metadata);
            }
            // Exec Init Function
            foreach (var instance in this._stateContext.Instances)
            {
                instance.Initialize();
            }
        }



        public void InjectProvider(IReadOnlyDictionary<Type, Object> objectMap)
        {
            this._stateContext.Autowired(objectMap);
        }


        public void InjectProvider<TObject>(TObject obj, String slotName = null)
        {
            this._stateContext.Autowired<TObject>(obj, slotName);
        }



        public WeakReference<T> MethodDelegate<T>(String scriptName, String methodName) where T : Delegate
        {
            var _delegate = this._stateContext.GetScriptMethod<T>(scriptName, methodName);
            return _delegate != null ? new WeakReference<T>(_delegate) : null;
        }


        public WeakReference<T> ScriptAs<T>(String scriptName) where T : class
        {
            var _object = this._stateContext.ScriptAs<T>(scriptName);
            return _object != null ? new WeakReference<T>(_object) : null;
        }




        public WeakReference<T> ScriptAs<T>() where T : class
        {
            var _object = this._stateContext.ScriptAs<T>();
            return _object != null ? new WeakReference<T>(_object) : null;
        }


        public WeakReference<Getter<T>> PropertyGetterDelegate<T>(String scriptName, String propertyName)
        {
            var getter = this._stateContext.GetScriptPropertyGetter<T>(scriptName, propertyName);
            return getter != null ? new WeakReference<Getter<T>>(getter) : null;
        }


        public WeakReference<Setter<T>> PropertySetterDelegate<T>(String scriptName, String propertyName)
        {
            var setter = this._stateContext.GetScriptPropertySetter<T>(scriptName, propertyName);
            return setter != null ? new WeakReference<Setter<T>>(setter) : null;
        }


        public object GetFieldValue(string scriptName, string variableName)
        {
            AbstractScript script = this._stateContext.InstanceOfName(scriptName);
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
            AbstractScript script = this._stateContext.InstanceOfName(scriptName);
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
