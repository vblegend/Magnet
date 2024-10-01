using System.Diagnostics;

namespace ScriptRuner
{
    internal class WitchTimer : IDisposable
    {
        private Stopwatch stopwatch;
        private String name;

        public WitchTimer(String name)
        {
            this.name = name;
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
        }

        public void Dispose()
        {
            if (this.stopwatch == null) throw new Exception();
            this.stopwatch.Stop();
            var f = Console.ForegroundColor;
            var b = Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor= ConsoleColor.Red;


            Console.WriteLine($"Task:{this.name} use {this.stopwatch.ElapsedMilliseconds}ms");
            this.stopwatch = null;



            Console.ForegroundColor = f;
            Console.BackgroundColor = b;
        }
    }
}
