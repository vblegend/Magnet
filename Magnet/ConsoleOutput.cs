using Magnet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magnet
{
    public class ConsoleOutput : IOutput
    {
        public void Write(MessageType type, string message)
        {
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Red;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(message);
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Gray;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Gray;



        }

        public void Write(MessageType type, string format, params object[] arg)
        {


            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Red;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(format, arg);
            if (type == MessageType.Error) Console.ForegroundColor = ConsoleColor.Black;
            if (type == MessageType.Warning) Console.ForegroundColor = ConsoleColor.Black;
        }
    }
}
