using System;


namespace App.Core.Timer
{
    public interface ITimerService
    {
        public void Enable(Int32 timerIndex, Action callback, UInt32 interval);
        public void Disable(Int32 timerIndex);
        public void Clear();
    }
}
