using System;


namespace App.Core.Timer
{
    public enum Duration : uint
    {
        Second = 1,
        Minute = 60,
        Hour = Minute * 60,
        Day = Hour * 24,
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TimerAttribute : Attribute
    {
        public TimerAttribute(ushort timerIndex, ushort interval, Duration unit = Duration.Second)
        {
            TimerIndex = timerIndex;
            Unit = unit;
            Interval = interval;
        }

        public ushort TimerIndex { get; private set; }

        public Duration Unit { get; private set; }

        public ushort Interval { get; private set; }
    }
}
