using System;


namespace Magnet.Core
{
    public class ScriptRunException : Exception
    {
        public String CallerFileName { get; private set; }
        public Int32 CallerLineNumber { get; private set; }
        public String CallerMethodName { get; private set; }


        public ScriptRunException(string? message, String fileName, Int32 lineNumber, String methodName) : base(message)
        {
            this.CallerLineNumber = lineNumber;
            this.CallerMethodName = methodName;
            this.CallerFileName = fileName;
        }


        public ScriptRunException(string? message) : base(message)
        {

        }

        public ScriptRunException(string? message, String fileName, Int32 lineNumber, String methodName, Exception? innerException) : base(message, innerException)
        {
            this.CallerLineNumber = lineNumber;
            this.CallerMethodName = methodName;
            this.CallerFileName = fileName;
        }


        public ScriptRunException(string? message, Exception? innerException) : base(message, innerException)
        {

        }



        public override string ToString()
        {
            return $"{CallerFileName}({CallerLineNumber}) [{CallerMethodName}] {base.ToString()}";
        }

    }
}
