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
            Console.WriteLine($"Task:{this.name} use {this.stopwatch.ElapsedMilliseconds}ms");
            this.stopwatch = null;
        }
    }
}
