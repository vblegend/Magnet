using System;
using System.Collections.Generic;

namespace App.Core.Timer
{
    /// <summary>
    /// Simulate Timer Service
    /// </summary>
    public class TimerService
    {
        private Dictionary<Int32, System.Threading.Timer> _timers = new Dictionary<int, System.Threading.Timer>();



        public void Enable(Int32 timerIndex, Action callback, UInt32 interval)
        {
            if (!_timers.ContainsKey(timerIndex))
            {
                var timer = new System.Threading.Timer((e) => callback());
                timer.Change(0, interval * 1000);
                _timers.Add(timerIndex, timer);
            }
        }

        public void Disable(Int32 timerIndex)
        {
            if (_timers.TryGetValue(timerIndex, out var timer))
            {
                _timers.Remove(timerIndex);
                timer.Dispose();
            }
        }

        public void Clear()
        {
            foreach (var key in _timers.Keys)
            {
                var timer = _timers[key];
                _timers.Remove(key);
                timer.Dispose();
            }



        }

    }
}
