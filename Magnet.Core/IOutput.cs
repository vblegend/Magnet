

namespace Magnet.Core
{


    public enum MessageType
    {
        Print = 0,
        Debug = 1,
        Warning = 2,
        Error = 3,





    }


    public interface IOutput
    {
        void Write(MessageType type, string message);

        void Write(MessageType type, string format, params object[] arg);
    }
}
