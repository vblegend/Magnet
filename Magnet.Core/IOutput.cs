

namespace Magnet.Core
{


    /// <summary>
    /// Output message type
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// print message
        /// </summary>
        Print = 0,

        /// <summary>
        /// debug message
        /// </summary>
        Debug = 1,

        /// <summary>
        /// warning message
        /// </summary>
        Warning = 2,

        /// <summary>
        /// error message
        /// </summary>
        Error = 3,

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IOutput
    {
        /// <summary>
        /// Writes a message to the output stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        void Write(MessageType type, string message);


        /// <summary>
        /// Writes a message to the output stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="arg"></param>
        void Write(MessageType type, string format, params object[] arg);
    }
}
