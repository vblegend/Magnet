using Magnet.Core;
using System;

namespace Magnet
{
    public class ConsoleOutput : IOutput
    {
        public void Write(MessageType type, string message)
        {
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Red;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));
            Console.WriteLine(message);
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Gray;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Write(MessageType type, string format, params object[] arg)
        {
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Red;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));
            Console.WriteLine(format, arg);
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Black;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Black;
        }
    }
}
