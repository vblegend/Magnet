using System;


namespace Magnet.Core
{

    /// <summary>
    /// Script run error
    /// </summary>
    public class ScriptRunException : Exception
    {
        /// <summary>
        /// Caller file name
        /// </summary>
        public String CallerFileName { get; private set; }

        /// <summary>
        /// Caller file line number
        /// </summary>
        public Int32 CallerLineNumber { get; private set; }

        /// <summary>
        /// Caller method name
        /// </summary>
        public String CallerMethodName { get; private set; }

        internal ScriptRunException(string message) : base(message)
        {

        }

        internal ScriptRunException(string message, String fileName, Int32 lineNumber, String methodName) : base(message)
        {
            this.CallerLineNumber = lineNumber;
            this.CallerMethodName = methodName;
            this.CallerFileName = fileName;
        }

        internal ScriptRunException(string message, String fileName, Int32 lineNumber, String methodName, Exception innerException) : base(message, innerException)
        {
            this.CallerLineNumber = lineNumber;
            this.CallerMethodName = methodName;
            this.CallerFileName = fileName;
        }


        internal ScriptRunException(string message, Exception innerException) : base(message, innerException)
        {

        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{CallerFileName}({CallerLineNumber}) [{CallerMethodName}] {base.ToString()}";
        }

    }
}
