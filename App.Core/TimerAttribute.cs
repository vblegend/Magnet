using Magnet.Core.Attributes;
using System;


namespace App.Core
{



    public enum Duration
    {
        Second = 1,
        Minute = 60,
        Hour = Minute * 60,
        Day =  Hour * 24,
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TimerAttribute : CustomAttribute
    {
        public TimerAttribute(UInt16 timerIndex, UInt16 interval, Duration unit = Duration.Second)
        {
            this.TimerIndex = timerIndex;
            this.Unit = unit;
            this.Interval = interval;
        }

        public UInt16 TimerIndex { get; private set; }

        public Duration Unit { get; private set; }

        public UInt16 Interval { get; private set; }



    }
}
