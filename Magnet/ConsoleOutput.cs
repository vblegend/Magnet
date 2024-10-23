using Magnet.Core;
using System;

namespace Magnet
{
    internal class ConsoleOutput : IOutput
    {
        public void Write(MessageType type, string message)
        {
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Red;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void Write(MessageType type, string format, params object[] arg)
        {
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Red;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));
            Console.WriteLine(format, arg);
            Console.ResetColor();
        }
    }
}
