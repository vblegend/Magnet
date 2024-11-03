using Magnet.Core;
using System;
using System.Diagnostics;

namespace ScriptRuner
{
    internal class WatchTimer : IDisposable
    {
        private Int64 startTimestamp;
        private String name;
        private IOutput _output;

        public WatchTimer(String name, IOutput output = null)
        {
            this.name = name;
            this.startTimestamp = Stopwatch.GetTimestamp();
        }



        private String ToTimeUnit(Int64 elapsed)
        {
            double frequency = Stopwatch.Frequency;
            var seconds = elapsed / frequency;
            if (seconds >= 1) return seconds + " Seconds";
            var milliseconds = elapsed / (frequency / 1000);
            if (milliseconds >= 1) return milliseconds + " Milliseconds";
            var microseconds = elapsed / (frequency / 1_000_000);
            if (microseconds >= 1) return microseconds + " Microseconds";
            var nanoseconds = elapsed / (frequency / 1_000_000_000);
            if (nanoseconds >= 1) return nanoseconds + " Nanoseconds";
            return "";
        }


        public void Dispose()
        {
            var elapsed = Stopwatch.GetTimestamp() - startTimestamp;
            var value = ToTimeUnit(elapsed);
            if (_output != null)
            {
                _output.Write(MessageType.Debug, "Task: {0} use {1}", name, value);
                return;
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("Task: ");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write(this.name);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" use ");
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Write(value);
            Console.ResetColor();
            Console.WriteLine();

        }
    }
}
