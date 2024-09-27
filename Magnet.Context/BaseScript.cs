using System.Diagnostics;
using System.Reflection;

namespace Magnet.Context
{
    public abstract class BaseScript
    {
        private IScriptCollection scriptCollection;

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
            DEBUG(String.Format(format, args));
        }

        public void CALL(String scriptName, String method, params Object[] objects)
        {
            var script = scriptCollection.NameOf(scriptName);
            script.GetType().GetMethod(method).Invoke(script, objects);
        }
        public void TRY_CALL(String scriptName, String method, params Object[] objects)
        {
            var script = scriptCollection.NameOf(scriptName);
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
            return scriptCollection.TypeOf<T>();
        }

        public void SCRIPT<T>(Action<T> callback) where T : BaseScript
        {
            var script = SCRIPT<T>();
            if (script != null) callback(script);
        }

    }
}
