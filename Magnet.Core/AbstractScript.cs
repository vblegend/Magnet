
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Magnet.Core
{


    /// <summary>
    /// Base script object, script object needs to inherit this type, provides some basic scripting mechanisms
    /// </summary>
    public abstract class AbstractScript : IScriptInstance
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IStateContext stateContext;

        
        protected Boolean IsDebuging => stateContext.RunMode == ScriptRunMode.Debug;

        /// <summary>
        /// Enter the debug breakpoint in debug mode
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected Action debugger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }



        void IScriptInstance.InjectedContext(IStateContext stateContext)
        {
            this.stateContext = stateContext;
            if (this.stateContext.UseDebuggerBreak)
            {
                this.debugger = LaunchDebugger;
                this.debugger += Debugger.Break;
            }
            else
            {
                this.debugger = () => { };
            }
        }


        private void LaunchDebugger()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }



        void IScriptInstance.Initialize()
        {
            this.Initialize();
        }


        protected virtual void Initialize()
        {
        }

        public void Output(MessageType type, String message)
        {
            stateContext.Output.Write(type, message);
        }




        /// <summary>
        /// Calls a method of the specified script, passing in the method parameters
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ScriptRunException"></exception>
        public Object? Call(String scriptName, String methodName, Object[] args, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
        {
            var script = stateContext.InstanceOfName(scriptName);
            if (script == null)
            {
                throw new ScriptRunException($"Script {scriptName} was not found when method {methodName} of script {scriptName} was called.", callFilePath, callLineNumber, callMethod);
            }
            var methodInfo = script.GetType().GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new ScriptRunException($"Method {methodName} was not found when method {methodName} of script {scriptName} was called.", callFilePath, callLineNumber, callMethod);
            }
            try
            {
                return methodInfo.Invoke(script, args);
            }
            catch (Exception ex)
            {
                throw new ScriptRunException($"While calling method {methodName} of script {scriptName}, an exception was encountered that could not be handled.", callFilePath, callLineNumber, callMethod, ex);
            }
        }


        /// <summary>
        /// Try Calls a method of the specified script, passing in the method parameters
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Object? TryCall(String scriptName, String methodName, Object[] args, [CallerFilePath] String callFilePath = null, [CallerLineNumber] Int32 callLineNumber = 0, [CallerMemberName] string? callMethod = null)
        {
            var script = stateContext.InstanceOfName(scriptName);
            if (script == null)
            {
                this.Output(MessageType.Warning, $"{callFilePath}({callLineNumber}) [{callMethod}] => TryCall(\"{scriptName}\",\"{methodName}\",??) not found script {scriptName}.");
                return null;
            }
            try
            {
                var methodInfo = script.GetType().GetMethod(methodName);
                if (methodInfo == null)
                {
                    this.Output(MessageType.Warning, $"{callFilePath}({callLineNumber}) [{callMethod}] => TryCall(\"{scriptName}\",\"{methodName}\",??) not found method {methodName}.");
                    return null;
                }
                return methodInfo.Invoke(script, args);
            }
            catch (Exception ex)
            {
                this.Output(MessageType.Warning, $"{callFilePath}({callLineNumber}) [{callMethod}] => TryCall(\"{scriptName}\",\"{methodName}\",??) an exception was encountered that could not be handled.");
                return null;
            }

        }

        public T? Script<T>() where T : AbstractScript
        {
            return stateContext.InstanceOfType<T>();
        }

        public void Script<T>(Action<T> callback) where T : AbstractScript
        {
            var script = Script<T>();
            if (script != null) callback(script);
        }


    }
}
