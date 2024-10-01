using System.Diagnostics;
using System.Reflection;

namespace Magnet.Core
{

    /// <summary>
    /// Base script object, script object needs to inherit this type, provides some basic scripting mechanisms
    /// </summary>
    public abstract class BaseScript : IScriptInstance
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
            get;
            private set;
        }


        void IScriptInstance.InjectedContext(IStateContext stateContext)
        {
            this.stateContext = stateContext;
            this.debugger = this.stateContext.UseDebuggerBreak ? Debugger.Break : () => { };
        }



        void IScriptInstance.Initialize()
        {
            this.Initialize();
        }


        protected virtual void Initialize()
        {
        }


        protected void PRINT(string format, params object?[] args)
        {
            PRINT(String.Format(format, args));
        }

        protected void PRINT(String message)
        {
            Console.WriteLine(message);
        }

        protected void DEBUG(String message)
        {
            if (stateContext.RunMode != ScriptRunMode.Debug) return;
            StackTrace stackTrace = new StackTrace(1, true);
            StackFrame callerFrame = stackTrace.GetFrame(0);
            var method = callerFrame.GetMethod();
            var methodName = method.Name;
            var className = method.DeclaringType.FullName;
            var fileName = callerFrame.GetFileName();
            var lineNumber = callerFrame.GetFileLineNumber();
            Console.WriteLine($"{fileName}({lineNumber}) [{className}.{methodName}] => {message}");
        }

        protected void DEBUG(string format, params object?[] args)
        {
            if (stateContext.RunMode != ScriptRunMode.Debug) return;
            DEBUG(String.Format(format, args));
        }

        public void CALL(String scriptName, String method, params Object[] objects)
        {
            var script = stateContext.InstanceOfName(scriptName);
            script.GetType().GetMethod(method).Invoke(script, objects);
        }

        public void TRY_CALL(String scriptName, String method, params Object[] objects)
        {
            var script = stateContext.InstanceOfName(scriptName);
            if (script != null)
            {
                try
                {
                    script.GetType().GetMethod(method).Invoke(script, objects);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public T? SCRIPT<T>() where T : BaseScript
        {
            return stateContext.InstanceOfType<T>();
        }

        public void SCRIPT<T>(Action<T> callback) where T : BaseScript
        {
            var script = SCRIPT<T>();
            if (script != null) callback(script);
        }


    }
}
