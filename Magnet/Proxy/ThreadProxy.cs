using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Magnet.Proxy
{
    public class ThreadProxy
    {
        public ThreadProxy(ThreadStart threadStart)
        {
            
        }
        public static void Sleep(int millisecondsTimeout)
        {
            Console.WriteLine("谁执行了 Sleep?");
        }

        public void Start()
        {
            Console.WriteLine("谁运行了 Start?");
        }


        public static void Sleep(TimeSpan timeout)
        {

        }

        public static Thread CurrentThread
        {
            get
            {
                return Thread.CurrentThread;
            }
        }


    }
}
