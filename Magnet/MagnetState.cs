using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Magnet
{
    public class MagnetState : IDisposable
    {
        private MagnetScript engine;
        private MagnetStateContext stateContext;
        public event Action<MagnetState> Unloading;
        public readonly Int64 Identity;
        private Dictionary<string, Delegate> delegateCache = new Dictionary<string, Delegate>();


        internal MagnetState(MagnetScript engine, Int64 identity)
        {
            this.Identity = identity;
            this.engine = engine;
            this.stateContext = new MagnetStateContext(engine);
            this.CreateState();
        }


        private void CreateState()
        {
            foreach (var meta in this.engine.scriptMetaInfos)
            {
                var instance = (AbstractScript)Activator.CreateInstance(meta.ScriptType);
                this.stateContext.AddInstance(meta, instance);
                this.stateContext.Autowired(meta.ScriptType, instance, engine.Options.InjectedObjectMap);
            }
            //Injected Data
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


        /// <summary>
        /// Batch inject objects into all script instances
        /// </summary>
        /// <param name="objectMap"></param>
        public void Inject(IReadOnlyDictionary<Type, Object> objectMap)
        {
            this.stateContext.Autowired(objectMap);
        }



        /// <summary>
        /// Inject objects into all script instances
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="obj"></param>
        /// <param name="slotName">[Autowired] Specifies the slot name</param>
        public void Inject<TObject>(TObject obj, String slotName = null)
        {
            this.stateContext.Autowired<TObject>(obj,slotName);
        }


        /// <summary>
        /// Gets a delegated weak reference to a script method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public WeakReference<T> MethodDelegate<T>(String scriptName, String methodName) where T : Delegate
        {
            var _delegate = this.stateContext.GetScriptMethod<T>(scriptName, methodName);
            return _delegate != null ? new WeakReference<T>(_delegate) : null;
        }


        /// <summary>
        /// Gets a weak reference to the tripartite interface implemented by the script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public WeakReference<T> ScriptAs<T>(String scriptName) where T : class
        {
            var _object = this.stateContext.ScriptAs<T>(scriptName);
            return _object != null ? new WeakReference<T>(_object) : null;
        }



        /// <summary>
        /// Gets a weak reference to the tripartite interface implemented by any script
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public WeakReference<T> ScriptAs<T>() where T : class
        {
            var _object = this.stateContext.ScriptAs<T>();
            return _object != null ? new WeakReference<T>(_object) : null;
        }





        /// <summary>
        /// Gets a weak reference to the script's property Getter delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public WeakReference<Getter<T>> PropertyGetterDelegate<T>(String scriptName, String propertyName)
        {
            var key = $"get {scriptName}.{propertyName}()";
            if (delegateCache.TryGetValue(key, out var @delegate))
            {
                return new WeakReference<Getter<T>>((Getter<T>)@delegate);
            }
            else
            {
                AbstractScript script = this.stateContext.InstanceOfName(scriptName);
                if (script != null)
                {
                    // 获取对象的类型
                    Type type = script.GetType();
                    // 获取方法信息 (MethodInfo)
                    PropertyInfo propertyInfo = type.GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        // 创建一个 Delegate 并绑定到 obj 对象
                        var getter = (Getter<T>)Delegate.CreateDelegate(typeof(Getter<T>), script, propertyInfo.GetMethod);
                        delegateCache.Add(key, getter);
                        return new WeakReference<Getter<T>>(getter);
                    }

                }
            }
            return null;
        }

        /// <summary>
        /// Gets the script's property Setter delegate weak reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public WeakReference<Setter<T>> PropertySetterDelegate<T>(String scriptName, String propertyName)
        {
            var key = $"set {scriptName}.{propertyName}()";
            if (delegateCache.TryGetValue(key, out var @delegate))
            {
                return new WeakReference<Setter<T>>((Setter<T>)@delegate);
            }
            else
            {
                AbstractScript script = this.stateContext.InstanceOfName(scriptName);
                if (script != null)
                {
                    // 获取对象的类型
                    Type type = script.GetType();
                    // 获取方法信息 (MethodInfo)
                    PropertyInfo propertyInfo = type.GetProperty(propertyName);
                    if (propertyInfo != null)
                    {
                        // 创建一个 Delegate 并绑定到 obj 对象
                        var setter = (Setter<T>)Delegate.CreateDelegate(typeof(Setter<T>), script, propertyInfo.SetMethod);
                        delegateCache.Add(key, setter);
                        return new WeakReference<Setter<T>>(setter);
                    }

                }
            }
            return null;
        }






        /// <summary>
        /// Get the value of the field in the script (not recommended)
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object GetFieldValue(string scriptName, string variableName)
        {
            AbstractScript script = this.stateContext.InstanceOfName(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                var field = type.GetField(variableName);
                return field?.GetValue(script);
            }
            throw new Exception("Variable not found");
        }


        /// <summary>
        /// Set the value of the field in the script (not recommended)
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public void SetFieldValue(string scriptName, string variableName, object value)
        {
            AbstractScript script = this.stateContext.InstanceOfName(scriptName);
            if (script != null)
            {
                Type type = script.GetType();
                var field = type.GetField(variableName);
                field?.SetValue(script, value);
            }
        }



        public void Dispose()
        {
            this.delegateCache?.Clear();
            this.delegateCache = null;
            this.Unloading?.Invoke(this);
            this.Unloading = null;
            this.stateContext?.Dispose();
            this.stateContext = null;
            this.engine = null;
        }
    }
}
